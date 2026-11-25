using System;
using TnT.EduGame.Characters;
using TnT.Systems.TimeSystem;

namespace TnT.EduGame
{

    /// <summary>
    /// A change to an attribute (e.g. damage or healing). It is applied once when added to an
    /// <see cref="AttributesMediator"/>; with a duration (<see cref="AttributeContext.Duration"/> &gt; 0) and a frequency
    /// (<see cref="AttributeContext.Frequency"/> &gt; 0) it is applied again that many times per second until the
    /// duration ends. Without a duration it is applied once and expires.
    /// </summary>
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

            // no timers without a duration: applied once (Handle expires it). No tick timer without a frequency: the
            // default -1 made a -1 s tick timer, which stopped and re-applied the modifier every frame (finding 7.17).
            if (_context.Duration > 0)
            {
                _timer = new CountdownTimer(_context.Duration);
                if (_context.Frequency > 0)
                    _tickTimer = new CountdownTimer(1.0 / _context.Frequency);
            }
        }
        internal void Init()
        {
            if (_timer != null)
            {
                _timer.OnTimerStop += () => IsExpired = true;
                _timer.Start();
            }

            if (_tickTimer != null)
            {
                _tickTimer.OnTimerStop += () =>
                {
                    Handle();
                    if (!IsExpired)
                        _tickTimer.Start();
                };
                _tickTimer.Start();
            }
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
        public BasicAttributeModifier(AttributeType attributeType, Func<int, int> effect, AttributeContext context) : base(attributeType, context)
        {
            _effect = effect;
        }

        protected override int Apply(AttributeContext c)
        {
            if (c.Target is Character3D)
            {
                var target = c.Target as Character3D;
                var attr = target.Attributes.Get(attributeType);
                return _effect.Invoke(attr);
            }
            return _effect.Invoke(0);
        }
    }
}
