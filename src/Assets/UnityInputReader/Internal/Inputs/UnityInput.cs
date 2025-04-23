using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public abstract class UnityInput
    {
        #region Variables

        private readonly IInput _input;

        #endregion

        #region Constructors

        protected UnityInput(IInput input)
        {
            _input = input;
        }

        #endregion

        #region Public

        public IInput DeviceInput => _input;

        public abstract bool TryGetInputPhase(out InputPhase inputPhase);

        #endregion
    }
}
