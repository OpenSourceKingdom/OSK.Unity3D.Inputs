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
        #region Public

        public abstract bool TryGetInputPhase(out InputPhase inputPhase);

        #endregion
    }
}
