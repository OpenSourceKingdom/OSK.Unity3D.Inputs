using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Services
{
    internal class UnityInputSystemReader : IInputReader
    {
        #region Variables

        private readonly InputDeviceIdentifier _deviceIdentifier;
        private readonly Dictionary<string, UnityInput> _inputControlsLookup;

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

        public event Action<InputDeviceIdentifier> OnControllerDisconnected;
        public event Action<InputDeviceIdentifier> OnControllerReconnected;

        public void Dispose()
        {
        }

        public Task ReadInputsAsync(UserInputReadContext context, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var activeInputActionPairs = context.InputActionPairs
                .Select(inputActionPair =>
                {
                    InputPhase currentPhase = InputPhase.Start;

                    return new
                    {
                        IsTriggered = _inputControlsLookup.TryGetValue(inputActionPair.InputName, out var unityInput)
                            && unityInput.TryGetInputPhase(out currentPhase) 
                            && inputActionPair.TriggerPhase.HasFlag(currentPhase),
                        UnityInput = unityInput,
                        CurrentPhase = currentPhase,
                        Pair = inputActionPair
                    };
                })
                .Where(v => v.IsTriggered);

            foreach (var activeInputActionPair in activeInputActionPairs)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(cancellationToken);
                }

                ActivateInput(context, activeInputActionPair.CurrentPhase, activeInputActionPair.Pair,
                    _inputControlsLookup[activeInputActionPair.Pair.InputName]);
            }

            return Task.CompletedTask;
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
        private Dictionary<string, UnityInput> CreateInputControllerLookup(InputDeviceIdentifier deviceIdentifier,
            IEnumerable<IInput> inputs) 
        {
            var device = InputSystem.devices.FirstOrDefault(inputDevice => inputDevice.deviceId == deviceIdentifier.DeviceId);
            if (device is null)
            {
                return new Dictionary<string, UnityInput>();
            }

            var unityControllerLookup = new Dictionary<string, UnityInput>();
            var deviceInputLookup = device.allControls.GroupBy(control => control.displayName)
                .Select(controlGroup => controlGroup.First())
                .ToDictionary(control => control.displayName);
            foreach (var input in inputs)
            {
                var inputKey = input.GetUnityInputName();
                if (!deviceInputLookup.TryGetValue(inputKey, out var inputControl))
                {
                    var invalidInputs = inputs.Where(i => !deviceInputLookup.TryGetValue(i.GetUnityInputName(), out _)).ToList();
                    Debug.Log("The following inputs did not have matches: " + string.Join(", ", invalidInputs.Select(i => i.Name)));
                    throw new InvalidOperationException($"The expected input {input.Name} was not found in the device input lookup");
                }

                unityControllerLookup[inputKey] = input switch
                {
                    MouseScrollInput scrollInput => new UnityDeltaInput(scrollInput, (DeltaControl) inputControl),
                    AnalogInput analogInput => new UnityStickInput(analogInput, (StickControl) inputControl),
                    HardwareInput hardwareInput => new UnityButtonInput(hardwareInput, (ButtonControl) inputControl),
                    TouchInput touchInput => new UnityTouchInput(touchInput, (TouchControl) inputControl),
                    SensorInput sensorInput => new UnitySensorInput(sensorInput, (Sensor)  inputControl),
                    _ => throw new InvalidOperationException($"A valid input to unity input mapping did not exist for the input {input.Name} which is of type {input.GetType().FullName}.^")
                };
            }

            return unityControllerLookup;
        }

        private void ActivateInput(UserInputReadContext context, InputPhase triggerPhase,
            InputActionMapPair inputActionMapPair, UnityInput input)
        {
            var pointerInformation = GetPointerLocation(input);
            switch (input)
            {
                case UnityButtonInput:
                    context.ActivateInput(inputActionMapPair, triggerPhase, pointerInformation.CurrentPosition);
                    break;
                case UnityStickInput stickInput:
                    var axisInputPowers = stickInput.InputControl.ReadValue();
                    context.ActivateInput(inputActionMapPair, triggerPhase, pointerInformation.CurrentPosition,
                        new float[] { axisInputPowers.x, axisInputPowers.y });
                    break;
                case UnitySensorInput sensorInput:
                    ActivateSensorInput(context, triggerPhase, inputActionMapPair, pointerInformation, sensorInput);
                    break;
                case UnityTouchInput touchInput:
                    context.ActivatePointerInput(inputActionMapPair, triggerPhase, pointerInformation,
                        new InputPower(new float[] { touchInput.InputControl.pressure.value }));
                    break;
                default:
                    throw new InvalidOperationException($"No mapping was configured to convert an active input of type {input.GetType().FullName} to an activated input data object.");
            }
        }

        private void ActivateSensorInput(UserInputReadContext context, InputPhase triggerPhase,
            InputActionMapPair inputActionMapPair, PointerInformation pointerInformation, UnitySensorInput sensorInput)
        {
            switch (sensorInput.InputControl)
            {
                case Accelerometer accelerometer:
                    context.ActivateInput(inputActionMapPair, triggerPhase, pointerInformation.CurrentPosition, 
                        new float[]
                        {
                            accelerometer.acceleration.value.x, 
                            accelerometer.acceleration.value.y, 
                            accelerometer.acceleration.value.z
                        });
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

        #endregion
    }  
}
