using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    // Shows the picture aspect-fitted and keeps each annotation on its spot of the
    // picture (not of the widget) whenever the widget resizes.
    public partial class ImageVisualizationWidget : ChallengeVisualizationWidget<ImageVisualization>
    {
        TextureRect _image;
        readonly List<(ImageAnnotation annotation, Control label)> _labels = new();

        public override void _Ready()
        {
            _image = this.FindAnyObjectByType<TextureRect>();
            _image.Texture = Settings.Image;

            foreach (var annotation in Settings.Annotations.Where(a => a != null))
            {
                var label = CreateLabel(annotation, Round.Format(annotation.Text));
                AddChild(label);
                _labels.Add((annotation, label));
            }

            Resized += Layout;
            // labels only know their size after their first layout pass
            Callable.From(Layout).CallDeferred();
        }

        static Control CreateLabel(ImageAnnotation annotation, string text)
        {
            var label = new Label
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            label.AddThemeFontSizeOverride("font_size", annotation.FontSize);
            label.AddThemeColorOverride("font_color", annotation.TextColor);

            var plate = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore };
            plate.AddThemeStyleboxOverride("panel", annotation.ShowBackground
                ? new StyleBoxFlat
                {
                    BgColor = new Color(1, 1, 1, 0.8f),
                    CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                    ContentMarginLeft = 6, ContentMarginRight = 6, ContentMarginTop = 2, ContentMarginBottom = 2,
                }
                : new StyleBoxEmpty());
            plate.AddChild(label);
            return plate;
        }

        void Layout()
        {
            var imageRect = DrawnImageRect();
            foreach (var (annotation, label) in _labels)
            {
                label.Size = label.GetCombinedMinimumSize();
                label.Position = imageRect.Position + annotation.Position * imageRect.Size - label.Size / 2;
            }
        }

        // where the texture actually ends up inside the TextureRect (KeepAspectCentered)
        Rect2 DrawnImageRect()
        {
            if (_image.Texture == null)
                return new Rect2(_image.Position, _image.Size);

            var textureSize = _image.Texture.GetSize();
            var scale = Mathf.Min(_image.Size.X / textureSize.X, _image.Size.Y / textureSize.Y);
            var drawnSize = textureSize * scale;
            return new Rect2(_image.Position + (_image.Size - drawnSize) / 2, drawnSize);
        }
    }
}
