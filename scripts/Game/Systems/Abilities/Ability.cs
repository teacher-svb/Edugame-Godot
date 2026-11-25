
using System;
using System.Runtime.CompilerServices;
using TnT.EduGame.Characters;
using Godot;

namespace TnT.EduGame
{
    [GlobalClass]
    public partial class Ability : Resource
    {
        [Export]
        public int cooldown { get; private set; } = 1;
        [Export]
        public int uses { get; private set; } = 1;
        [Export(PropertyHint.ResourceType, "BaseAbilityEffect")]
        public Godot.Collections.Array<BaseAbilityEffect> Effects = new();

        // the use state per character that activates the ability: a loaded .tres is one shared instance, so fields on
        // the Resource made two characters with the same Ability share one cooldown (finding 7.17). A character's
        // entry goes with it (weak keys).
        readonly ConditionalWeakTable<Character3D, UseState> _states = new();

        sealed class UseState
        {
            public int RemainingUses;
            public bool IsOnCooldown;
        }

        public void Activate(Character3D source, Character3D target)
        {
            ArgumentNullException.ThrowIfNull(source);
            // starts with all uses (the field used to start at 0: the first use went straight into the full cooldown)
            var state = _states.GetValue(source, _ => new UseState { RemainingUses = uses });
            if (state.IsOnCooldown)
                return;

            ConsumeUse(state);
            foreach (var effect in Effects)
                effect.Apply(source, target);
        }

        void ConsumeUse(UseState state)
        {
            state.RemainingUses--;
            if (state.RemainingUses <= 0)
                Cooldown(state, cooldown, refill: true);
            else
                // a short pause between uses; it keeps the uses left (it used to refill them, so with more than one
                // use the full cooldown never came)
                Cooldown(state, 1, refill: false);
        }

        // a tree timer, not Task.Delay: it stops while the game is paused and has no thread-pool continuation that
        // could run after the engine shut down
        void Cooldown(UseState state, double seconds, bool refill)
        {
            state.IsOnCooldown = true;
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                EndCooldown();
                return;
            }
            tree.CreateTimer(seconds, processAlways: false).Timeout += EndCooldown;

            void EndCooldown()
            {
                if (refill)
                    state.RemainingUses = uses;
                state.IsOnCooldown = false;
            }
        }
    }
}
