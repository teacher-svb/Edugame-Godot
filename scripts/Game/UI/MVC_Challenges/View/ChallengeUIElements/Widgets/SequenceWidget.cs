using System.Collections.Generic;
using Godot;
using TnT.EduGame.Challenges;

namespace TnT.EduGame.UI
{
    // The number series in a row of boxes with arrows between them; the boxes pop in one after
    // another. The missing term is a dashed box with `?`, so the row shows where the gap is.
    public partial class SequenceWidget : ChallengeVisualizationWidget<SequenceVisualization>
    {
        [Export] public Color PaperColor { get; set; } = new(0.97f, 0.94f, 0.86f);
        [Export] public Color BoxColor { get; set; } = new(1f, 0.99f, 0.95f);
        [Export] public Color FrameColor { get; set; } = new(0.35f, 0.25f, 0.15f);
        [Export] public Color LabelColor { get; set; } = new(0.20f, 0.13f, 0.07f);
        [Export] public Color UnknownColor { get; set; } = new(0.80f, 0.20f, 0.15f);

        // seconds between two boxes popping in
        [Export] public float StepDuration { get; set; } = 0.15f;

        const float Margin = 0.06f;      // of the width, around the row
        const float GapRatio = 0.35f;    // gap between boxes, of a box's width

        IReadOnlyList<int> _terms = [];
        int _missing;
        float _time;

        public override void _Ready()
        {
            _terms = Settings.Terms(Round);
            _missing = Settings.MissingIndex(Round);
        }

        public override void _Process(double delta)
        {
            if (_time > StepDuration * (_terms.Count + 2))
                return;
            _time += (float)delta;
            QueueRedraw();
        }

        // 0..1 per box, staggered
        float Pop(int i) => Mathf.Clamp((_time - i * StepDuration) / Mathf.Max(StepDuration * 2, 0.01f), 0, 1);

        public override void _Draw()
        {
            int count = _terms.Count;
            if (count == 0 || Size.X < 10 || Size.Y < 10)
                return;

            DrawRect(new Rect2(Vector2.Zero, Size), PaperColor);

            var inner = Size.X * (1 - 2 * Margin);
            var boxWidth = Mathf.Min(inner / (count + (count - 1) * GapRatio), Size.Y * 0.5f);
            var gap = boxWidth * GapRatio;
            var rowWidth = count * boxWidth + (count - 1) * gap;
            var left = (Size.X - rowWidth) / 2;
            var centerY = Size.Y / 2;
            var boxHeight = boxWidth * 0.8f;

            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(10, (int)(boxHeight * 0.38f));
            // the widest label must fit its box
            foreach (var term in _terms)
            {
                var width = font.TextSize(Settings.Label(term), fontSize).X;
                if (width > boxWidth * 0.85f)
                    fontSize = Mathf.Max(8, (int)(fontSize * boxWidth * 0.85f / width));
            }

            for (int i = 0; i < count; i++)
            {
                var center = new Vector2(left + boxWidth / 2 + i * (boxWidth + gap), centerY);
                var t = Pop(i);
                var scale = 1 - Mathf.Pow(1 - t, 3);
                var size = new Vector2(boxWidth, boxHeight) * scale;
                var rect = new Rect2(center - size / 2, size);

                if (i > 0 && t > 0)
                    DrawArrow(new Vector2(center.X - boxWidth / 2 - gap * 0.8f, centerY), new Vector2(center.X - boxWidth / 2 - gap * 0.2f, centerY));
                if (t <= 0)
                    continue;

                if (i == _missing)
                {
                    DrawDashedRect(rect, UnknownColor);
                    if (t >= 1)
                        this.DrawTextCentered(font, "?", center, (int)(fontSize * 1.4f), UnknownColor);
                }
                else
                {
                    DrawRect(rect, BoxColor);
                    DrawRect(rect, FrameColor, false, 2);
                    if (t >= 1)
                        this.DrawTextCentered(font, Settings.Label(_terms[i]), center, fontSize, LabelColor);
                }
            }
        }

        void DrawArrow(Vector2 from, Vector2 to)
        {
            DrawLine(from, to, FrameColor, 2);
            var head = (to - from).Normalized() * Mathf.Min(8, from.DistanceTo(to) * 0.5f);
            DrawLine(to, to - head.Rotated(0.5f), FrameColor, 2);
            DrawLine(to, to - head.Rotated(-0.5f), FrameColor, 2);
        }

        void DrawDashedRect(Rect2 rect, Color color)
        {
            Vector2[] corners = [rect.Position, new(rect.End.X, rect.Position.Y), rect.End, new(rect.Position.X, rect.End.Y)];
            for (int i = 0; i < 4; i++)
                DrawDashedLine(corners[i], corners[(i + 1) % 4], color, 2, 6);
        }
    }
}
