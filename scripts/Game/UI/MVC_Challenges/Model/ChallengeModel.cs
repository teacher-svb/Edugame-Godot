
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeModel : Node
    {
        [Export]
        MathChallenge _challenge;
        public MathChallenge Challenge { get => _challenge; set => _challenge = value; }
        public ChallengeRound Round { get; private set; }

        public string ChallengeName => Challenge.Name;
        public string Question => Challenge.QuestionFor(Round);
        public string Hint => Challenge.HintFor(Round);

        readonly Dictionary<string, int> _answers = new();

        public bool IsComplete => Round != null && Round.Unknowns.All(_answers.ContainsKey);

        public void AssignValue(string paramName, int value) => _answers[paramName] = value;

        public void ClearValue(string paramName) => _answers.Remove(paramName);

        public bool CheckAnswer() => Round.Check(_answers);

        public void SetChallenge(MathChallenge challenge, int seed)
        {
            Challenge = challenge;
            Round = challenge.Roll(seed);
            _answers.Clear();
        }
    }
}
