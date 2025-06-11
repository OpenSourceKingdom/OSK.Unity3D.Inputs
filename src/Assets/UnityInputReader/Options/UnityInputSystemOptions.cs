using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options
{
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

        public DeviceJoinOptions PlayerJoinOptions { get; set; }

        #endregion
    }
}