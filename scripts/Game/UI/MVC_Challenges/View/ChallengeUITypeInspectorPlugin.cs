using Godot;

[Tool]
public partial class ChallengeUITypeInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => true;

    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (name == "ChallengeUIType")
        {
            AddPropertyEditor(name, new ChallengeUITypeProperty());
            return true;
        }
        else
        {
            return base._ParseProperty(@object, type, name, hintType, hintString, usageFlags, wide);
        }
    }

}
