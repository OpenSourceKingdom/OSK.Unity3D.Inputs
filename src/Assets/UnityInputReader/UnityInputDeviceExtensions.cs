using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using System;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class UnityInputDeviceExtensions
    {
        public static InputDeviceIdentifier GetDeviceIdentifier(this UnityEngine.InputSystem.InputDevice device)
            => new InputDeviceIdentifier(device.deviceId, device.GetDeviceName());

        public static InputDeviceName GetDeviceName(this UnityEngine.InputSystem.InputDevice device)
            => device.displayName switch
            {
                "Xbox 360" => XboxController.XboxControllerName,
                "Xbox One" => XboxController.XboxControllerName,
                "PS4" => PlayStationController.PlayStationControllerName,
                "Keyboard" => Keyboard.KeyboardName,
                "Mouse" => Mouse.MouseName,
                _ => throw new NotSupportedException($"Unity input device {device.displayName} does not have a mapping to a device name.")
            };
    }
}
