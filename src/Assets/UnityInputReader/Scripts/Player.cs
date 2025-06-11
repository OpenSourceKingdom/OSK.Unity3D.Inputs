using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    public class Player
    {
        #region Variables

        public int Id { get; internal set; }

        public IReadOnlyCollection<InputDevice> InputDevices { get; internal set; }

        public Dictionary<string, object> DataContext { get; internal set; }

        #endregion
    }
}