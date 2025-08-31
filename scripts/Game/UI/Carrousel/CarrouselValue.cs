using Godot;
using System;

[GlobalClass]
public partial class CarrouselValue : PanelContainer
{
    // TODO: When Godot 4.5 releases, consider changing "string" to "Variant"
    [Export] public string Value;

    public override void _Ready()
    {
        this.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        this.SizeFlagsVertical = SizeFlags.ExpandFill;
    }
}
