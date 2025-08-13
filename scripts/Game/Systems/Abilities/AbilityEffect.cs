using System;
using Godot;
using TnT.EduGame.Characters;

namespace TnT.EduGame
{
    public enum AbilityOperatorType { Add, Multiply, Subtract }
    
    public interface IAbilityEffect
    {
        public ElementalType elementalType { get; }
        public AbilityOperatorType operatorType { get; }
        public float value { get; }
        public int duration { get; }
        public int frequency { get; }
        void Apply(Character source, Character target);
    }

    public abstract partial class ModiferEffect : Resource, IAbilityEffect
    {
        [Export] public ElementalType elementalType { get; private set; } = ElementalType.None;
        [Export] public AbilityOperatorType operatorType { get; private set; } = AbilityOperatorType.Add;
        [Export] public float value { get; private set; } = 10;
        [Export] public int duration { get; private set; } = -1;
        [Export] public int frequency { get; private set; } = 1;
        public void Apply(Character source, Character target)
        { 
            ApplyEffect(source, target);
        }
        protected abstract void ApplyEffect(Character source, Character target);
    }

    [GlobalClass]
    public partial class AttributeModifierEffect : ModiferEffect
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

    [GlobalClass]
    public partial class StatModifierEffect : ModiferEffect
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