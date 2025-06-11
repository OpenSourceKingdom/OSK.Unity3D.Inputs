using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using Keyboard = OSK.Inputs.Models.Configuration.Keyboard;
using Mouse = OSK.Inputs.Models.Configuration.Mouse;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class InputExtensions
    {
        private static Dictionary<string, Dictionary<int, string[]>> s_inputNameOverrideLookup = GetOverrideInputKeys()
            .GroupBy(kvp => kvp.Key.DeviceType)
            .ToDictionary(kvpGroup => kvpGroup.Key, kvpGroup => kvpGroup.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value));

        public static string[] GetUnityInputNames(this IInput input)
        {
            if (s_inputNameOverrideLookup.TryGetValue(input.DeviceType, out var deviceOverrideLookup)
                 && deviceOverrideLookup.TryGetValue(input.Id, out var customOverride))
            {
                return customOverride;
            }

            return input switch
            {
                IKeyboardInput keyBoardInput => keyBoardInput switch
                {
                    KeyBoardInput key => new string[] { key.Symbol },
                    KeyboardCombination => new string[] { keyBoardInput.Name },
                    _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName}, {input.Name}, to a Unity input could be found.")
                },
                IGamePadInput gamePadInput => gamePadInput switch 
                {
                    GamePadButtonInput buttonInput => new string[] { buttonInput.Name },
                    GamePadStickInput stickInput => new string[] { gamePadInput.Name },
                    _ => throw new InvalidOperationException("No mapping for input of type {input.GetType().FullName}, {input.Name}, to a Unity input could be found.")
                },
                _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName}, {input.Name},  to a Unity input could be found.")
            };
        }

        private static IEnumerable<KeyValuePair<IInput, string[]>> GetOverrideInputKeys()
            => new List<KeyValuePair<IInput, string[]>>()
            {
                OverrideInputKey(Keyboard.BackSpace, "Backspace"),
                OverrideInputKey(Keyboard.Caps, "Caps Lock"),
                OverrideInputKey(Keyboard.Space, Keyboard.Space.Name),
                OverrideInputKey(Keyboard.LeftArrow, "Left"),
                OverrideInputKey(Keyboard.RightArrow, "Right"),
                OverrideInputKey(Keyboard.UpArrow, "Up"),
                OverrideInputKey(Keyboard.DownArrow, "Down"),
                OverrideInputKey(Keyboard.Escape, "Esc"),
                OverrideInputKey(Keyboard.Delete, "Delete"),
                OverrideInputKey(Keyboard.Home, "Home"),
                OverrideInputKey(Keyboard.Shift, "Shift", "Right Shift"),
                OverrideInputKey(Keyboard.Alt, "Alt", "Right Alt"),
                OverrideInputKey(Keyboard.Ctrl, "Ctrl", "Right Ctrl"),

                OverrideInputKey(Mouse.LeftClick, "Left Button"),
                OverrideInputKey(Mouse.RightClick, "Right Button"),
                OverrideInputKey(Mouse.ScrollWheelClick, "Middle Button"),
                OverrideInputKey(Mouse.ScrollWheel, "Scroll"),

                OverrideInputKey(GamePadDevice.X, "Cross", "X"),
                OverrideInputKey(GamePadDevice.LeftBumper, "L1", "Left Bumper"),
                OverrideInputKey(GamePadDevice.RightBumper, "R1", "Right Bumper"),
                OverrideInputKey(GamePadDevice.LeftTrigger, "L2", "leftTrigger"),
                OverrideInputKey(GamePadDevice.RightTrigger, "R2", "rightTrigger"),
                OverrideInputKey(GamePadDevice.LeftJoyStickClick, "L3"),
                OverrideInputKey(GamePadDevice.RightJoyStickClick, "R3"),
                OverrideInputKey(GamePadDevice.LeftJoyStick, "Left Stick"),
                OverrideInputKey(GamePadDevice.RightJoyStick, "Right Stick"),
                OverrideInputKey(GamePadDevice.Menu, "Options", "Menu"),
                OverrideInputKey(GamePadDevice.Options, "Share", "View"),
                OverrideInputKey(GamePadDevice.DpadLeft, "D-Pad/left"),
                OverrideInputKey(GamePadDevice.DpadRight, "D-Pad/right"),
                OverrideInputKey(GamePadDevice.DpadUp, "D-Pad/up"),
                OverrideInputKey(GamePadDevice.DpadDown, "D-Pad/down")
            };

        private static KeyValuePair<IInput, string[]> OverrideInputKey(IInput input, params string[] overrideNames)
            => new KeyValuePair<IInput, string[]>(input, overrideNames);
    }
}
