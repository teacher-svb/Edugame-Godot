using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;
using TnT.EduGame.Question;
using static TnT.EduGame.Question.ConcentricCirclesVisualization;

namespace TnT.Systems.UI
{
    // Draws the rings outer → inner around a centre disc, spins each one to its
    // selected segment (staggered, alternating direction) and locks it in under
    // the window at the top.
    public partial class ConcentricCirclesWidget : ChallengeVisualizationWidget
    {
        [Export] public Color NumberRingColor { get; set; } = new(0.93f, 0.86f, 0.70f);
        [Export] public Color OperatorRingColor { get; set; } = new(0.78f, 0.62f, 0.42f);
        [Export] public Color CenterColor { get; set; } = new(0.98f, 0.95f, 0.88f);
        [Export] public Color LineColor { get; set; } = new(0.35f, 0.25f, 0.15f);
        [Export] public Color TextColor { get; set; } = new(0.20f, 0.13f, 0.07f);
        [Export] public Color LockedColor { get; set; } = new(0.32f, 0.53f, 0.21f);
        [Export] public Color WindowColor { get; set; } = new(1f, 1f, 1f, 0.35f);

        // full turns each ring makes before landing
        const int SpinTurns = 2;
        // extra seconds per ring further inwards
        const float StaggerPerRing = 0.35f;
        const float LockPulseDuration = 0.25f;
        const float Margin = 16f;
        // operator glyphs are tiny next to digits in the pixel font
        const float OperatorFontScale = 1.8f;

        IReadOnlyList<Ring> _rings;
        float[] _angles;
        // 0 while spinning, rises to 1 during the lock-in pulse
        float[] _lock;

        public override void _Ready()
        {
            var visualization = (ConcentricCirclesVisualization)Challenge.Visualization;
            _rings = visualization.BuildRings(Round);
            _angles = new float[_rings.Count];
            _lock = new float[_rings.Count];

            var rng = new Random(Round.Seed);
            for (int i = 0; i < _rings.Count; i++)
                _angles[i] = (float)(rng.NextDouble() * Mathf.Tau);

            for (int i = 0; i < _rings.Count; i++)
                _ = Spin(i, visualization.SpinDuration + i * StaggerPerRing);
        }

        async Task Spin(int ring, float duration)
        {
            var step = Mathf.Tau / _rings[ring].Segments.Count;
            var direction = ring % 2 == 0 ? 1 : -1;

            // land with the selected segment at the top (angle 0), after a few full turns:
            // the remaining distance in spin direction to `target`, plus whole turns
            var from = _angles[ring];
            var target = -_rings[ring].SelectedIndex * step;
            var to = from + direction * (SpinTurns * Mathf.Tau + Mathf.PosMod(direction * (target - from), Mathf.Tau));

            await foreach (var t in Easings.Easings.Animate(duration, Ease.EaseOutCubic))
            {
                if (!IsInsideTree())
                    return;
                _angles[ring] = Mathf.Lerp(from, to, t);
                QueueRedraw();
            }

            await foreach (var t in Easings.Easings.Animate(LockPulseDuration, Ease.EaseOutQuad))
            {
                if (!IsInsideTree())
                    return;
                _lock[ring] = t;
                QueueRedraw();
            }
        }

        public override void _Draw()
        {
            if (_rings == null || _rings.Count == 0)
                return;

            var center = Size / 2;
            var outer = Mathf.Min(Size.X, Size.Y) / 2 - Margin;
            if (outer <= 0)
                return;

            // rings and centre disc share the radius equally
            var thickness = outer / (_rings.Count + 1);
            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(8, (int)(thickness * 0.45f));

            // filled discs from outside in, each one covering the inner part of the previous
            for (int i = 0; i < _rings.Count; i++)
            {
                var radius = outer - i * thickness;
                DrawCircle(center, radius, _rings[i].Kind == RingKind.Number ? NumberRingColor : OperatorRingColor);
                DrawArc(center, radius, 0, Mathf.Tau, 96, LineColor, 2, true);
            }
            var centerRadius = outer - _rings.Count * thickness;
            DrawCircle(center, centerRadius, CenterColor);
            DrawArc(center, centerRadius, 0, Mathf.Tau, 64, LineColor, 2, true);

            // selection window from the centre disc up through all rings
            var windowWidth = thickness * 1.1f;
            DrawRect(new Rect2(center.X - windowWidth / 2, center.Y - outer, windowWidth, outer - centerRadius), WindowColor);

            for (int i = 0; i < _rings.Count; i++)
            {
                var ring = _rings[i];
                var step = Mathf.Tau / ring.Segments.Count;
                var labelRadius = outer - (i + 0.5f) * thickness;
                var ringFontSize = ring.Kind == RingKind.Operator ? (int)(fontSize * OperatorFontScale) : fontSize;

                for (int k = 0; k < ring.Segments.Count; k++)
                {
                    var angle = _angles[i] + k * step;
                    var locked = k == ring.SelectedIndex && _lock[i] > 0;
                    var size = locked ? (int)(ringFontSize * (1.4f - 0.25f * _lock[i])) : ringFontSize;
                    DrawLabel(font, ring.Segments[k], center + Vector2.Up.Rotated(angle) * labelRadius, angle, size, locked ? LockedColor : TextColor);
                }
            }

            // centre: the result, or `?` when that's what the player has to find
            var result = Round.IsUnknown(ChallengeRound.AnswerParam) ? "?" : Round.Values[ChallengeRound.AnswerParam].ToString();
            DrawLabel(font, $"= {result}", center, 0, fontSize, TextColor);

            // marker above the window
            var tip = new Vector2(center.X, center.Y - outer + 2);
            DrawColoredPolygon([tip, tip + new Vector2(-10, -14), tip + new Vector2(10, -14)], LineColor);
        }

        // centred on `position`, rotated with the ring so the top segment reads upright
        void DrawLabel(Font font, string text, Vector2 position, float angle, int fontSize, Color color)
        {
            var textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
            DrawSetTransform(position, angle);
            DrawString(font, new Vector2(-textSize.X / 2, font.GetAscent(fontSize) - textSize.Y / 2), text,
                HorizontalAlignment.Left, -1, fontSize, color);
            DrawSetTransform(Vector2.Zero);
        }
    }
}
