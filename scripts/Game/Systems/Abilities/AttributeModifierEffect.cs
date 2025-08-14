using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{
    [GlobalClass]
    public partial class AttributeModifierEffect : ModifierEffect
    {
        [Export] AttributeType type = AttributeType.Health;
        protected override void ApplyEffect(Character source, Character target)
        {
            AttributeContext c = new(source, target, elementalType, duration, frequency);
            AttributeModifier mod = operatorType switch
            {
                AbilityOperatorType.Add => new BasicAttributeModifier(type, v => (int)(v + value), c),
                AbilityOperatorType.Subtract => new BasicAttributeModifier(type, v => (int)(v - value), c),
                AbilityOperatorType.Multiply => new BasicAttributeModifier(type, v => (int)(v * value), c),
                _ => throw new System.NotImplementedException(),
            };

            target.Attributes.Mediator.AddModifier(mod);
        }
    }
}