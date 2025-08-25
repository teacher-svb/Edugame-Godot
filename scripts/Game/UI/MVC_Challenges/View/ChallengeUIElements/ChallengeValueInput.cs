using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeValueInput : LineEdit
    {
        int _index = 0;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                // this.RemoveFromClassList($"input-{Index}");
                _index = value;
                // this.AddClass($"value-{Index}");
            }
        }
        public string ParamName { get; set; }
        public ChallengeValueInput(string paramName)
        {
            this.Text = "Klaar!";
            this.ParamName = paramName;
        }
    }
}