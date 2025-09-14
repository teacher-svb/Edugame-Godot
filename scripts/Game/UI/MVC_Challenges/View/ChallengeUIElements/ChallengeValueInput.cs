using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeValueInput : LineEdit
    {
        public int Index { get; set; } = 0;
        public string ParamName { get; set; }
        public void Init(string paramName)
        {
            this.Text = "Klaar!";
            this.ParamName = paramName;
        }
    }
}