using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Attributes;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Ports;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    public class UnityInputManager: MonoBehaviour, IContainerBehaviour
    {
        #region Variables

        private bool _initialized = false;
        private bool _isRunning = false;

        private bool _isInputPaused;
        [SerializeField]
        private bool _blockerPointerWithUI;

        private IInputManager _inputManager;

        #endregion

        #region MonoBehvaiour Overrides

        private async void Start()
        {
            await InitializePlayers(1);
            _initialized = true;
        }

        private async void Update()
        {
            if (!_initialized || IsInputPaused)
            {
                return;
            }
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            var activationContext = await _inputManager.ReadInputsAsync(InputReadOptions.SingleThreaded);
            await activationContext.ExecuteCommandsAsync((next, activationEvent) =>
            {
                Debug.Log("Input received!");
                Debug.Log($"Device: {activationEvent.Input.DeviceName} Phase: {activationEvent.Input.TriggeredPhase} Action: {activationEvent.InputAction.ActionKey} Input Name: {activationEvent.Input.Input.Name}");
                return next(activationEvent);
            });

            _isRunning = false;
        }

        #endregion

        #region API

        public event Action<UnityInputActivationContext> OnInputActivated;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceAdded;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceReconnected;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceDisconnected;

        public bool IsInputPaused
        {
            get => _isInputPaused;
            set => _isInputPaused = value;
        }

        public bool BlockPointerWithUI
        {
            get => _blockerPointerWithUI;
            set => _blockerPointerWithUI = value;
        }

        public void JoinPlayer(int playerId, InputDevice device)
        {
            _inputManager.JoinUserAsync(playerId, new JoinUserOptions()
            {
                DeviceIdentifiers = new InputDeviceIdentifier[] { device.GetDeviceIdentifier() }
            }); 
        }

        public void RemovePlayer(int playerId)
        {
            _inputManager.RemoveUser(playerId);
        }

        public async Task InitializePlayers(int playerCount)
        {
            if (playerCount == 1)
            {
                var deviceIdentifiers = InputSystem.devices.Select(device => device.GetDeviceIdentifier());
                await _inputManager.JoinUserAsync(playerCount, new JoinUserOptions()
                {
                    DeviceIdentifiers = deviceIdentifiers
                });

                return;
            }
        }

        #endregion

        #region Helpers

        [ContainerInject]
        private void Initialize(IInputManager inputManager)
        {
            _inputManager = inputManager;

            _inputManager.OnInputDeviceAdded += OnInputDeviceAdded;
            _inputManager.OnInputDeviceReconnected += OnInputDeviceReconnected;
            _inputManager.OnInputDeviceDisconnected += OnInputDeviceDisconnected;
        }

        #endregion
    }
}
