
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private int _remainingUses;
        private bool _isOnCooldown;

        public void Activate(Character source, Character target)
        {
            if (_isOnCooldown)
                return;
            
            ConsumeUse();
            foreach (var effect in Effects)
                effect.Apply(source, target);
        }

        protected void ConsumeUse()
        {
            _remainingUses--;
            if (_remainingUses <= 0)
                StartCooldown();
            else
                StartMinorCooldown();
        }

        async void StartCooldown()
        {
            _isOnCooldown = true;
            await Task.Delay(cooldown * 1000);
            _remainingUses = uses;
            _isOnCooldown = false;
        }

        async void StartMinorCooldown()
        {
            _isOnCooldown = true;
            await Task.Delay(1000);
            _remainingUses = uses;
            _isOnCooldown = false;
        }
    }
}