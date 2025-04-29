using OSK.Inputs.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class InputExtensions
    {
        public static string GetUnityInputName(this IInput input)
        {
            var gamePadInputs = Gamepad.all.SelectMany(pad => pad.allControls);
            var keyboardInputs = Keyboard.current.allControls;
            var mouseInputs = Mouse.current.allControls;
            var sensorInputs = InputSystem.devices;
            return input switch
            {
                IKeyboardInput keyBoardInput => keyBoardInput switch
                {
                    KeyBoardInput k => k.Symbol switch
                    {
                        "<-" => "Backspace",
                        "Caps" => "Caps Lock",
                        " " => k.Name,
                        "˂" => "Left",
                        "˄" => "Up",
                        "˃" => "Right",
                        "˅" => "Down",
                        "ESC" => "Esc",
                        "DEL" => "Delete",
                        "HOME" => "Home",
                        _ => k.Symbol
                    },
                    KeyboardCombination => keyBoardInput.Name,
                    _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName} to a Unity input could be found.")
                },
                IMouseInput mouseInput => mouseInput switch
                {
                    MouseButtonInput mouseButton => mouseButton.Id switch
                    {
                        "Left Click" => "Left Button",
                    },
                    _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName} to a Unity input could be found.")

                },
                _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName} to a Unity input could be found.")
            };
        }
    }
}
