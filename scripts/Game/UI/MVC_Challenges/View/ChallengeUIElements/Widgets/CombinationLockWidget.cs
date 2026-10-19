using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // One wheel per unknown. Every wheel always shows a value, so each one is
    // assigned as soon as it's built and the player only has to spin.
    public partial class CombinationLockWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>();

            foreach (var paramName in Round.Unknowns)
            {
                var wheel = fields.CreateChild<CombinationLockWheel>();
                wheel.ValueSelected += (param, value) => EmitValueAssigned(param, value);
                wheel.Init(paramName, Round.OptionsFor(paramName));
            }
        }
    }
}
