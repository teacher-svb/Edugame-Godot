using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Larger ≥ Smaller (or > without AllowEqual), e.g. to keep `x - y` from going negative.
    [GlobalClass, Tool]
    public partial class Ordered : ChallengeConstraint
    {
        [Export] public string Larger { get; set; } = "x";
        [Export] public string Smaller { get; set; } = "y";
        [Export] public bool AllowEqual { get; set; } = true;

        public override bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values) =>
            AllowEqual ? values[Larger] >= values[Smaller] : values[Larger] > values[Smaller];

        public override IEnumerable<string> Params => [Larger, Smaller];

        public override string Describe() => $"{Larger} {(AllowEqual ? "≥" : ">")} {Smaller}";
    }
}
