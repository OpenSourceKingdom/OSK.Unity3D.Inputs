using OSK.Inputs.Models.Inputs;
using System;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public abstract class UnityInput
    {
        #region Public

        public IInput DeviceInput { get; }

        public string UnityInputName { get; }

        public abstract bool TryGetInputPhase(out InputPhase inputPhase);

        #endregion

        #region Constructors

        protected UnityInput(IInput input, string unityInputName)
        {
            DeviceInput = input ?? throw new ArgumentNullException(nameof(input));
            UnityInputName = unityInputName;
        }

        #endregion
    }
}
