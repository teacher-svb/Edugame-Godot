using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class TextInputWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var spinbox = this.FindAnyObjectByType<SpinBox>();
            spinbox.SetValueNoSignal(0);

            var paramName = Challenge.FormulaParams[0];
            spinbox.ValueChanged += value => EmitValueAssigned(paramName, (int)value);
        }
    }
}
