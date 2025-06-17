namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options
{
    public class GameSelectionOptions
    {
        #region Static

        public static GameSelectionOptions Default = new GameSelectionOptions()
        {
            PointerCastDepth = 10
        };

        #endregion

        #region Variables

        /// <summary>
        /// The total depth to cast the pointer. This is used to determine how far the pointer should reach into the scene.
        /// </summary>
        public float PointerCastDepth { get; set; }

        #endregion
    }
}