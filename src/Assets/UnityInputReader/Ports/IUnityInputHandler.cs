using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Ports
{
    public interface IUnityInputHandler
    {
        ValueTask HandleInputAsync(UnityInputActivationContext context, CancellationToken cancellationToken);
    }
}
