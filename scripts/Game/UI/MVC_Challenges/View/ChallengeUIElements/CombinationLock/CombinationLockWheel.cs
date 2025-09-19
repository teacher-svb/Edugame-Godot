
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CombinationLockWheel : Control
    {
        [Signal]
        public delegate void ValueSelectedEventHandler(string paramName, int value);

        Carrousel _carrousel;
        CarrouselController _controller;

        CarrouselValue[] _values;

        public void Init(string paramName, IEnumerable<ChallengeValue> values)
        {
            this.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            this.SizeFlagsVertical = SizeFlags.ExpandFill;
            this.SetAnchorsPreset(LayoutPreset.FullRect);

            _carrousel = this.CreateChild<Carrousel>();
            _carrousel.SetAnchorsPreset(LayoutPreset.FullRect);
            _carrousel.Vertical = true;

            _controller = this.CreateChild<CarrouselController>();
            _controller.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _controller.SizeFlagsVertical = SizeFlags.ExpandFill;
            _controller.SetAnchorsPreset(LayoutPreset.FullRect);
            _controller.Carrousel = _carrousel;

            var controllerContainer = _controller.CreateChild<VBoxContainer>();
            controllerContainer.SetAnchorsPreset(LayoutPreset.FullRect);
            _controller.BtnPrevious = controllerContainer.CreateChild<Button>();
            _controller.BtnPrevious.SizeFlagsVertical = SizeFlags.ExpandFill;
            var spacer = controllerContainer.CreateChild<Panel>();
            spacer.SizeFlagsVertical = SizeFlags.ExpandFill;
            _controller.BtnNext = controllerContainer.CreateChild<Button>();
            _controller.BtnNext.SizeFlagsVertical = SizeFlags.ExpandFill;

            var paddedValues = values;
            while (paddedValues.Count() < 5)
            {
                paddedValues = paddedValues.Concat(values);
            }

            _values = paddedValues.Select(v =>
            {
                var value = _carrousel.CreateChild<CarrouselValue>();
                var label = value.CreateChild<RichTextLabel>();
                label.Text = v.Value.ToString();
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.VerticalAlignment = VerticalAlignment.Center;
                label.ScrollActive = false;
                value.Value = v.Value.ToString();
                return value;
            }).ToArray();

            _carrousel.ValueSelected += v => EmitSignal(SignalName.ValueSelected, paramName, int.Parse(v));
            _carrousel.Init();
        }
    }
}