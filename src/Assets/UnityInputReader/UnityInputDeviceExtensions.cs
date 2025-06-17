using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using Keyboard = OSK.Inputs.Models.Configuration.Keyboard;
using Mouse = OSK.Inputs.Models.Configuration.Mouse;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class UnityInputDeviceExtensions
    {
        public static InputDeviceIdentifier? TryGetDeviceIdentifier(this UnityEngine.InputSystem.InputDevice device) 
        {
            var deviceName = device.TryGetDeviceName();
            if (deviceName is null)
            {
                Debug.Log($"Device, with name {device.displayName}, does not have a recognized device identifier.");
            }

            return deviceName switch
            {
                null => null,
                _ => new InputDeviceIdentifier(device.deviceId, deviceName.Value)
            };
        }

        public static InputDeviceName? TryGetDeviceName(this UnityEngine.InputSystem.InputDevice device)
        {
            return device switch
            {
                DualShockGamepad => PlayStationController.PlayStationControllerName,
                XInputController => XboxController.XboxControllerName,
                UnityEngine.InputSystem.Keyboard => Keyboard.KeyboardName,
                UnityEngine.InputSystem.Mouse => Mouse.MouseName,
                _ => null
            };
        }
    }
}
