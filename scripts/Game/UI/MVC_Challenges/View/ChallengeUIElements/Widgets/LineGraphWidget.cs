using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Draws the line graph: vertical gridlines with x values, the line drawn in from left
    // to right (clipped to the plot), and the marked point with dashed guides to both axes.
    public partial class LineGraphWidget : ChallengeVisualizationWidget<LineGraphVisualization>
    {
        [Export] public Color PaperColor { get; set; } = new(0.97f, 0.94f, 0.86f);
        [Export] public Color GridColor { get; set; } = new(0.35f, 0.25f, 0.15f, 0.25f);
        [Export] public Color LabelColor { get; set; } = new(0.20f, 0.13f, 0.07f);
        [Export] public Color FrameColor { get; set; } = new(0.35f, 0.25f, 0.15f);
        [Export] public Color LineColor { get; set; } = new(0.20f, 0.40f, 0.70f);
        [Export] public Color MarkerColor { get; set; } = new(0.80f, 0.20f, 0.15f);

        // seconds to draw the line in
        [Export] public float DrawDuration { get; set; } = 0.8f;

        // samples per x gridline step, so curves look smooth
        const int SamplesPerStep = 4;
        const int MaxSamples = 400;

        readonly List<Vector2> _points = new();
        bool _hasMark;
        Vector2 _mark;
        float _reveal;

        public override void _Ready()
        {
            var span = Settings.XMax - Settings.XMin;
            var samples = Mathf.Clamp(span / Mathf.Max(Settings.XStep, 1) * SamplesPerStep, 1, MaxSamples);
            for (int i = 0; i <= samples; i++)
            {
                var x = Settings.XMin + (double)span * i / samples;
                if (Settings.TryValueAt(Round, x, out var y))
                    _points.Add(new Vector2((float)x, (float)y));
            }

            if (Settings.TryMarkedX(Round, out var markX) && Settings.TryValueAt(Round, markX, out var markY))
            {
                _hasMark = true;
                _mark = new Vector2(markX, (float)markY);
            }
        }

        public override void _Process(double delta)
        {
            if (_reveal >= 1)
                return;
            _reveal = Mathf.Min(1, _reveal + (float)delta / Mathf.Max(DrawDuration, 0.01f));
            QueueRedraw();
        }

        public override void _Draw()
        {
            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(10, (int)(Mathf.Min(Size.X, Size.Y) * 0.04f));
            var area = this.DrawValueAxis(font, fontSize, Size, Settings.YMin, Settings.YMax, Settings.YStep, Settings.YTitle,
                new GraphStyle(PaperColor, GridColor, LabelColor, FrameColor));
            if (!area.IsDrawable || Settings.XMax <= Settings.XMin)
                return;

            var plot = area.Plot;
            float X(double x) => plot.Position.X + (float)((x - Settings.XMin) / (Settings.XMax - Settings.XMin)) * plot.Size.X;

            if (Settings.XStep > 0)
                for (var x = Settings.XMin; x <= Settings.XMax; x += Settings.XStep)
                {
                    DrawLine(new Vector2(X(x), plot.Position.Y), new Vector2(X(x), plot.End.Y), GridColor, 1);
                    this.DrawTextCentered(font, x.ToString(), new Vector2(X(x), area.LabelRowY), fontSize, LabelColor);
                }
            if (Settings.XTitle != "")
                this.DrawTextCentered(font, Settings.XTitle, new Vector2(plot.GetCenter().X, area.TitleRowY), fontSize, LabelColor);

            // reveal left to right, then keep only the part inside the plot (a line leaving the graph is cut off)
            var revealX = Settings.XMin + (Settings.XMax - Settings.XMin) * _reveal;
            var line = _points.Where(p => p.X <= revealX).Select(p => new Vector2(X(p.X), area.Y(p.Y))).ToArray();
            Vector2[] bounds = [plot.Position, new(plot.End.X, plot.Position.Y), plot.End, new(plot.Position.X, plot.End.Y)];
            if (line.Length >= 2)
                foreach (var piece in Geometry2D.IntersectPolylineWithPolygon(line, bounds))
                    if (piece.Length >= 2)
                        DrawPolyline(piece, LineColor, 3, true);

            if (_hasMark && _reveal >= 1)
                DrawMark(font, fontSize, new Vector2(X(_mark.X), area.Y(_mark.Y)), plot);
        }

        void DrawMark(Font font, int fontSize, Vector2 point, Rect2 plot)
        {
            DrawDashedLine(new Vector2(point.X, plot.End.Y), point, MarkerColor, 2, 6);
            DrawDashedLine(new Vector2(plot.Position.X, point.Y), point, MarkerColor, 2, 6);
            DrawCircle(point, 7, MarkerColor);
            DrawCircle(point, 3, PaperColor);

            var label = Round.Format(Settings.MarkLabel);
            if (label != "")
                this.DrawTextCentered(font, label, point + new Vector2(16, -16), (int)(fontSize * 1.3f), MarkerColor);
        }
    }
}
