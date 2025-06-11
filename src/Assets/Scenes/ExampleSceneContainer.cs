using Microsoft.Extensions.DependencyInjection;
using OSK.Functions.Outputs.Logging;
using OSK.Inputs;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader;
using OSK.Unity3D.NetCollections.Assets.Plugins.NetCollections.Scripts;

public class ExampleSceneContainer : SceneContainer
{
    protected override void Configure(IServiceCollection services)
    {
        services.AddLogging();
        services.AddLoggingFunctionOutputs();
        services.AddUnityInputs(static builder =>
        {
            builder.AddKeyboard();
            builder.AddMouse();
            builder.AddPlayStationController();

            builder.AddInputDefinition("Test", definition =>
            {
                definition.AddAction("Triggered", null, @event =>
                {
                    return UnityValueTasks.CompletedTask; 
                });
                definition.AddAction("Triggered2", null, @event => 
                {
                    return UnityValueTasks.CompletedTask;
                });
                definition.AddAction("Triggered3", null, @event =>
                {
                    return UnityValueTasks.CompletedTask;
                });
                definition.AddAction("Triggered4", null, @event =>
                {
                    return UnityValueTasks.CompletedTask;
                });

                definition.AddInputScheme("default", scheme =>
                {
                    scheme.UseKeyboard(keyboard =>
                    {
                        keyboard.AssignStartAction(Keyboard.ExclamationPoint, "Triggered");
                        keyboard.AssignEndAction(Keyboard.W, "Triggered2");
                    });
                    scheme.UseMouse(mouse =>
                    {
                        mouse.AssignStartAction(Mouse.LeftClick, "Triggered4");
                        mouse.AssignActiveAction(Mouse.RightClick, "Triggered3");
                    });
                });

                definition.AddInputScheme("default", scheme =>
                {
                    scheme.UsePlayStationGamePad(dualShockGamePad =>
                    {
                        dualShockGamePad.AssignStartAction(GamePadDevice.Square, "Triggered");
                        dualShockGamePad.AssignEndAction(GamePadDevice.X, "Triggered2");
                        dualShockGamePad.AssignActiveAction(GamePadDevice.LeftTrigger, "Triggered3");
                        dualShockGamePad.AssignActiveAction(GamePadDevice.RightJoyStick, "Triggered4");
                    });
                });
            });
        });
    }
}
