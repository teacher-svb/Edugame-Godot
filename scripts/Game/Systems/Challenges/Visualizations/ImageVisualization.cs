using Godot;

namespace TnT.EduGame.Question
{
    // A custom picture with the round's values written onto it at authored spots,
    // e.g. a floor plan with `{x} m` along the walls and `?` in the middle.
    [GlobalClass, Tool]
    public partial class ImageVisualization : ChallengeVisualization
    {
        [Export] public Texture2D Image { get; set; }

        [Export] public Godot.Collections.Array<ImageAnnotation> Annotations { get; set; } = new();

        protected override string DefaultScenePath => "res://Scenes/UI/ChallengeVisualizations/Image.tscn";
    }
}
