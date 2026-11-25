using System.Text.RegularExpressions;
using Godot;

namespace TnT.EduGame.Challenges
{
    // Shows the challenge as a written-out sum, e.g. `5 - 3 = ?`.
    [GlobalClass, Tool]
    public partial class TextVisualization : ChallengeVisualization
    {
        // `{param}` / `{answer}` placeholders; empty means "formula = answer". Shown as written
        // (e.g. `{d} km/u`); only the generated formula gets school notation.
        [Export(PropertyHint.MultilineText)]
        public string Template { get; set; } = "";

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Text.tscn";

        public string Render(MathChallenge challenge, ChallengeRound round)
        {
            var template = Template != ""
                ? Template
                : $"{PrettifyOperators(Regex.Replace(challenge.Formula, FormulaEvaluator.ParamPattern, m => $"{{{m.Value}}}"))} = {{{ChallengeRound.AnswerParam}}}";

            return round.Format(template);
        }

        // school notation rather than programmer notation
        static string PrettifyOperators(string text) => text.Replace("*", "×").Replace("/", ":");
    }
}
