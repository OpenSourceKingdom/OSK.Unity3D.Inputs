using System;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class InputSystemBuilderExtensions
    {
        public static IInputSystemBuilder AddKeyboard(this IInputSystemBuilder builder)
            => builder.AddKeyboard<Internal.Services.UnityInputSystemReader>();

        public static IInputSystemBuilder AddMouse(this IInputSystemBuilder builder)
            => builder.AddMouse<Internal.Services.UnityInputSystemReader>();

        public static IInputSystemBuilder AddXboxController(this IInputSystemBuilder builder)
            => builder.AddXboxController<Internal.Services.UnityInputSystemReader>();

        public static IInputSystemBuilder AddPlayStationController(this IInputSystemBuilder builder)
            => builder.AddPlayStationController<Internal.Services.UnityInputSystemReader>();

        public static IInputSystemBuilder WithUnityInputOptions(this IInputSystemBuilder builder, Action<UnityInputSystemOptions> configuration)
        {
            var unityInputOptions = UnityInputSystemOptions.Default;
            configuration?.Invoke(unityInputOptions);

            return builder;
        }
    }
}
