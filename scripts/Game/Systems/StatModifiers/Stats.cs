namespace TnT.EduGame
{

    public enum StatType
    {
        Attack,
        Defense,
        MaxHealth,
        Resistance
    }
    public class Stats
    {
        readonly BaseStats _baseStats;
        readonly StatsMediator _mediator = new();
        public StatsMediator Mediator => _mediator;

        public int Attack(StatContext c = null) => _mediator.Query(new(StatType.Attack, _baseStats.attack), c ?? new(sender: this));
        public int Defense(StatContext c = null) => _mediator.Query(new(StatType.Defense, _baseStats.defense), c ?? new(sender: this));
        public int MaxHealth(StatContext c = null) => _mediator.Query(new(StatType.MaxHealth, _baseStats.maxHealth), c ?? new(sender: this));
        public int Resistance(ElementalType type, StatContext c = null) => _mediator.Query(new(StatType.Resistance, _baseStats.Resistances[type]), c ?? new(sender: this, elementalType: type));

        public Stats(BaseStats baseStats)
        {
            _baseStats = baseStats;
        }

        public override string ToString() => $"Attack: {Attack()}, Defense: {Defense()}, Health: {MaxHealth()}";
    }
}