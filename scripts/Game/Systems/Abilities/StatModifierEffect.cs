using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{

    [GlobalClass]
    public partial class StatModifierEffect : ModifierEffect
    {
        [Export] StatType type = StatType.MaxHealth;


        protected override void ApplyEffect(Character source, Character target)
        {
            StatModifier mod = operatorType switch
            {
                AbilityOperatorType.Add => new BasicStatModifier(type, v => (int)(v + value), duration, elementalType),
                AbilityOperatorType.Subtract => new BasicStatModifier(type, v => (int)(v - value), duration, elementalType),
                AbilityOperatorType.Multiply => new BasicStatModifier(type, v => (int)(v * value), duration, elementalType),
                _ => throw new System.NotImplementedException(),
            };

            target.Stats.Mediator.AddModifier(mod);
        }
    }
}