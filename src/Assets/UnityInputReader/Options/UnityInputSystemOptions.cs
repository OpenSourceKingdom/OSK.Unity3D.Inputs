using System;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;
using UnityEngine;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options
{
    [Serializable]
    public class UnityInputSystemOptions
    {
        #region Static

        public static UnityInputSystemOptions Default => new UnityInputSystemOptions
        {
            PlayerJoinOptions = new DeviceJoinOptions
            {
                DeviceJoinBehavior = DeviceJoinBehavior.Automatic,
                MaxInputControllersPerPlayer = null // No limit by default
            }
        };

        #endregion

        #region Variables

        [SerializeField]
        public DeviceJoinOptions PlayerJoinOptions;

        #endregion
    }
}