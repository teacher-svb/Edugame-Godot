using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // One carrousel wheel per unknown. Every wheel always shows a value, so each
    // one is assigned as soon as it's built and the player only has to spin.
    public partial class CombinationLockWidget : ChallengeInputWidget
    {
        // Carrousel shows 5 slots; repeat short option lists so the wheel stays full
        const int MinVisibleValues = 5;

        static readonly Vector2 WheelSize = new(72, 200);

        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>();
            var wheelScene = ((CombinationLockInput)Challenge.Input).ResolveWheelScene();

            foreach (var paramName in Round.Unknowns)
            {
                var wheel = wheelScene.Instantiate<Control>();
                wheel.CustomMinimumSize = WheelSize;

                var carrousel = wheel.FindAnyObjectByType<Carrousel>();
                FillValues(carrousel, Round.OptionsFor(paramName));
                carrousel.ValueSelected += v => EmitValueAssigned(paramName, int.Parse(v));

                fields.AddChild(wheel);
                carrousel.Init();
            }
        }

        // Swap the scene's placeholder values for the options, keeping the first
        // placeholder's look as template.
        static void FillValues(Carrousel carrousel, IReadOnlyList<int> options)
        {
            var placeholders = carrousel.GetChildren().OfType<CarrouselValue>().ToList();
            var template = placeholders.First();

            var padded = options.AsEnumerable();
            while (padded.Count() < MinVisibleValues)
                padded = padded.Concat(options);

            foreach (var option in padded)
            {
                var value = (CarrouselValue)template.Duplicate();
                value.Value = option.ToString();
                var label = value.FindAnyObjectByType<RichTextLabel>();
                if (label != null)
                    label.Text = option.ToString();
                carrousel.AddChild(value);
            }

            foreach (var placeholder in placeholders)
            {
                carrousel.RemoveChild(placeholder);
                placeholder.Free();
            }
        }
    }
}
