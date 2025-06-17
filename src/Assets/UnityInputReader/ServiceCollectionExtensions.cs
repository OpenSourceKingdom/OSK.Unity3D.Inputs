using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Ports;
using System;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnityInputs(this IServiceCollection services,
            Action<IInputSystemBuilder> buildConfiguration)
        {
            services.AddInputs(buildConfiguration);

            return services;
        }
    }
}
