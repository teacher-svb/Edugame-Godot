using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // One SpinBox per unknown, prefixed with the param name when there's more than one.
    public partial class TextInputWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>();
            var labelled = Round.Unknowns.Count > 1;

            foreach (var paramName in Round.Unknowns)
            {
                if (labelled)
                    fields.CreateChild<Label>().Text = $"{paramName} =";

                var spinbox = fields.CreateChild<SpinBox>();
                spinbox.CustomMinimumSize = new Vector2(120, 0);
                spinbox.Alignment = HorizontalAlignment.Center;
                spinbox.Rounded = true;
                spinbox.AllowGreater = true;
                spinbox.AllowLesser = true;
                spinbox.UpdateOnTextChanged = true;
                spinbox.SelectAllOnFocus = true;
                spinbox.SetValueNoSignal(0);

                // ValueChanged covers the arrows; TextChanged also catches typing
                // the value the box already holds (e.g. an answer of 0)
                spinbox.ValueChanged += value => EmitValueAssigned(paramName, (int)value);
                spinbox.GetLineEdit().TextChanged += text =>
                {
                    if (int.TryParse(text, out var value))
                        EmitValueAssigned(paramName, value);
                };
            }
        }
    }
}
