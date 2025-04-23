using OSK.Inputs.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    internal class UnityDeltaInput: UnityInput<MouseScrollInput, DeltaControl>
    {
        #region Constructors

        public UnityDeltaInput(MouseScrollInput scrollInput, DeltaControl inputControl)
            : base(scrollInput, inputControl)
        {
        }

        #endregion

        #region UnityInput Overrides

        public override bool TryGetInputPhase(out InputPhase inputPhase)
        {
            var previousValue = InputControl.ReadValueFromPreviousFrame();

            if (InputControl.CheckStateIsAtDefaultIgnoringNoise())
            {
                inputPhase = previousValue.magnitude == 0
                    ? InputPhase.Start
                    : InputPhase.End;
                return previousValue.magnitude == 0;
            }

            inputPhase = previousValue.magnitude == 0
                ? InputPhase.Start
                : InputPhase.Active;

            return true;
        }

        #endregion
    }
}
