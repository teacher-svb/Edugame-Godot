using System;
using System.Collections.Generic;
using System.Linq;

namespace TnT.EduGame
{
    /// <summary>
    /// Mediator for stats and stat queries.
    /// </summary>
    public class StatsMediator
    {
        readonly List<StatModifier> modifiers = new();

        /// <summary>
        /// Event invoked when a stat query is made.
        /// </summary>
        /// <param name="query">The stat query being made.</param>
        /// <param name="context">Optional context for the query. When null, the default value is used.</param>
        public event Func<StatQuery, StatContext, int> Queries;

        /// <summary>
        /// Makes a stat query.
        /// </summary>
        /// <param name="query">The stat query being made.</param>
        /// <param name="context">Optional context for the query.</param>
        /// <returns>The result of the stat query.</returns>
        public int Query(StatQuery query, StatContext context = null) => Queries?.Invoke(query, context) ?? query.Result;

        /// <summary>
        /// Adds a modifier to this mediator.
        /// </summary>
        /// <param name="mod">The modifier to add.</param>
        public void AddModifier(StatModifier mod)
        {
            modifiers.Add(mod);
            Queries += mod.Handle;

            mod.OnDispose += _ =>
            {
                modifiers.Remove(mod);
                Queries -= mod.Handle;
            };
        }

        /// <summary>
        /// Updates the stats for the given time delta.
        /// </summary>
        /// <param name="deltaTime">The time delta to update.</param>
        public void Update(double deltaTime)
        {
            modifiers.ForEach(m => m.Update(deltaTime));

            modifiers
                .Where(m => m.MarkedForRemoval)
                .ToList()
                .ForEach(m =>  m.Dispose());
        }
    }

    /// <summary>
    /// Represents a stat query.
    /// </summary>
    public class StatQuery
    {
        /// <summary>
        /// The type of the stat being queried.
        /// </summary>
        public readonly StatType statType;
        /// <summary>
        /// The result of the stat query.
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Creates a new stat query.
        /// </summary>
        /// <param name="statType">The type of stat being queried.</param>
        /// <param name="value">The result of the stat query.</param>
        public StatQuery(StatType statType, int value)
        {
            this.statType = statType;
            this.Result = value;
        }
    }

    /// <summary>
    /// Context for a stat modifier, including information about the damage type and source/target.
    /// → Query = what you're querying (Defense, Attack, Health)
    /// → StatContext = the situation (damage type, source, target, etc.)
    /// </summary>
    public class StatContext
    {
        /// <summary>
        /// Type of damage applied (e.g., "physical", "magical", etc.).
        /// </summary>
        public ElementalType ElementalType { get; private set; }
        /// <summary>
        /// Source of the stat change (e.g., player, enemy, item, etc.).
        /// </summary>
        public object Sender { get; private set; }
        /// <summary>
        /// Target of the stat change (e.g., player, enemy, item, etc.).
        /// </summary>
        public object Target { get; private set; }

        public StatContext(object sender, object target = null, ElementalType elementalType = ElementalType.Physical)
        { 
            ElementalType = elementalType;
            Sender = sender;
            Target = target;
        }
    }
}