using Microsoft.Extensions.DependencyInjection;
using OSK.Functions.Outputs.Logging;
using OSK.Inputs;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Scripts;
using System.Threading.Tasks;

public class ExampleSceneContainer : SceneContainer
{
    public static ValueTask CompletedTask = new ValueTask(Task.CompletedTask);

    protected override void Configure(IServiceCollection services)
    {
        services.AddLogging();
        services.AddLoggingFunctionOutputs();
        services.AddUnityInputs(static builder =>
        {
            builder.AddKeyboard();
            builder.AddMouse();

            builder.AddInputDefinition("Test", definition =>
            {
                definition.AddAction("Triggered", null, @event => CompletedTask);
                definition.AddAction("Triggered2", null, @event => CompletedTask);

                definition.AddInputScheme("default", scheme =>
                {
                    scheme.UseKeyboard(keyboard =>
                    {
                        keyboard.AssignStartAction(Keyboard.ExclamationPoint, "Triggered");
                    });
                    scheme.UseMouse(mouse =>
                    {
                        mouse.AssignEndAction(Mouse.LeftClick, "Triggered2");
                    });
                });
            });
        });
    }
}
