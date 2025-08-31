using Godot;
using System;

[GlobalClass]
public partial class CarrouselValue : PanelContainer
{
    // TODO: When Godot 4.5 releases, consider changing "string" to "Variant"
    [Export] public string _value;
}
