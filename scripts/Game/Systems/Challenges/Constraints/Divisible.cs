using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Dividend / Divisor has no remainder (and Divisor isn't 0).
    [GlobalClass, Tool]
    public partial class Divisible : ChallengeConstraint
    {
        [Export] public string Dividend { get; set; } = "x";
        [Export] public string Divisor { get; set; } = "y";

        public override bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values) =>
            values[Divisor] != 0 && values[Dividend] % values[Divisor] == 0;

        public override IEnumerable<string> Params => [Dividend, Divisor];
        protected override IEnumerable<string> ParamFields => [nameof(Dividend), nameof(Divisor)];

        public override string Describe() => $"{Dividend} divisible by {Divisor}";
    }
}
