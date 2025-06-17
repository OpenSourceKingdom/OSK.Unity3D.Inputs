using OSK.Inputs.Models.Inputs;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public class UnityTouchInput: UnityInput<TouchInput, TouchControl>
    {
        #region Constructors

        public UnityTouchInput(TouchInput touchInput, TouchControl control)
            : base(touchInput, control)
        { }

        #endregion

        #region UnityInput Overrides

        public override bool TryGetInputPhase(out InputPhase inputPhase)
        {
            InputPhase? phase = null;
            if (InputControl.press.wasPressedThisFrame)
            {
                phase = InputPhase.Start;
            }
            else if (InputControl.press.wasReleasedThisFrame)
            {
                phase = InputPhase.End;
            }
            else if (InputControl.press.isPressed)
            {
                phase = InputPhase.Active;
            }

            inputPhase = phase ?? InputPhase.Start;
            return phase != null;
        }

        #endregion
    }
}
