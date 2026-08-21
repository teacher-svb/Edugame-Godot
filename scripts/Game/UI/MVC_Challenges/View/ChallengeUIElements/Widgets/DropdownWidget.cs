using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class DropdownWidget : ChallengeInputWidget
    {
        public override void _Ready()
        {
            var dropdown = this.CreateChild<OptionButton>();

            var values = Challenge.Values.Where(v => v.ParamName != "").ToList();
            values.ForEach(v => dropdown.AddItem(v.Value.ToString()));

            dropdown.ItemSelected += index =>
            {
                var v = values[(int)index];
                EmitValueAssigned(v.ParamName, v.Value);
            };
        }
    }
}
