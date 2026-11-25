
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Challenges;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class ChallengeModel : Node
    {
        public MathChallenge Challenge { get; private set; }
        public ChallengeRound Round { get; private set; }

        public string ChallengeName => Challenge?.Name;
        /// <summary>The round's hint with its values filled in; empty when the challenge has none.</summary>
        public string Hint => Challenge == null || Round == null ? "" : Challenge.HintFor(Round);

        readonly Dictionary<string, int> _answers = new();

        public bool IsComplete => Round != null && Round.Unknowns.All(_answers.ContainsKey);

        public void AssignValue(string paramName, int value) => _answers[paramName] = value;

        public void ClearValue(string paramName) => _answers.Remove(paramName);

        public bool CheckAnswer() => Round.Check(_answers);

        // rolls first: a challenge that can't be rolled leaves the model as it was
        public void SetChallenge(MathChallenge challenge, int seed)
        {
            var round = challenge.Roll(seed);
            Challenge = challenge;
            Round = round;
            _answers.Clear();
        }
    }
}
