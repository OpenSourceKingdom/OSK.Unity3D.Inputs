using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Services
{
    internal class UnityInputSystemReader : IInputDeviceReader
    {
        #region Variables

        private readonly InputDeviceIdentifier _deviceIdentifier;
        private readonly Dictionary<int, UnityInput[]> _inputControlsLookup;

        #endregion

        #region Constructors

        public UnityInputSystemReader(InputReaderParameters parameters)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            _deviceIdentifier = parameters.DeviceIdentifier;
            _inputControlsLookup = CreateInputControllerLookup(parameters.DeviceIdentifier, parameters.Inputs);
        }

        #endregion

        #region IInputReader

        public event Action<InputDeviceIdentifier> OnDeviceDisconnected;
        public event Action<InputDeviceIdentifier> OnDeviceConnected;

        public void Dispose()
        {
            InputSystem.onDeviceChange -= OnHandleInputDeviceEvent;
        }

        public ValueTask ReadInputAsync(DeviceInputReadContext context, IInput input, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return UnityValueTasks.CompletedTask;
            }

            if (_inputControlsLookup.TryGetValue(input.Id, out var unityInputs))
            {
                var activeInput = unityInputs.Select(unityInput => new
                {
                    unityInput,
                    IsActiveInput = unityInput.TryGetInputPhase(out var activePhase),
                    Phase = activePhase
                })
                .FirstOrDefault(unityInput => unityInput.IsActiveInput);

                if (activeInput is not null)
                {
                    // Don't need to check all the inputs if the input is triggered for of the same keys (i.e. left and right shift for shift)
                    SetInputState(context, activeInput.unityInput, activeInput.Phase);
                }
                else
                {
                    context.SetInputState(input, InputPhase.Idle);
                }
            }

            return UnityValueTasks.CompletedTask;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// This method takes an input device identifier and converts it into a supported unity input device.
        /// See https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/SupportedDevices.html for more information
        /// </summary>
        /// <param name="deviceIdentifier">The input device identifier that will be used to retrieve the respective unity device for input reading</param>
        /// <param name="inputs">The actual inputs that are being read by the input manager</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Dictionary<int, UnityInput[]> CreateInputControllerLookup(InputDeviceIdentifier deviceIdentifier,
            IEnumerable<IInput> inputs) 
        {
            var device = InputSystem.devices.FirstOrDefault(inputDevice => inputDevice.deviceId == deviceIdentifier.DeviceId);
            if (device is null)
            {
                return new Dictionary<int, UnityInput[]>();
            }

            InputSystem.onDeviceChange += OnHandleInputDeviceEvent;

            var deviceInputLookup = new Dictionary<int, UnityInput[]>();
            var inputControlLookup = device.allControls.GroupBy(control => control.displayName)
                .ToDictionary(controlGroup => controlGroup.Key, controlGroup => controlGroup.ToArray());

            foreach (var input in inputs.OfType<HardwareInput>())
            {
                var unityInputs = GetUnityInputs(inputControlLookup, input);
                deviceInputLookup[input.Id] = unityInputs.ToArray();
            }

            return deviceInputLookup;
        }

        private IEnumerable<UnityInput> GetUnityInputs(Dictionary<string, InputControl[]> inputControlLookup, IInput deviceInput)
        {
            var unityInputNames = deviceInput.GetUnityInputNames();
            foreach (var unityInputName in unityInputNames)
            {
                var keyParts = unityInputName == "/" 
                    ? new string[] { unityInputName }
                    : unityInputName.Split("/");
                if (!inputControlLookup.TryGetValue(keyParts[0], out var inputControlGroup))
                {
                    Debug.Log($"The expected input key, {unityInputName}, for input {deviceInput.Name} was not found in the device input lookup");
                    continue;
                }
                
                for (var i = 0; i < inputControlGroup.Length; i++)
                {
                    var input = inputControlGroup[i];
                    if (input is DpadControl dpadControl && keyParts.Length > 1)
                    {
                        inputControlGroup[i] = keyParts[1] switch
                        {
                            "left" => dpadControl.left,
                            "right" => dpadControl.right,
                            "up" => dpadControl.up,
                            "down" => dpadControl.down,
                            _ => inputControlGroup[i]
                        };
                    }
                }

                var inputControl = inputControlGroup.First();
                yield return deviceInput switch
                {
                    MouseScrollInput scrollInput => new UnityDeltaInput(scrollInput, (DeltaControl)inputControl),
                    AnalogInput analogInput => new UnityStickInput(analogInput, (StickControl)inputControl),
                    HardwareInput hardwareInput => new UnityButtonInput(hardwareInput, (ButtonControl)inputControl),
                    TouchInput touchInput => new UnityTouchInput(touchInput, (TouchControl)inputControl),
                    SensorInput sensorInput => new UnitySensorInput(sensorInput, (Sensor)inputControl),
                    _ => throw new InvalidOperationException($"A valid input to unity input mapping did not exist for the input {deviceInput.Name} which is of type {deviceInput.GetType().FullName}.^")
                };
            }
        }
        private void SetInputState(DeviceInputReadContext context, UnityInput input, InputPhase triggeredPhase)
        {
            var pointerInformation = GetPointerLocation(input);
            switch (input)
            {
                case UnityButtonInput:
                    context.SetInputState(input.DeviceInput, triggeredPhase, pointerInformation.CurrentPosition);
                    break;
                case UnityStickInput stickInput:
                    var axisInputPowers = stickInput.InputControl.ReadValue();
                    context.SetInputState(input.DeviceInput, triggeredPhase, pointerInformation.CurrentPosition,
                        axisInputPowers.x, axisInputPowers.y);
                    break;
                case UnitySensorInput sensorInput:
                    SetSensorInputState(context, sensorInput, triggeredPhase, pointerInformation);
                    break;
                case UnityTouchInput touchInput:
                    context.SetInputState(input.DeviceInput, triggeredPhase, pointerInformation,
                        touchInput.InputControl.pressure.value);
                    break;
                default:
                    throw new InvalidOperationException($"No mapping was configured to convert an active input of type {input.GetType().FullName} to an activated input data object.");
            }
        }

        private void SetSensorInputState(DeviceInputReadContext context, UnitySensorInput sensorInput, InputPhase triggerPhase,
            PointerInformation pointerInformation)
        {
            switch (sensorInput.InputControl)
            {
                case Accelerometer accelerometer:
                    context.SetInputState(sensorInput.DeviceInput, triggerPhase, pointerInformation.CurrentPosition,
                            accelerometer.acceleration.value.x,
                            accelerometer.acceleration.value.y,
                            accelerometer.acceleration.value.z);
                    break;
                default:
                    throw new NotSupportedException($"Sensor input {sensorInput.InputControl.GetType().FullName} is not currently supported");
            }
        }

        private PointerInformation GetPointerLocation(UnityInput input)
        {
            return input switch
            {
                UnityTouchInput touchInput => new PointerInformation(touchInput.InputControl.touchId.value,
                    new System.Numerics.Vector2[] { touchInput.InputControl.startPosition.value.ToNumericVector() }),
                _ => new PointerInformation(PointerInformation.DefaultPointerId, new System.Numerics.Vector2[] { UnityEngine.InputSystem.Mouse.current.position.value.ToNumericVector() })
            };
        }

        private void OnHandleInputDeviceEvent(InputDevice device, InputDeviceChange change)
        {
            if (device.deviceId != _deviceIdentifier.DeviceId)
            {
                return;
            }

            switch (change)
            {
                case InputDeviceChange.Added:
                    OnDeviceConnected?.Invoke(_deviceIdentifier);
                    break;
                case InputDeviceChange.Disconnected:
                    OnDeviceDisconnected?.Invoke(_deviceIdentifier);
                    break;
                case InputDeviceChange.Reconnected:
                    OnDeviceConnected?.Invoke(_deviceIdentifier);
                    break;
            }
        }

        #endregion
    }  
}
