using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // One OptionButton per unknown, prefixed with the param name when there's more than one.
    public partial class DropdownWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>();
            var labelled = Round.Unknowns.Count > 1;

            foreach (var paramName in Round.Unknowns)
            {
                if (labelled)
                    fields.CreateChild<Label>().Text = $"{paramName} =";

                var options = Round.OptionsFor(paramName);
                var dropdown = fields.CreateChild<OptionButton>();
                dropdown.CustomMinimumSize = new Vector2(100, 0);
                foreach (var option in options)
                    dropdown.AddItem(option.ToString());

                // nothing preselected, so the player has to make a choice
                dropdown.Selected = -1;
                dropdown.ItemSelected += index => EmitValueAssigned(paramName, options[(int)index]);
            }
        }
    }
}
