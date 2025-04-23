using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                inputPhase = currentValue.magnitude == previousValue.magnitude 
                    ? InputPhase.Start
                    : InputPhase.End;

                return inputPhase == InputPhase.End;
            }

            if (currentValue.magnitude == previousValue.magnitude)
            {
                inputPhase = InputPhase.Active;
            }

            return true;
        }

        #endregion
    }
}
