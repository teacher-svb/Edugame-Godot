
using System;
using System.Collections.Generic;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class ChallengeModel : Resource
    {
        [Export]
        MathChallenge _challenge;
        public IMathChallenge Challenge => _challenge;

        public string Name => Challenge.Name;
        public string Question => Challenge.Question;
        public string Hint => Challenge.Hint;
        public int ParamCount => Challenge.ParamCount;

        Queue<string> _formulaParams;

        public void SetParameter(int index, string name)
        {
            Challenge.SetFormulaParam(index, name);
        }
        public void SetParameter( string name, int value)
        {
            Challenge.ChangeValue(name, value);
        }
        public void SetParameter(int index)
        {
            if (_formulaParams == null)
                _formulaParams = new(Challenge?.FormulaParams);

            if (Challenge.Values[index].ParamName != "")
                return;

            string name = _formulaParams.Dequeue();
            _formulaParams.Enqueue(name);
            this.SetParameter(index, name);
        }
        public void SetChallenge(MathChallenge challenge)
        {
            _challenge = challenge;
        }
    }
}