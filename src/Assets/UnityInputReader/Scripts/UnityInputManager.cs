using Microsoft.Extensions.Options;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Events;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Attributes;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    public class UnityInputManager: MonoBehaviour, IContainerBehaviour
    {
        #region Variables

        private bool _isRunning = false;

        private bool _isInputPaused;

        [SerializeField]
        private bool _overrideInjectedInputOptions;
        [SerializeField]
        private UnityInputSystemOptions _inputOptions;

        private IInputManager _inputManager;
        private IOutputFactory<UnityInputManager> _outputFactory;

        private Dictionary<int, Player> _players = new();

        private readonly static InputReadOptions _singleThreadedCustom = new InputReadOptions()
        {
            DeviceReadTime = TimeSpan.FromMilliseconds(100),
            MaxConcurrentDevices = 1,
            MaxConcurrenUsers = 1,
            RunInputUsersInParallel = false
        };

        #endregion

        #region MonoBehvaiour Overrides

        private async void Start()
        {
            if (_inputOptions.PlayerJoinOptions is not null
                 && _inputOptions.PlayerJoinOptions.DeviceJoinBehavior is DeviceJoinBehavior.Automatic)
            {
                foreach (var unpairedDevice in InputUser.GetUnpairedInputDevices())
                {
                    await PairDeviceToUserAsync(unpairedDevice);
                }
            }
        }

        private void OnDestroy()
        {
            if (_inputOptions.PlayerJoinOptions?.DeviceJoinBehavior is DeviceJoinBehavior.DeviceActivation)
            {
                InputUser.onUnpairedDeviceUsed -= HandleUnpairedDeviceInputReceived;
                InputUser.listenForUnpairedDeviceActivity = 0;
            }
            InputSystem.onDeviceChange -= HandleInputDeviceChange;
        }

        private async void Update()
        {
            if (IsInputPaused)
            {
                return;
            }
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            var activationContext = await _inputManager.ReadInputsAsync(_singleThreadedCustom);
            await activationContext.ExecuteCommandsAsync((next, activationEvent) =>
            {
                Debug.Log("Input received!");
                Debug.Log($"UserId: {activationEvent.UserId} Device: {activationEvent.Input.DeviceName} Phase: {activationEvent.Input.TriggeredPhase} Action: {activationEvent.InputAction.ActionKey} Input Name: {activationEvent.Input.Input.Name}");
                return next(activationEvent);
            });

            _isRunning = false;
        }

        #endregion

        #region API

        public event Action<PlayerJoinedEvent> OnPlayerJoined;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceAdded;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceReconnected;
        public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceDisconnected;

        public bool IsInputPaused
        {
            get => _isInputPaused;
            set => _isInputPaused = value;
        }

        /// <summary>
        /// The total number of local players allowed in the game session
        /// </summary>
        public int TotalLocalPlayersAllowed => _inputManager.Configuration.MaxLocalUsers;

        /// <summary>
        /// Retrieves the <see cref="Player"/> of a given id
        /// </summary>
        /// <param name="playerId">The id for the player to get</param>
        /// <returns>The player</returns>
        public Player GetPlayer(int playerId)
            => _players[playerId];

        /// <summary>
        /// Returns the list of players currently in the local game session.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Player> GetPlayers()
            => _players.Values;

        /// <summary>
        /// Attempts to pair a device to an existing or new player.
        /// 
        /// <br />
        /// <br />
        /// Notes:
        /// <list type="bullet">
        ///     <item>If no existing user is specified, an attempt will be made to find a current user that is missing a needed device, before creating a new user if none are found</item>
        ///     <item>Devices will not be paired if the <see cref="UnityInputSystemOptions.PlayerJoinOptions"/> total controllers per useer is exceeded</item>
        /// </list>
        /// </summary>
        /// <param name="device">The device to pair</param>
        /// <param name="existingUser">A preferred user for the device to pair with</param>
        /// <returns>A task representing the operation</returns>
        public async Task PairDeviceToUserAsync(InputDevice device, InputUser? existingUser = null)
        {
            if (InputUser.FindUserPairedToDevice(device) is not null)
            {
                Debug.LogWarning($"Device {device.displayName} is already paired to a user.");
                return;
            }

            var newDeviceIdentifier = device.TryGetDeviceIdentifier();
            if (newDeviceIdentifier is null)
            {
                Debug.LogError($"Device {device.displayName} does not have a valid device name mapping.");
                return;
            }

            var controller = _inputManager.Configuration.InputControllers.FirstOrDefault(controller => controller.DeviceNames.Any(deviceName => deviceName == newDeviceIdentifier.Value.DeviceName));
            if (controller is null)
            {
                Debug.LogError($"No input controller found for unity device {device.displayName} with device name {newDeviceIdentifier}.");
                return;
            }

            var isNewUser = true;
            InputUser deviceUser;
            if (existingUser is null)
            {
                // Want to find a user that needs the device to complete their current controller
                // Tuple Data: AppUser, AppUser Needs Device to Complete a Controller, Total Valid Controllers AppUser currently possesses
                Tuple<IApplicationInputUser, bool, int> existingAppUserData = null;
                foreach (var appUser in _inputManager.GetApplicationInputUsers())
                {
                    var userDeviceNameLookup = appUser.DeviceIdentifiers.Select(identifier => identifier.DeviceName).ToHashSet();
                    var player = InputUser.all.First(inputUser => inputUser.id == appUser.Id);

                    var totalValidControllers = GetTotalValidInputControllersForUser(player);
                    var needsDeviceForController = controller.DeviceNames.Any(userDeviceNameLookup.Contains) && !userDeviceNameLookup.Contains(newDeviceIdentifier.Value.DeviceName);
                    if ((existingAppUserData is null || totalValidControllers < existingAppUserData.Item3)
                         && needsDeviceForController)
                    {
                        existingAppUserData = new Tuple<IApplicationInputUser, bool, int>(appUser, needsDeviceForController, totalValidControllers);
                    }
                }
                
                // Only need to add a device to an existing app user if that user is currently missing a device for their input controller,
                // otherwise we'll create a new user until we hit the limit for local users at which point we'll proceed to adding new devices to previous users
                if (existingAppUserData is not null && !existingAppUserData.Item2 && _inputManager.GetApplicationInputUsers().Count() < _inputManager.Configuration.MaxLocalUsers)
                {
                    existingAppUserData = null;
                }

                isNewUser = existingAppUserData is null;
                deviceUser = isNewUser
                    ? InputUser.CreateUserWithoutPairedDevices()
                    : InputUser.all.First(player => player.id == existingAppUserData.Item1.Id);
            }
            else
            {
                deviceUser = existingUser.Value;
                isNewUser = false;
            }

            if (_inputOptions.PlayerJoinOptions.MaxInputControllersPerPlayer.HasValue)
            {
                if (GetTotalValidInputControllersForUser(deviceUser) >= _inputOptions.PlayerJoinOptions.MaxInputControllersPerPlayer.Value)
                {
                    Debug.LogWarning($"User {deviceUser.id} already has the maximum number of input controllers ({_inputOptions.PlayerJoinOptions.MaxInputControllersPerPlayer.Value}) assigned. Cannot pair device {device.displayName}.");
                    return;
                }
            }

            deviceUser = InputUser.PerformPairingWithDevice(device, deviceUser);
            if (isNewUser)
            {
                var joinUserOutput = await _inputManager.JoinUserAsync((int)deviceUser.id, new JoinUserOptions()
                {
                    DeviceIdentifiers = new InputDeviceIdentifier[] { newDeviceIdentifier.Value }
                });
                if (!joinUserOutput.IsSuccessful)
                {
                    deviceUser.UnpairDevicesAndRemoveUser();
                    Debug.LogError($"Failed to join user with device {device.displayName}. Error: {joinUserOutput.GetErrorString()}");
                    return;
                }

                var newPlayer = UpsertPlayer(deviceUser);

                Debug.Log($"Device {device.displayName} paired to new user with id {deviceUser.id}.");
                OnPlayerJoined?.Invoke(new PlayerJoinedEvent()
                {
                    NewPlayer = newPlayer
                });
            }
            else
            {
                _inputManager.PairDevice((int)deviceUser.id, newDeviceIdentifier.Value);
                UpsertPlayer(deviceUser);

                Debug.Log($"Device {device.displayName} paired to existing user with id {deviceUser.id}.");
            }
        }

        public async Task UnpairDeviceAsync(InputDevice device)
        {
            var inputUser = InputUser.FindUserPairedToDevice(device);
            if (inputUser.HasValue)
            {
                _inputManager.RemoveUser((int)inputUser.Value.id);
                inputUser.Value.UnpairDevice(device);

                var joinUserOutput = await _inputManager.JoinUserAsync((int)inputUser.Value.id, new JoinUserOptions()
                {
                    DeviceIdentifiers = inputUser.Value.pairedDevices.Select(d => d.TryGetDeviceIdentifier())
                                            .Where(identifier => identifier is not null).Select(identifier => identifier.Value).ToArray()
                });
                
                if (!joinUserOutput.IsSuccessful)
                {
                    inputUser.Value.UnpairDevicesAndRemoveUser();
                    Debug.LogError($"Failed to complete depairing process as expected for user {inputUser.Value.id} with device {device.displayName}. Input User will be entirely removed. Error: {joinUserOutput.GetErrorString()}");
                }
            }
        }

        /// <summary>
        /// Removes the player from the input system and unpairs all devices associated with the player.
        /// </summary>
        /// <param name="playerId">The player id to remove</param>
        public void RemovePlayer(int playerId)
        {
            InputUser? user = null;
            foreach (var inputUser in InputUser.all)
            {
                if (inputUser.id == playerId)
                {
                    user = inputUser;
                    break;
                }
            }

            if (user is not null)
            {
                user.Value.UnpairDevicesAndRemoveUser();
                _inputManager.RemoveUser(playerId);
                _players.Remove(playerId);
            }
        }

        #endregion

        #region Helpers

        private void HandleUnpairedDeviceInputReceived(InputControl newDeviceInputControl, InputEventPtr inputEventPtr)
        {
            if (_inputOptions.PlayerJoinOptions.DeviceJoinBehavior is not DeviceJoinBehavior.Manual)
            {
                Task.WaitAll(PairDeviceToUserAsync(newDeviceInputControl.device));
            }
        }

        private void HandleInputDeviceChange(InputDevice device, InputDeviceChange deviceChange)
        {
            if (deviceChange is InputDeviceChange.Added && _inputOptions.PlayerJoinOptions.DeviceJoinBehavior is DeviceJoinBehavior.Automatic)
            {
                Task.WaitAll(PairDeviceToUserAsync(device));
            }
        }

        private Player UpsertPlayer(InputUser user)
        {
            if (!_players.TryGetValue((int)user.id, out var player))
            {
                player = new Player()
                {
                    Id = (int)user.id,
                    DataContext = new Dictionary<string, object>()
                };
            }

            player.InputDevices = user.pairedDevices;
            _players[(int)user.id] = player;

            return player;
        }

        private int GetTotalValidInputControllersForUser(InputUser user)
        {
            var userDeviceLookup = user.pairedDevices.Select(deviceUser => deviceUser.TryGetDeviceIdentifier())
                .Where(identifier => identifier is not null)
                .Select(identifier => identifier.Value.DeviceName)
                .ToHashSet();

            return _inputManager.Configuration.InputControllers.Count(controller => controller.DeviceNames.All(userDeviceLookup.Contains));
        }

        [ContainerInject]
        private void Initialize(IInputManager inputManager, IOutputFactory<UnityInputManager> outputFactory, IOptions<UnityInputSystemOptions> inputSystemOptions)
        {
            _inputManager = inputManager;
            _outputFactory = outputFactory;
            _inputOptions = _overrideInjectedInputOptions
                ? _inputOptions
                : inputSystemOptions.Value ?? UnityInputSystemOptions.Default;

            _inputManager.OnInputDeviceAdded += OnInputDeviceAdded;
            _inputManager.OnInputDeviceReconnected += OnInputDeviceReconnected;
            _inputManager.OnInputDeviceDisconnected += OnInputDeviceDisconnected;

            // There is performance hit with using the DeviceActivation API within Unity, developers should be aware of this when attempting to use it
            // https://docs.unity.cn/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.Users.InputUser.html
            if (_inputOptions.PlayerJoinOptions.DeviceJoinBehavior is DeviceJoinBehavior.DeviceActivation)
            {
                InputUser.onUnpairedDeviceUsed += HandleUnpairedDeviceInputReceived;
                InputUser.listenForUnpairedDeviceActivity = 1;
            }

            InputSystem.onDeviceChange += HandleInputDeviceChange;
        }
        
        #endregion
    }
}
