using OSK.Inputs.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            : base(input)
        {
            InputControl = inputControl;
        }

        #endregion
    }
}
