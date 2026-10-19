using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Draws the radar: screen, distance rings with labels, crosshair, a rotating
    // sweep with a fading trail and the blip, which flares as the sweep passes it.
    public partial class RadarWidget : ChallengeVisualizationWidget
    {
        [Export] public Color ScreenColor { get; set; } = new(0.04f, 0.14f, 0.07f);
        [Export] public Color RimColor { get; set; } = new(0.20f, 0.30f, 0.22f);
        [Export] public Color GridColor { get; set; } = new(0.30f, 0.90f, 0.40f, 0.55f);
        [Export] public Color LabelColor { get; set; } = new(0.55f, 1.00f, 0.60f);
        [Export] public Color SweepColor { get; set; } = new(0.30f, 1.00f, 0.40f, 0.35f);
        [Export] public Color BlipColor { get; set; } = new(0.75f, 1.00f, 0.75f);

        // seconds per sweep revolution
        [Export] public float SweepPeriod { get; set; } = 3f;

        const float Margin = 16f;
        const float RimWidth = 6f;
        const float TrailAngle = Mathf.Pi / 3;
        const int TrailSlices = 24;

        RadarVisualization _radar;
        float _blipAngle;
        float _sweep;

        public override void _Ready()
        {
            _radar = (RadarVisualization)Challenge.Visualization;
            _blipAngle = _radar.BlipAngle(Round);
        }

        public override void _Process(double delta)
        {
            _sweep = Mathf.Wrap(_sweep + (float)delta / SweepPeriod * Mathf.Tau, 0, Mathf.Tau);
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_radar == null)
                return;

            var center = Size / 2;
            var radius = Mathf.Min(Size.X, Size.Y) / 2 - Margin;
            if (radius <= 0)
                return;

            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(8, (int)(radius * 0.07f));

            DrawCircle(center, radius + RimWidth, RimColor);
            DrawCircle(center, radius, ScreenColor);

            DrawSweep(center, radius);

            // crosshair
            DrawLine(center + Vector2.Up * radius, center + Vector2.Down * radius, GridColor, 1, true);
            DrawLine(center + Vector2.Left * radius, center + Vector2.Right * radius, GridColor, 1, true);

            for (int i = 1; i <= _radar.RingCount; i++)
            {
                var ringRadius = radius * i / _radar.RingCount;
                DrawArc(center, ringRadius, 0, Mathf.Tau, 96, GridColor, 2, true);
                DrawRingLabel(font, fontSize, $"{i * _radar.RingStep} {_radar.Unit}", center + Direction(RadarVisualization.LabelAngle) * ringRadius);
            }

            // you are here
            DrawCircle(center, radius * 0.02f, LabelColor);

            DrawBlip(font, fontSize, center, radius);
        }

        void DrawSweep(Vector2 center, float radius)
        {
            for (int i = 0; i < TrailSlices; i++)
            {
                var from = _sweep - TrailAngle * (i + 1) / TrailSlices;
                var to = _sweep - TrailAngle * i / TrailSlices;
                var color = SweepColor with { A = SweepColor.A * (1 - (float)i / TrailSlices) };
                DrawColoredPolygon([center, center + Direction(from) * radius, center + Direction(to) * radius], color);
            }
            DrawLine(center, center + Direction(_sweep) * radius, SweepColor with { A = 1 }, 2, true);
        }

        void DrawBlip(Font font, int fontSize, Vector2 center, float radius)
        {
            var distance = Round.Values[_radar.DistanceParam];
            var position = center + Direction(_blipAngle) * radius * distance / _radar.Range;

            // brightest right after the sweep passed, fading over one revolution
            var sinceSweep = Mathf.PosMod(_sweep - _blipAngle, Mathf.Tau) / Mathf.Tau;
            var flare = 1 - sinceSweep;

            var size = radius * 0.035f;
            DrawCircle(position, size * (1.6f + flare), BlipColor with { A = 0.15f + 0.35f * flare });
            DrawCircle(position, size, BlipColor);

            if (_radar.BlipLabel != "")
                DrawString(font, position + new Vector2(size * 1.8f, fontSize * 0.35f), Round.Format(_radar.BlipLabel),
                    HorizontalAlignment.Left, -1, fontSize, LabelColor);
        }

        // centred on the ring, on a screen-coloured plate so the ring line doesn't cross the text
        void DrawRingLabel(Font font, int fontSize, string text, Vector2 position)
        {
            var textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
            var plate = new Rect2(position - textSize / 2 - new Vector2(3, 1), textSize + new Vector2(6, 2));
            DrawRect(plate, ScreenColor);
            DrawString(font, new Vector2(position.X - textSize.X / 2, position.Y - textSize.Y / 2 + font.GetAscent(fontSize)),
                text, HorizontalAlignment.Left, -1, fontSize, LabelColor);
        }

        // 0 = up, clockwise
        static Vector2 Direction(float angle) => Vector2.Up.Rotated(angle);
    }
}
