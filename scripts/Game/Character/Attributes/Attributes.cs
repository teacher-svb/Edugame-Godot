
using System;
using System.Collections.Generic;
using Godot;

namespace TnT.EduGame
{
    public enum AttributeType
    {
        Health,
        // Shield,
        // Mana,
        // Stamina,
    }

    public class Attributes
    {
        Dictionary<AttributeType, int> _attributes = new()
        {
            { AttributeType.Health, 10 },
        };

        private readonly Dictionary<AttributeType, Func<int, AttributeContext, int>> _clampResolvers;

        readonly Stats _stats;
        AttributesMediator _mediator = new();
        public AttributesMediator Mediator => _mediator;

        // public int Health => _attributes.FirstOrDefault(a => a.Key == AttributeType.Health).Value;
        public int Get(AttributeType type)
        {
            if (_clampResolvers.TryGetValue(type, out var clamp))
                _attributes[type] = clamp(_attributes[type], null);
            return _attributes[type];
        }

        public Attributes(Stats stats)
        {
            _stats = stats;
            _mediator.OnAttributeModified += ChangeAttribute;
            _clampResolvers = new()
            {
                { AttributeType.Health, (v, c) => Mathf.Clamp(v, 0, _stats.MaxHealth()) },
            };
        }

        void ChangeAttribute(AttributeType type, int value, AttributeContext c)
        {
            var delta = _attributes.GetValueOrDefault(type) - value;

            if (delta > 0) // delta > 0 means taking damage (and we can apply resistance), otherwise it's healing
            {
                int resist = _stats.Resistance(c.ElementalType);
                delta = (int)(delta * (1 - resist / 100f));
            }

            _attributes[type] = _attributes.GetValueOrDefault(type) - delta;

            if (_clampResolvers.TryGetValue(type, out var clamp))
                _attributes[type] = clamp(_attributes[type], c);
        }

        public override string ToString() => $"Health: {Get(AttributeType.Health)} / {_stats.MaxHealth()}";
    }
}