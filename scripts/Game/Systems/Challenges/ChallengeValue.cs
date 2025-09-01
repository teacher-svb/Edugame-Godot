
using Godot;

namespace TnT.EduGame.Question
{
    [GlobalClass, Tool]
    public partial class ChallengeValue : Resource
    {
        [Export] public int Value;
        [Export] public string ParamName;
        // empty constructor for Unity Editor
        public ChallengeValue() { }
        public ChallengeValue(int value, string paramName = "")
        {
            Value = value;
            ParamName = paramName;
        }
    }
}