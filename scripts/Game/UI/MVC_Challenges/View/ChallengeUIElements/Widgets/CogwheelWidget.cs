using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class CogwheelWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var container = this.CreateChild<HBoxContainer>();
            container.SetAnchorsPreset(LayoutPreset.FullRect);

            Challenge.Values
                .Where(v => v.ParamName != "")
                .Shuffle(42) // predetermined seed, so shuffled order is stable across rebuilds
                .ToList()
                .ForEach(v =>
                {
                    var button = container.CreateChild<Button>();
                    button.Text = v.Value.ToString();
                    // paramName travels with the value itself (author-set on the
                    // ChallengeValue resource), no external lookup needed on click.
                    button.Pressed += () => EmitValueAssigned(v.ParamName, v.Value);
                });
        }
    }
}
