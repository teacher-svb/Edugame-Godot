using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Keeps a value (by default the answer) within bounds, e.g. "only positive answers".
    [GlobalClass, Tool]
    public partial class ValueRange : ChallengeConstraint
    {
        [Export] public string Param { get; set; } = ChallengeRound.AnswerParam;
        [Export] public int Min { get; set; }
        [Export] public bool HasMax { get; set; }
        [Export] public int Max { get; set; } = 100;

        public override bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values) =>
            values[Param] >= Min && (!HasMax || values[Param] <= Max);

        public override IEnumerable<string> Params => [Param];
        protected override IEnumerable<string> ParamFields => [nameof(Param)];

        public override string Describe() => HasMax ? $"{Min} ≤ {Param} ≤ {Max}" : $"{Param} ≥ {Min}";
    }
}
