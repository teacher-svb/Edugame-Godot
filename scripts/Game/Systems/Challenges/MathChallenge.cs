using System.Linq;
using Godot;


namespace TnT.EduGame.Question
{
    // What a challenge asks, independent of how it's shown or answered: a formula
    // whose result is the implicit `answer`, how each param gets its value, and
    // which params (possibly `answer` itself) the player has to supply. Values only
    // exist per ChallengeRound, rolled from a seed.
    [GlobalClass, Tool]
    public partial class MathChallenge : Resource
    {
        [Export]
        public string Name { get; set; } = "";

        [Export]
        public string Formula { get; set; } = "";

        // How each formula param is rolled; kept in sync with the formula by "Extract Params"
        [Export]
        public Godot.Collections.Array<ParamSpec> Params
        {
            get => _params;
            set { _params = value; ShareParamNames(); }
        }
        Godot.Collections.Array<ParamSpec> _params = new();

        // Params the player supplies; may include `answer` (the formula's result)
        [Export]
        public string[] Unknowns { get; set; } = [ChallengeRound.AnswerParam];

        // Rules every round must satisfy, true/false expressions in the formula's language:
        // `answer >= 0`, `x > y`, `x % y = 0`, `answer % 10 = 0`, `distinct(x, y, z)`,
        // `x % 10 + y % 10 < 10` (no tens crossing), combined with AND / OR / NOT
        [Export]
        public string[] Constraints { get; set; } = [];

        // empty means the question text alone carries the challenge
        [Export]
        public ChallengeVisualization Visualization
        {
            get => _visualization;
            set { _visualization = value; ShareParamNames(); }
        }
        ChallengeVisualization _visualization;

        public ChallengeVisualization VisualizationOrNone => Visualization ?? NoVisualization.Instance;

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
            Params = new(FormulaEvaluator.ExtractParamNames(Formula)
                .Where(name => name != ChallengeRound.AnswerParam)
                .Select(name => existing.TryGetValue(name, out var spec) ? spec : new ParamSpec(name)));
        }

        // lets the visualization offer the formula's params (plus `answer`) as a
        // dropdown in the inspector; runs whenever Params or Visualization is (re)assigned
        void ShareParamNames()
        {
            var names = (_params ?? new())
                .Where(p => p != null && p.Name != "")
                .Select(p => p.Name)
                .Append(ChallengeRound.AnswerParam)
                .ToArray();

            _visualization?.SetAvailableParams(names);
        }

        public ChallengeRound Roll(int seed) => VisualizationOrNone.Generate(this, seed);

        public string QuestionFor(ChallengeRound round) => round.Format(QuestionText);
        public string HintFor(ChallengeRound round) => round.Format(HintText);
    }
}
