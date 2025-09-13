using Godot;
using System;
using TnT.Extensions;

[GlobalClass]
public partial class CarrouselController : Control
{
    [Export]
    Carrousel _carrousel;
    [Export]
    Button _btnPrevious;
    [Export]
    Button _btnNext;

    public Carrousel Carrousel { get => _carrousel; set => _carrousel = value; }
    public Button BtnPrevious { get => _btnPrevious; set => _btnPrevious = value; }
    public Button BtnNext { get => _btnNext; set => _btnNext = value; }

    public override void _Ready()
    {
        var buttons = this.FindObjectsByType<Button>();

        BtnPrevious.Pressed += Carrousel._SelectPrevious;

        BtnNext.Pressed += Carrousel._SelectNext;
    }
}
