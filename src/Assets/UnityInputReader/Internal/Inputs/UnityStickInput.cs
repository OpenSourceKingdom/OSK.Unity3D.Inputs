using OSK.Inputs.Models.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public class UnityStickInput: UnityInput<AnalogInput, StickControl>
    {
        #region Constructors

        public UnityStickInput(AnalogInput analogInput, StickControl stickControl)
            : base(analogInput, stickControl)
        {
        }

        #endregion

        #region UnityInput Overrides

        public override bool TryGetInputPhase(out InputPhase inputPhase)
        {
            inputPhase = InputPhase.Start;
            var currentValue = InputControl.ReadValue();
            var previousValue = InputControl.ReadValue();

            if (InputControl.CheckStateIsAtDefaultIgnoringNoise())
            {
                inputPhase = previousValue.magnitude == 0 
                    ? InputPhase.Idle
                    : InputPhase.End;

                return inputPhase == InputPhase.End;
            }

            inputPhase = previousValue.magnitude == 0
                ? InputPhase.Start
                : InputPhase.Active;

            return true;
        }

        #endregion
    }
}
