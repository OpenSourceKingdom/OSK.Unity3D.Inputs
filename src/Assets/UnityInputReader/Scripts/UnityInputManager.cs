using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Attributes;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Ports;
using System;
using System.Collections.Generic;
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
        private IOutputFactory<UnityInputManager> _outputFactory;

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

        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceAdded;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceReconnected;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceDisconnected;

        public bool IsInputPaused
        {
            get => _isInputPaused;
            set => _isInputPaused = value;
        }

        public Player GetPlayer(int playerId)
            => ToPlayer(_inputManager.GetApplicationInputUser(playerId));

        public IEnumerable<Player> GetPlayers()
            => _inputManager.GetApplicationInputUsers().Select(ToPlayer);

        public async Task<IOutput<Player>> JoinPlayerAsync(int playerId, InputDevice device)
        {
            var joinOutput = await _inputManager.JoinUserAsync(playerId, new JoinUserOptions()
            {
                DeviceIdentifiers = new InputDeviceIdentifier[] { device.GetDeviceIdentifier() }
            });
            if (!joinOutput.IsSuccessful)
            {
                return joinOutput.AsOutput<Player>();
            }

            return _outputFactory.Succeed(ToPlayer(joinOutput.Value));
        }

        public void PairDevice(int playerId, InputDeviceIdentifier deviceIdentifier)
            => _inputManager.PairDevice(playerId, deviceIdentifier);

        public void RemovePlayer(int playerId)
            => _inputManager.RemoveUser(playerId);

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

        private Player ToPlayer(IApplicationInputUser applicationInputUser)
        {
            var deviceIdentifiers = applicationInputUser.DeviceIdentifiers.Select(identifier => identifier.DeviceId).ToHashSet();
            return new Player
            {
                Id = applicationInputUser.Id,
                InputDevices = InputSystem.devices.Where(device => deviceIdentifiers.Contains(device.deviceId))
            };
        }

        [ContainerInject]
        private void Initialize(IInputManager inputManager, IOutputFactory<UnityInputManager> outputFactory)
        {
            _inputManager = inputManager;
            _outputFactory = outputFactory;

            _inputManager.OnInputDeviceAdded += OnInputDeviceAdded;
            _inputManager.OnInputDeviceReconnected += OnInputDeviceReconnected;
            _inputManager.OnInputDeviceDisconnected += OnInputDeviceDisconnected;
        }

        #endregion
    }
}
