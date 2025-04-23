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
                    KeyBoardInput k => k.Symbol,
                    _ => keyBoardInput.Name switch
                    {
                        "Backspace" => "Backspace",
                        "Caps" => "Caps Lock",
                        _ => keyBoardInput.Name
                    }
                },
                _ => throw new InvalidOperationException($"No mapping for input of type {input.GetType().FullName} to a Unity input could be found.")
            };
        }
    }
}
