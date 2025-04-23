using OSK.Inputs.Models.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Ports
{
    public interface IUnityInputProcessor
    {
        ValueTask ProcessActivatedInputsAsync(InputActivationContext context, CancellationToken cancellationToken = default);
    }
}
