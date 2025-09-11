using System.Linq;
using Godot;

[Tool]
public partial class ChallengeUIFactoryPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        GD.Print("ChallengeUIFactory Plugin Loaded.");
        ChallengeUIRegistry.Initialize();

        var types = ChallengeUIRegistry.GetRegisteredTypes().Select(t => t.ToString()).ToArray().Join("\n- ");
        GD.Print($"Registered Visualization Strategies:\n- {types}");
    }
}