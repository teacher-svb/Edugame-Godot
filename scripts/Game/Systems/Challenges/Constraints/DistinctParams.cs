using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Question
{
    // No two formula params share a value (`answer` is not compared).
    [GlobalClass, Tool]
    public partial class DistinctParams : ChallengeConstraint
    {
        public override bool IsSatisfiedBy(IReadOnlyDictionary<string, int> values)
        {
            var paramValues = values.Where(kv => kv.Key != ChallengeRound.AnswerParam).Select(kv => kv.Value).ToList();
            return paramValues.Distinct().Count() == paramValues.Count;
        }

        public override IEnumerable<string> Params => [];

        public override string Describe() => "all params differ";
    }
}
