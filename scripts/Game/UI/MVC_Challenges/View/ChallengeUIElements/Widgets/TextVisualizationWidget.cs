using Godot;
using TnT.EduGame.Challenges;
using TnT.Extensions;

namespace TnT.EduGame.UI
{
    public partial class TextVisualizationWidget : ChallengeVisualizationWidget<TextVisualization>
    {
        public override void _Ready()
        {
            var label = this.FindAnyObjectByType<Label>();
            label.Text = Settings.Render(Challenge, Round);
        }
    }
}
