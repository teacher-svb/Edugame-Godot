using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // One toggle button per number in the combined option pool of all unknowns.
    // Pressing fills the first empty unknown (in formula order); releasing frees it again.
    public partial class CogwheelWidget : ChallengeInputWidget
    {
        readonly Dictionary<string, Button> _picked = new();

        public override void _Ready()
        {
            var fields = this.FindAnyObjectByType<HBoxContainer>();

            var pool = Round.Unknowns
                .SelectMany(u => Round.OptionsFor(u))
                .Distinct();

            foreach (var value in pool)
            {
                var button = fields.CreateChild<Button>();
                button.Text = value.ToString();
                button.ToggleMode = true;
                button.CustomMinimumSize = new Vector2(56, 56);
                button.Toggled += pressed => OnToggled(button, value, pressed);
            }
        }

        void OnToggled(Button button, int value, bool pressed)
        {
            if (!pressed)
            {
                var param = _picked.FirstOrDefault(kv => kv.Value == button).Key;
                if (param == null)
                    return;
                _picked.Remove(param);
                EmitValueCleared(param);
                return;
            }

            var free = Round.Unknowns.FirstOrDefault(u => !_picked.ContainsKey(u));
            if (free == null)
            {
                // every unknown already has a value: refuse the extra pick
                button.SetPressedNoSignal(false);
                return;
            }

            _picked[free] = button;
            EmitValueAssigned(free, value);
        }
    }
}
