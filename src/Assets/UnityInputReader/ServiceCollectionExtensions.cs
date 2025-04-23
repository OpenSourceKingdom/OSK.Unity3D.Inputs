using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Services;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnityInputs(this IServiceCollection services,
            Action<IInputSystemBuilder> buildConfiguration)
        {
            services.AddInputs(buildConfiguration);
            services.AddTransient<IUnityInputProcessor, UnityInputProcessor>();

            return services;
        }
    }
}
