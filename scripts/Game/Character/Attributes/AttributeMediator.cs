using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame
{
    public class AttributesMediator
    {
        private readonly List<AttributeModifier> modifiers = new();
        public event Action<AttributeType, int, AttributeContext> OnAttributeModified = delegate { };

        public void AddModifier(AttributeModifier mod)
        {
            modifiers.Add(mod);
            mod.OnApply += ApplyMod;
            mod.Init();
            
            mod.OnDispose += _ =>
            {
                mod.OnApply -= ApplyMod;
                modifiers.Remove(mod);
            };
        }

        private void ApplyMod(AttributeType type, int v, AttributeContext c)
        {
            OnAttributeModified?.Invoke(type, v, c);
        }

        public void Update(double deltaTime)
        {
            modifiers.ForEach(m => m.Update(deltaTime));
            modifiers
                .Where(e => e.IsExpired)
                .ToList()
                .ForEach(e => e.Dispose());
        }
    }

    public class AttributeContext
    {
        public ElementalType ElementalType { get; private set; }
        public object Sender { get; private set; }
        public object Target { get; private set; }
        public float Duration { get; private set; }
        public int Frequency { get; private set; }

        public AttributeContext(object sender, object target = null, ElementalType elementalType = ElementalType.Physical, float duration = -1, int frequency = -1)
        {
            ElementalType = elementalType;
            Sender = sender;
            Target = target;
            Duration = duration;
            Frequency = frequency;
        }
    }
}