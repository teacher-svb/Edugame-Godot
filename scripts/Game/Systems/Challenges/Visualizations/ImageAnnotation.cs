using Godot;

namespace TnT.EduGame.Question
{
    // A label placed on an ImageVisualization's picture.
    [GlobalClass, Tool]
    public partial class ImageAnnotation : Resource
    {
        // `{param}` / `{answer}` placeholders, unknowns render as `?`; e.g. "{x} m"
        [Export] public string Text { get; set; } = "";

        // centre of the label in image coordinates: (0,0) top-left, (1,1) bottom-right,
        // so it stays on the same spot whatever size the image is shown at
        [Export] public Vector2 Position { get; set; } = new(0.5f, 0.5f);

        [Export] public int FontSize { get; set; } = 24;
        [Export] public Color TextColor { get; set; } = new(0.20f, 0.13f, 0.07f);

        // light plate behind the text, for readability on busy pictures
        [Export] public bool ShowBackground { get; set; } = true;
    }
}
