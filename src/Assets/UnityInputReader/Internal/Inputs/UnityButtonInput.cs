using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem.Controls;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public class UnityButtonInput: UnityInput<HardwareInput, ButtonControl>
    {
        #region Constructors

        public UnityButtonInput(HardwareInput hardwareInput, ButtonControl buttonControl)
            : base(hardwareInput, buttonControl)
        {

        }

        #endregion

        #region UnityInput Overrides

        public override bool TryGetInputPhase(out InputPhase inputPhase)
        {
            InputPhase? phase = null;

            if (InputControl.wasPressedThisFrame)
            {
                phase = InputPhase.Start;
            }
            else if (InputControl.wasReleasedThisFrame)
            {
                phase = InputPhase.End;
            }
            else if (InputControl.isPressed)
            {
                phase = InputPhase.Active;
            }

            inputPhase = phase ?? InputPhase.Start;
            return phase != null;
        }

        #endregion
    }
}
