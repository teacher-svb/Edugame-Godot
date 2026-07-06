using System;
using System.Threading.Tasks;
using TnT.Systems.TimeSystem;

namespace TnT.EduGame
{
    /// <summary>
    /// Base class for stat modifiers that can be applied to a stat query.
    /// </summary>
    public abstract class StatModifier : IDisposable
    {
        public bool MarkedForRemoval { get; private set; }
        protected StatType StatType => _statType;
        protected ElementalType ElementalType => _elementalType;
        public event Action<StatModifier> OnDispose = delegate { };
        readonly CountdownTimer _timer;
        private StatType _statType;
        ElementalType _elementalType;

        /// <summary>
        /// Initializes a new instance of the StatModifier class.
        /// </summary>
        /// <param name="duration">Duration for which the modifier is active (in seconds). Defaults to 0.</param>
        protected StatModifier(StatType statType, ElementalType elementalType = ElementalType.None, float duration = -1)
        {
            _statType = statType;
            _elementalType = elementalType;

            if (duration <= 0)
                return;

            _timer = new CountdownTimer(duration);
            _timer.OnTimerStop += () => MarkedForRemoval = true;
            _timer.Start();
        }

        /// <summary>
        /// Handles a stat query and applies the modifier.
        /// </summary>
        /// <param name="sender">Object that triggered the stat query.</param>
        /// <param name="q">Stat query to be modified.</param>
        /// <param name="c">Context information for the stat change.</param>
        /// <returns>Modified result of the stat query.</returns>
        public int Handle(StatQuery q, StatContext c)
        {
            int result = Modify(q, c);
            if (_timer == null)
                MarkedForRemoval = true;
            // ConsumeUse();
            return result;
        }

        /// <summary>
        /// Updates the timer for the modifier.
        /// </summary>
        /// <param name="deltaTime">Elapsed time since last update.</param>
        public void Update(double deltaTime) => _timer?.Tick(deltaTime);

        /// <summary>
        /// Disposes of the modifier and raises the OnDispose event.
        /// </summary>
        public void Dispose() => OnDispose.Invoke(this);

        /// <summary>
        /// Modifies a stat query based on the specific implementation of the subclass.
        /// </summary>
        /// <param name="sender">Object that triggered the stat query.</param>
        /// <param name="q">Stat query to be modified.</param>
        /// <param name="c">Context information for the stat change.</param>
        /// <returns>Modified result of the stat query.</returns>
        public abstract int Modify(StatQuery q, StatContext c);
    }

    /// <summary>
    /// Basic implementation of a stat modifier that applies a simple arithmetic operation.
    /// </summary>
    public class BasicStatModifier : StatModifier
    {
        private Func<int, int> _operation;

        /// <summary>
        /// Initializes a new instance of the BasicStatModifier class.
        /// </summary>
        /// <param name="statType">Type of stat being modified.</param>
        /// <param name="operation">Arithmetic operation to apply.</param>
        /// <param name="duration">Duration for which the modifier is active (in seconds). Defaults to 0 (no duration).</param>
        public BasicStatModifier(StatType statType, Func<int, int> operation, float duration = -1/*, int uses = -1*/, ElementalType elementalType = ElementalType.None) : base(statType, elementalType: elementalType, duration: duration/*, uses: uses*/)
        {
            this._operation = operation;
        }

        /// <summary>
        /// Modifies a stat query based on the specific implementation of the subclass.
        /// </summary>
        /// <param name="sender">Object that triggered the stat query.</param>
        /// <param name="q">Stat query to be modified.</param>
        /// <param name="c">Context information for the stat change.</param>
        /// <returns>Modified result of the stat query.</returns>
        public override int Modify(StatQuery q, StatContext c)
        {
            if (q.statType != StatType) return q.Result;

            q.Result = _operation(q.Result);

            return q.Result;
        }
    }
}
