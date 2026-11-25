using System;
using System.Collections.Generic;
using System.Linq;

namespace TnT.EduGame
{
    public class AttributesMediator
    {
        private readonly List<AttributeModifier> modifiers = new();
        public event Action<AttributeType, int, AttributeContext> OnAttributeModified = delegate { };

        /// <summary>Whether a modifier is held (an expired one until the next <see cref="Update"/>); without one, <see cref="Update"/> has nothing to do.</summary>
        public bool HasModifiers => modifiers.Count > 0;

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
            modifiers.ToList().ForEach(m => m.Update(deltaTime));   // a copy: an OnApply handler may add a modifier
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