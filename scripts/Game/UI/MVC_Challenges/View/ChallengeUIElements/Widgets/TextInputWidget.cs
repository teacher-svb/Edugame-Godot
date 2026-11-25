using System.Globalization;
using Godot;
using TnT.Extensions;

namespace TnT.EduGame.UI
{
    // One SpinBox per unknown, prefixed with the param name when there's more than one.
    public partial class TextInputWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>(includeSelf: true);
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

                // ValueChanged covers the arrows; TextChanged also catches typing the value the
                // box already holds (e.g. an answer of 0), and an empty or unfinished field (`-`)
                // takes the value back, so Submit can't grade a number that is no longer there
                var lineEdit = spinbox.GetLineEdit();
                spinbox.ValueChanged += value => EmitValueAssigned(paramName, (int)value);
                lineEdit.TextChanged += text => Report(paramName, text);
                // the box puts its last value back in a field left empty (SpinBox does that
                // deferred), so report what it shows once it has
                void Resync() => Callable.From(() =>
                {
                    if (IsInstanceValid(lineEdit))
                        Report(paramName, lineEdit.Text);
                }).CallDeferred();
                lineEdit.FocusExited += Resync;
                lineEdit.TextSubmitted += _ => Resync();

                FirstFocus ??= lineEdit;
            }
        }

        void Report(string paramName, string text)
        {
            if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                EmitValueAssigned(paramName, value);
            else
                EmitValueCleared(paramName);
        }
    }
}
