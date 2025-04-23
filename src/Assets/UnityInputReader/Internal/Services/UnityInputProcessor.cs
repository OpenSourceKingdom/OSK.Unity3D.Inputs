using OSK.Inputs.Models.Runtime;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Ports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Services
{
    internal class UnityInputProcessor : IUnityInputProcessor
    {
        #region IUnityInputProcessor

        public ValueTask ProcessActivatedInputsAsync(InputActivationContext context, CancellationToken cancellationToken = default)
        {
            return UnityEngine.Debug.isDebugBuild
                ? ExecuteWithDebugMiddlewareAsync(context)
                : context.ExecuteCommandsAsync(null);
        }

        #endregion

        #region Helpers

        private async ValueTask ExecuteWithDebugMiddlewareAsync(InputActivationContext context)
        {
            await context.ExecuteCommandsAsync((next, @event) =>
            {
                UnityEngine.Debug.Log($"Received Input!");
                UnityEngine.Debug.Log(@event.Input);

                return next(@event);
            });
        }

        #endregion
    }
}
