using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{
    public abstract partial class ModifierEffect : BaseAbilityEffect
    {
        [Export] public override ElementalType elementalType { get; protected set; } = ElementalType.None;
                 public override AbilityOperatorType operatorType { get; protected set; } = AbilityOperatorType.Add;
        // [Export] public override AbilityOperatorType OperatorType { get; protected set; } = AbilityOperatorType.Add;
        [Export] public override float value { get; protected set; } = 10;
        [Export] public override int duration { get; protected set; } = -1;
        [Export] public override int frequency { get; protected set; } = 1;
        public override void Apply(Character source, Character target)
        {
            ApplyEffect(source, target);
        }
        protected abstract void ApplyEffect(Character source, Character target);
    }
    }