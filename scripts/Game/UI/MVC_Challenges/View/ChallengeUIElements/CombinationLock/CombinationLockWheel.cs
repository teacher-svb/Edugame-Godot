using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.Systems.UI
{
    // ▲ / vertical carrousel of values / ▼. Emits the centred value on init and after every spin.
    public partial class CombinationLockWheel : Control
    {
        [Signal]
        public delegate void ValueSelectedEventHandler(string paramName, int value);

        // Carrousel shows 5 slots; repeat short option lists so the wheel stays full
        const int MinVisibleValues = 5;

        public void Init(string paramName, IReadOnlyList<int> values)
        {
            CustomMinimumSize = new Vector2(72, 200);
            SizeFlagsVertical = SizeFlags.ExpandFill;

            var layout = new VBoxContainer();
            layout.SetAnchorsPreset(LayoutPreset.FullRect);

            var previous = new Button { Text = "▲" };
            var carrousel = new Carrousel { Vertical = true, SizeFlagsVertical = SizeFlags.ExpandFill };
            var next = new Button { Text = "▼" };

            var padded = values.AsEnumerable();
            while (padded.Count() < MinVisibleValues)
                padded = padded.Concat(values);

            foreach (var v in padded)
            {
                var value = new CarrouselValue { Value = v.ToString() };
                value.AddChild(new Label
                {
                    Text = v.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
                carrousel.AddChild(value);
            }

            // CarrouselController wires its buttons in _Ready, so its references must
            // be set before it enters the tree
            var controller = new CarrouselController
            {
                Carrousel = carrousel,
                BtnPrevious = previous,
                BtnNext = next,
                MouseFilter = MouseFilterEnum.Ignore,
            };

            layout.AddChild(previous);
            layout.AddChild(carrousel);
            layout.AddChild(next);
            AddChild(layout);
            AddChild(controller);

            carrousel.ValueSelected += v => EmitSignal(SignalName.ValueSelected, paramName, int.Parse(v));
            carrousel.Init();
        }
    }
}
