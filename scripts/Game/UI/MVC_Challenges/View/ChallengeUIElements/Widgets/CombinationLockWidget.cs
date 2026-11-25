using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.UI
{
    // One carrousel wheel per unknown. Every wheel always shows a value, so each one is
    // reported as soon as it's built (deferred: ChallengeUI subscribes after adding the widget,
    // and the wheel's own first ValueSelected fires inside Init, i.e. in this _Ready) and the
    // player only has to spin.
    public partial class CombinationLockWidget : ChallengeInputWidget
    {
        // Carrousel wheel instanced per unknown; its CarrouselValue children are
        // placeholders, the first one is used as template for the round's options
        const string WheelScenePath = "res://Scenes/UI/carrousel.tscn";

        // Carrousel shows 5 slots; repeat short option lists so the wheel stays full
        const int MinVisibleValues = 5;

        static readonly Vector2 WheelSize = new(72, 200);

        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>(includeSelf: true);
            var wheelScene = GD.Load<PackedScene>(WheelScenePath);

            var wheels = new List<(string Param, Carrousel Carrousel)>();
            foreach (var paramName in Round.Unknowns)
            {
                var wheel = wheelScene.Instantiate<Control>();
                wheel.CustomMinimumSize = WheelSize;

                var carrousel = wheel.FindAnyObjectByType<Carrousel>(includeSelf: true);
                FillValues(carrousel, Round.OptionsFor(paramName));
                carrousel.ValueSelected += v => EmitValueAssigned(paramName, ParseValue(v));

                fields.AddChild(wheel);
                carrousel.Init();
                wheels.Add((paramName, carrousel));
                FirstFocus ??= wheel.FindAnyObjectByType<CarrouselController>(includeSelf: true)?.BtnNext;
            }

            Callable.From(() => ReportStart(wheels)).CallDeferred();
        }

        void ReportStart(List<(string Param, Carrousel Carrousel)> wheels)
        {
            if (!IsInstanceValid(this))
                return;
            foreach (var (param, carrousel) in wheels)
                EmitValueAssigned(param, ParseValue(carrousel.CurrentValue));
        }

        static int ParseValue(string value) => int.Parse(value, CultureInfo.InvariantCulture);

        // Swap the scene's placeholder values for the options, keeping the first
        // placeholder's look as template.
        static void FillValues(Carrousel carrousel, IReadOnlyList<int> options)
        {
            var placeholders = carrousel.GetChildren().OfType<CarrouselValue>().ToList();
            var template = placeholders.First();

            var padded = options.ToList();
            while (options.Count > 0 && padded.Count < MinVisibleValues)
                padded.AddRange(options);

            foreach (var option in padded)
            {
                var value = (CarrouselValue)template.Duplicate();
                value.Value = option.ToString();
                var label = value.FindAnyObjectByType<RichTextLabel>(includeSelf: true);
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
