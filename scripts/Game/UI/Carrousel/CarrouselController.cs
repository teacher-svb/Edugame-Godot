using Godot;
using System;
using TnT.Extensions;

[GlobalClass]
public partial class CarrouselController : BoxContainer
{
    [Export]
    Carrousel _carrousel;
    [Export]
    Button _btnPrevious;
    [Export]
    Button _btnNext;
    public override void _Ready()
    {
        var buttons = this.FindObjectsByType<Button>();

        _btnPrevious.Pressed += _carrousel._SelectPrevious;

        _btnNext.Pressed += _carrousel._SelectNext;
    }
}
