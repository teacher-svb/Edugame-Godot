using System.Text.RegularExpressions;
using Godot;

namespace TnT.EduGame.Question
{
    // Shows the challenge as a written-out sum, e.g. `5 - 3 = ?`.
    [GlobalClass, Tool]
    public partial class TextVisualization : ChallengeVisualization
    {
        // `{param}` / `{answer}` placeholders; empty means "formula = answer"
        [Export(PropertyHint.MultilineText)]
        public string Template { get; set; } = "";

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Text.tscn";

        public string Render(IMathChallenge challenge, ChallengeRound round)
        {
            var template = Template != ""
                ? Template
                : $"{Regex.Replace(challenge.Formula, @"\b[a-zA-Z_][a-zA-Z0-9_]*\b", m => $"{{{m.Value}}}")} = {{{ChallengeRound.AnswerParam}}}";

            return PrettifyOperators(round.Format(template));
        }

        // school notation rather than programmer notation
        static string PrettifyOperators(string text) => text.Replace("*", "×").Replace("/", ":");
    }
}
