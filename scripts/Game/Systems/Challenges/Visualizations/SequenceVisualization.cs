using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

namespace TnT.EduGame.Challenges
{
    // A number series with one term missing (card challenge-sequence), e.g. the school bell times
    // 15, 60, 105, ?: the player finds the rule. Each term is `Term` with `i` the term's index (0 for
    // the first), over the formula's params; how many terms and which one is missing are expressions
    // too, so they can come from the round (e.g. `n + 1` terms, the last one missing).
    // The formula still decides the answer: the missing term must be the `MissingValue` param
    // (`answer` by default), so formula `s + n * d` goes with Term `s + i * d` and MissingTerm `n`.
    // Generate checks that and fails with a clear message when they don't match.
    [GlobalClass, Tool]
    public partial class SequenceVisualization : ChallengeVisualization
    {
        public const string IndexName = "i";

        // the value of term `i`, e.g. `a + i * d` or `a * (i + 1)`; formula params allowed, not `answer`
        [Export] public string Term { get; set; } = "a + i * d";

        // how many terms are shown: a number or an expression over the params, e.g. `n + 1`
        [Export] public string TermCount { get; set; } = "5";

        // index of the term shown as `?`: an expression, or a negative number to count from the end (-1 = last)
        [Export] public string MissingTerm { get; set; } = "-1";

        // the param the missing term stands for
        [Export] public string MissingValue { get; set; } = ChallengeRound.AnswerParam;

        // shown after every term, e.g. "min"; empty for none
        [Export] public string Unit { get; set; } = "";

        // more terms don't fit next to each other
        [Export(PropertyHint.Range, "2,12")] public int MaxTerms { get; set; } = 8;

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Sequence.tscn";

        protected override IEnumerable<string> ParamFields => [nameof(MissingValue)];

        // the terms must fit and the missing one must be one of them
        public override IEnumerable<string> ImpliedConstraints()
        {
            var count = $"({TermCount})";
            var missing = $"({MissingExpression})";
            return [$"{count} >= 2 AND {count} <= {MaxTerms}", $"{missing} >= 0 AND {missing} <= {count} - 1"];
        }

        public override ChallengeRound Generate(MathChallenge challenge, int seed)
        {
            var round = base.Generate(challenge, seed);
            int missing = MissingIndex(round);
            int term = TermAt(round, missing);
            if (!round.Values.TryGetValue(MissingValue, out var value))
                throw new ArgumentException($"sequence MissingValue '{MissingValue}' is not in formula '{challenge.Formula}'");
            if (term != value)
                throw new InvalidOperationException(
                    $"sequence term {missing} ({Term} = {term}) is not {MissingValue} ({value}): formula '{challenge.Formula}' and the sequence don't match");
            return round;
        }

        // a negative MissingTerm counts from the end
        string MissingExpression =>
            int.TryParse(MissingTerm.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var k) && k < 0
                ? $"({TermCount}) - {-k}"
                : MissingTerm;

        public int Count(ChallengeRound round) => Evaluate(TermCount, round.Values, nameof(TermCount));

        public int MissingIndex(ChallengeRound round) => Evaluate(MissingExpression, round.Values, nameof(MissingTerm));

        public int TermAt(ChallengeRound round, int index)
        {
            if (round.Values.ContainsKey(IndexName))
                throw new ArgumentException($"sequence: the formula has a param '{IndexName}', which Term uses for the term index; rename the param");
            var values = new Dictionary<string, int>(round.Values) { [IndexName] = index };
            return Evaluate(Term, values, nameof(Term));
        }

        public IReadOnlyList<int> Terms(ChallengeRound round) =>
            Enumerable.Range(0, Count(round)).Select(i => TermAt(round, i)).ToList();

        public string Label(int value) => Unit == "" ? value.ToString(CultureInfo.InvariantCulture) : $"{value} {Unit}";

        static int Evaluate(string expression, IReadOnlyDictionary<string, int> values, string field)
        {
            var unknown = FormulaEvaluator.ExtractParamNames(expression).Where(n => !values.ContainsKey(n)).ToList();
            if (unknown.Any())
                throw new ArgumentException($"sequence {field} '{expression}' uses unknown names: {string.Join(", ", unknown)}");
            if (!FormulaEvaluator.TryEvaluate(expression, values, out var result))
                throw new ArgumentException($"sequence {field} '{expression}' can't be evaluated");
            return result;
        }
    }
}
