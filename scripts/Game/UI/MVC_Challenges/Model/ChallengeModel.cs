
using System.Collections.Generic;
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

        public string ChallengeName => Challenge.Name;
        public string Question => Challenge.Question;
        public string Hint => Challenge.Hint;
        public int ParamCount => Challenge.ParamCount;

        readonly Dictionary<string, int> _answers = new();

        public bool IsComplete => _answers.Count == Challenge.FormulaParams.Length;

        public void AssignValue(string paramName, int value)
        {
            _answers[paramName] = value;
            Challenge.ChangeValue(paramName, value);
        }

        public void SetChallenge(MathChallenge challenge)
        {
            Challenge = challenge;
            _answers.Clear();
        }
    }
}
