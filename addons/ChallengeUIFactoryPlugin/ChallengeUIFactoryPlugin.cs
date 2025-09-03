using Godot;

[Tool]
public partial class ChallengeUIFactoryPlugin : EditorPlugin
{
    private ChallengeUITypeInspectorPlugin _inspector;

    public override void _EnterTree()
    {
        GD.Print("ChallengeUIFactory Plugin Loaded.");
        _inspector = new ChallengeUITypeInspectorPlugin();
        AddInspectorPlugin(_inspector);

        ChallengeUIRegistry.Initialize();
    }

    public override void _ExitTree()
    {
        if (_inspector != null)
            RemoveInspectorPlugin(_inspector);
    }
}