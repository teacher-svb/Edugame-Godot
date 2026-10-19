using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class TextVisualizationWidget : ChallengeVisualizationWidget
    {
        public override void _Ready()
        {
            var label = this.FindAnyObjectByType<Label>();
            label.Text = ((TextVisualization)Challenge.Visualization).Render(Challenge, Round);
        }
    }
}
