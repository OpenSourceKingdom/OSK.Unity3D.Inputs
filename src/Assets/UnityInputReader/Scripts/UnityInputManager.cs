using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Ports;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Attributes;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    public class UnityInputManager: MonoBehaviour, IContainerBehaviour
    {
        #region Variables

        private bool _initialized = false;

        private bool _isInputPaused;
        [SerializeField]
        private bool _blockerPointerWithUI;

        private IInputManager _inputManager;
        private IUnityInputProcessor _inputProcessor;

        [SerializeField]
        private InputReadOptions _readOptions = new InputReadOptions()
        {
            DeviceReadTime = TimeSpan.FromMilliseconds(10),
            MaxConcurrentDevices = 2,
            RunInputUsersInParallel = true
        };

        #endregion

        #region MonoBehvaiour Overrides

        private async void Start()
        {
            await InitializePlayers(1);
        }

        private async void Update()
        {
            if (!_initialized || IsInputPaused)
            {
                return;
            }

            var activationContext = await _inputManager.ReadInputsAsync(_readOptions);
            await activationContext.ExecuteCommandsAsync((next, activationEvent) =>
            {
                Console.WriteLine("Input received!");
                Console.WriteLine($"Device: {activationEvent.Input.DeviceName} Phase: {activationEvent.Input.TriggeredPhase} Action: {activationEvent.Input.ActionKey} Input Name: {activationEvent.Input.Input.Name}");
                return next(activationEvent);
            });
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
        private void Initialize(IInputManager inputManager, IUnityInputProcessor inputProcessor)
        {
            _inputManager = inputManager;
            _inputProcessor = inputProcessor;

            _inputManager.OnInputDeviceAdded += OnInputDeviceAdded;
            _inputManager.OnInputDeviceReconnected += OnInputDeviceReconnected;
            _inputManager.OnInputDeviceDisconnected += OnInputDeviceDisconnected;
        }

        #endregion
    }
}
