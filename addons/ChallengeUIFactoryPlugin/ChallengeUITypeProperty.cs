using Godot;
using System;

[Tool]
public partial class ChallengeUITypeProperty : EditorProperty
{

    private OptionButton _dropdown;
    private string[] _typeNames;

    public ChallengeUITypeProperty()
    {
        _dropdown = new OptionButton();
        AddChild(_dropdown);

        _typeNames = ChallengeUIRegistry.GetNames();
        foreach (var t in _typeNames)
        {
            _dropdown.AddItem(t.Replace("UIStrategy", "")); // optional cleanup
        }

        _dropdown.ItemSelected += OnItemSelected;
    }

    private void OnItemSelected(long index)
    {
        // Tell Godot the property has changed
        EmitChanged(
            GetEditedProperty(),           // the property name (e.g. "ChallengeUIType")
            _typeNames[index],             // the new value
            default,                       // field (not needed here)
            false                          // not continuously changing
        );
    }

    // This is called by Godot when the inspector wants to set the current property value
    public override bool _Set(StringName name, Variant value)
    {
        if (name == GetEditedProperty())
        {
            var currentValue = value.AsString();
            int currentIndex = Array.IndexOf(_typeNames, currentValue);
            _dropdown.Select(currentIndex >= 0 ? currentIndex : 0);
            return true;
        }
        return false;
    }
}