using Godot;

namespace TnT.Systems.UI
{
    // Drawing helpers shared by the custom-drawn visualization widgets (_Draw).
    public static class ChallengeDrawing
    {
        // unit vector for an angle measured from straight up, clockwise
        public static Vector2 Direction(float angle) => Vector2.Up.Rotated(angle);

        public static Vector2 TextSize(this Font font, string text, int fontSize) =>
            font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);

        // text centred on `position`, optionally rotated around it
        public static void DrawTextCentered(this CanvasItem canvas, Font font, string text, Vector2 position,
            int fontSize, Color color, float rotation = 0)
        {
            var size = font.TextSize(text, fontSize);
            canvas.DrawSetTransform(position, rotation);
            canvas.DrawString(font, new Vector2(-size.X / 2, font.GetAscent(fontSize) - size.Y / 2), text,
                HorizontalAlignment.Left, -1, fontSize, color);
            canvas.DrawSetTransform(Vector2.Zero);
        }

        // Lays out a graph inside `size` (room for value labels on the left, an optional title
        // above, category/x labels and an x title below) and draws the paper, horizontal
        // gridlines with their values, and the axes. Returns the plot area for value mapping.
        public static GraphArea DrawValueAxis(this CanvasItem canvas, Font font, int fontSize, Vector2 size,
            int min, int max, int step, string title, GraphStyle style)
        {
            const float margin = 8f;
            var labelWidth = Mathf.Max(font.TextSize(min.ToString(), fontSize).X, font.TextSize(max.ToString(), fontSize).X);
            var lineHeight = font.GetHeight(fontSize);
            var top = margin + (title != "" ? lineHeight + margin : 0);
            var plot = new Rect2(labelWidth + margin * 2, top,
                size.X - labelWidth - margin * 3, size.Y - top - lineHeight * 2 - margin * 3);
            var area = new GraphArea(plot, min, max, margin, lineHeight);
            if (plot.Size.X <= 0 || plot.Size.Y <= 0 || max <= min)
                return area;

            canvas.DrawRect(plot, style.Paper);
            if (step > 0)
                for (var value = min; value <= max; value += step)
                {
                    var y = area.Y(value);
                    canvas.DrawLine(new Vector2(plot.Position.X, y), new Vector2(plot.End.X, y), style.Grid, 1);
                    var width = font.TextSize(value.ToString(), fontSize).X;
                    canvas.DrawTextCentered(font, value.ToString(), new Vector2(plot.Position.X - margin - width / 2, y), fontSize, style.Label);
                }

            if (title != "")
                canvas.DrawString(font, new Vector2(plot.Position.X - labelWidth - margin, margin + font.GetAscent(fontSize)), title,
                    HorizontalAlignment.Left, -1, fontSize, style.Label);

            canvas.DrawLine(plot.Position, new Vector2(plot.Position.X, plot.End.Y), style.Frame, 2);
            canvas.DrawLine(new Vector2(plot.Position.X, plot.End.Y), plot.End, style.Frame, 2);
            return area;
        }
    }

    public record struct GraphStyle(Color Paper, Color Grid, Color Label, Color Frame);

    // Plot area of a graph plus the mapping from values to pixels.
    public readonly struct GraphArea
    {
        public Rect2 Plot { get; }
        readonly int _min, _max;
        // spacing used around the plot, and the height of one text row
        public float Margin { get; }
        public float LineHeight { get; }

        public GraphArea(Rect2 plot, int min, int max, float margin, float lineHeight)
        {
            Plot = plot;
            _min = min;
            _max = max;
            Margin = margin;
            LineHeight = lineHeight;
        }

        public bool IsDrawable => Plot.Size.X > 0 && Plot.Size.Y > 0 && _max > _min;

        public float Y(double value) => Plot.End.Y - (float)((value - _min) / (_max - _min)) * Plot.Size.Y;

        // centre of the first text row below the plot (category / x labels), and of the second (x title)
        public float LabelRowY => Plot.End.Y + Margin + LineHeight / 2;
        public float TitleRowY => Plot.End.Y + Margin * 2 + LineHeight * 1.5f;
    }
}
