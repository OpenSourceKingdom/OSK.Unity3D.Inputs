using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Ports;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class InputSystemBuilderExtensions
    {
        public static IInputSystemBuilder AddKeyboard(this IInputSystemBuilder builder)
            => builder.AddKeyboard<Internal.Services.UnityInputSystemReader>(_ => { });

        public static IInputSystemBuilder AddMouse(this IInputSystemBuilder builder)
            => builder.AddMouse<Internal.Services.UnityInputSystemReader>(_ => { });

        public static IInputSystemBuilder AddXboxController(this IInputSystemBuilder builder)
            => builder.AddXboxController<Internal.Services.UnityInputSystemReader>(_ => { });

        public static IInputSystemBuilder AddPlayStationController(this IInputSystemBuilder builder)
            => builder.AddPlayStationController<Internal.Services.UnityInputSystemReader>(_ => { });
    }
}
