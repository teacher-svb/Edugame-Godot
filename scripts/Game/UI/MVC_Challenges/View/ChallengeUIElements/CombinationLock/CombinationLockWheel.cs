
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
            _carrousel = this.CreateChild<Carrousel>();
            _carrousel.Vertical = true;

            _controller = this.CreateChild<CarrouselController>();
            _controller.Carrousel = _carrousel;

            var controllerContainer = _controller.CreateChild<VBoxContainer>();
            _controller.BtnPrevious = controllerContainer.CreateChild<Button>();
            controllerContainer.CreateChild<Panel>();
            _controller.BtnNext = controllerContainer.CreateChild<Button>();

            _values = values.Select(v =>
            {
                var value = _carrousel.CreateChild<CarrouselValue>();
                var label = value.CreateChild<RichTextLabel>();
                label.Text = v.ToString();
                value.Value = v.Value.ToString();
                return value;
            }).ToArray();

            _carrousel.ValueSelected += v => EmitSignal(SignalName.ValueSelected, paramName, int.Parse(v));
        }
    }
}