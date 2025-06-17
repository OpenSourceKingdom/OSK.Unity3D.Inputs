using OSK.Inputs.Models.Inputs;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public abstract class UnityInput<TInput, TInputControl>: UnityInput
        where TInput : IInput
        where TInputControl : InputControl
    {
        #region Variables

        public TInputControl InputControl { get; }

        #endregion

        #region Constructors

        public UnityInput(TInput input, TInputControl inputControl)
            : base(input, inputControl.displayName)
        {
            InputControl = inputControl;
        }

        #endregion
    }
}
