using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Value lies on a step grid: Offset, Offset ± Step, Offset ± 2·Step, ...
    // e.g. multiples of 10, or times-table answers.
    [GlobalClass, Tool]
    public partial class MultipleOf : ChallengeConstraint
    {
        [Export] public string Param { get; set; } = ChallengeRound.AnswerParam;
        [Export] public int Step { get; set; } = 10;
        [Export] public int Offset { get; set; }

        public override bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values) =>
            Step == 0 ? values[Param] == Offset : (values[Param] - Offset) % Step == 0;

        public override IEnumerable<string> Params => [Param];

        public override string Describe() => Offset == 0 ? $"{Param} multiple of {Step}" : $"{Param} = {Offset} + k×{Step}";
    }
}
