using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Runtime;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    public class Player
    {
        #region Variables

        public int Id { get; internal set; }

        public IEnumerable<InputDevice> InputDevices { get; internal set; }

        #endregion
    }
}