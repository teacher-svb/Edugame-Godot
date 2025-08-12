using System;
using TnT.EduGame.Characters;
using TnT.Systems.TimeSystem;
using Godot;

namespace TnT.EduGame
{

    public abstract class AttributeModifier : IDisposable
    {
        public bool IsExpired { get; private set; }
        protected AttributeType attributeType => _attributeType;
        public event Action<AttributeModifier> OnDispose = delegate { };
        public event Action<AttributeType, int, AttributeContext> OnApply = delegate { };
        readonly CountdownTimer _timer;
        readonly CountdownTimer _tickTimer;
        private readonly AttributeType _attributeType;
        AttributeContext _context;
        protected AttributeModifier(AttributeType attributeType, AttributeContext context)
        {
            _attributeType = attributeType;
            _context = context;

            _timer = new CountdownTimer(_context.Duration);
            _tickTimer = new CountdownTimer(1f / _context.Frequency);
        }
        internal void Init()
        {
            _timer.OnTimerStop += () => IsExpired = true;
            _timer.Start();

            _tickTimer.OnTimerStop += () =>
            {
                Handle();
                if (!IsExpired)
                    _tickTimer.Start();
            };
            _tickTimer.Start();
            Handle();
        }
        public void Handle()
        {
            if (IsExpired)
                return;
            int result = Apply(_context);
            if (_timer == null)
                IsExpired = true;
            OnApply?.Invoke(_attributeType, result, _context);
        }

        public void Update(double deltaTime)
        {
            _timer?.Tick(deltaTime);
            _tickTimer?.Tick(deltaTime);
        }
        public void Dispose() => OnDispose.Invoke(this);

        protected abstract int Apply(AttributeContext c);

    }

    public class BasicAttributeModifier : AttributeModifier
    {
        Func<int, int> _effect;
        public BasicAttributeModifier(AttributeType attributeType, Func<int, int> effect, AttributeContext context, int duration = -1/*, int uses = 1*/) : base(attributeType, context)
        {
            _effect = effect;
        }

        protected override int Apply(AttributeContext c)
        {
            if (c.Target is Character)
            {
                var target = c.Target as Character;
                var attr = target.Attributes.Get(attributeType);
                return _effect.Invoke(attr);
            }
            return _effect.Invoke(0);
        }
    }
}
