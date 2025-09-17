using Godot;

namespace TnT.Systems.UI
{
    public partial class ChallengeValueInput : SpinBox
    {
        public int Index { get; set; } = 0;
        public string ParamName { get; set; }
        public void Init(string paramName)
        {
            this.Value = 0;
            this.ParamName = paramName;
            this.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            this.Alignment = HorizontalAlignment.Center;
        }
    }
}