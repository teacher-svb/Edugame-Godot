using System.Linq;
using Godot;


namespace TnT.EduGame.Question
{
    public interface IMathChallenge
    {
        public string Name { get; }
        public string Formula { get; }
        public ChallengeVisualization Visualization { get; }
        public ChallengeInput Input { get; }
        public ChallengeRound Roll(int seed);
        public string QuestionFor(ChallengeRound round);
        public string HintFor(ChallengeRound round);
    }

    // What a challenge asks, independent of how it's shown or answered: a formula
    // whose result is the implicit `answer`, how each param gets its value, and
    // which params (possibly `answer` itself) the player has to supply. Values only
    // exist per ChallengeRound, rolled from a seed.
    [GlobalClass, Tool]
    public partial class MathChallenge : Resource, IMathChallenge
    {
        [Export]
        public string Name { get; set; } = "";

        [Export]
        public string Formula { get; set; } = "";

        // How each formula param is rolled; kept in sync with the formula by "Extract Params"
        [Export]
        public Godot.Collections.Array<ParamSpec> Params { get; set; } = new();

        // Params the player supplies; may include `answer` (the formula's result)
        [Export]
        public string[] Unknowns { get; set; } = [ChallengeRound.AnswerParam];

        // rules every round must satisfy, e.g. "answer ≥ 0" (ValueRange) or "x divisible by y"
        [Export]
        public Godot.Collections.Array<ChallengeConstraint> Constraints { get; set; } = new();

        [Export]
        public ChallengeVisualization Visualization { get; set; }

        [Export]
        public ChallengeInput Input { get; set; }

        // `{param}` / `{answer}` placeholders, unknowns render as `?`
        [Export(PropertyHint.MultilineText)]
        protected string QuestionText { get; set; } = "";
        [Export]
        protected string HintText { get; set; } = "";

        [ExportToolButton("Extract Params")]
        public Callable ExtractParamsFromFormulaBtn => Callable.From(ExtractParamsFromFormula);
        public void ExtractParamsFromFormula()
        {
            if (Formula == "")
                return;

            // keep authored specs for params that survive a formula edit
            var existing = (Params ?? new())
                .Where(p => p != null)
                .GroupBy(p => p.Name)
                .ToDictionary(g => g.Key, g => g.First());
            Params = new(ChallengeRound.ExtractParamNames(Formula)
                .Where(name => name != ChallengeRound.AnswerParam)
                .Select(name => existing.TryGetValue(name, out var spec) ? spec : new ParamSpec(name)));
        }

        public ChallengeRound Roll(int seed) =>
            Visualization != null
                ? Visualization.Generate(this, seed)
                : ChallengeRound.Roll(Formula, Params, Unknowns, seed, Constraints);

        public string QuestionFor(ChallengeRound round) => round.Format(QuestionText);
        public string HintFor(ChallengeRound round) => round.Format(HintText);
    }
}
