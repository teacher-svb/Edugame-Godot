using System.Linq;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Draws the bar chart; bars grow into place when shown. A bar whose param is
    // unknown shows only a dashed slot with `?`, so its height isn't given away.
    public partial class BarGraphWidget : ChallengeVisualizationWidget<BarGraphVisualization>
    {
        [Export] public Color PaperColor { get; set; } = new(0.97f, 0.94f, 0.86f);
        [Export] public Color GridColor { get; set; } = new(0.35f, 0.25f, 0.15f, 0.25f);
        [Export] public Color LabelColor { get; set; } = new(0.20f, 0.13f, 0.07f);
        [Export] public Color FrameColor { get; set; } = new(0.35f, 0.25f, 0.15f);
        [Export] public Color BarColor { get; set; } = new(0.32f, 0.53f, 0.21f);
        [Export] public Color UnknownColor { get; set; } = new(0.80f, 0.20f, 0.15f);

        // seconds for the bars to grow in
        [Export] public float GrowDuration { get; set; } = 0.6f;

        const float BarWidthRatio = 0.6f;

        float _grow;

        public override void _Process(double delta)
        {
            if (_grow >= 1)
                return;
            _grow = Mathf.Min(1, _grow + (float)delta / Mathf.Max(GrowDuration, 0.01f));
            QueueRedraw();
        }

        public override void _Draw()
        {
            var bars = Settings.Bars.Where(b => b != null && b.Param != "").ToList();
            if (bars.Count == 0)
                return;

            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(10, (int)(Mathf.Min(Size.X, Size.Y) * 0.04f));
            var area = this.DrawValueAxis(font, fontSize, Size, 0, Settings.AxisMax, Settings.AxisStep, Settings.Unit,
                new GraphStyle(PaperColor, GridColor, LabelColor, FrameColor));
            if (!area.IsDrawable)
                return;

            var plot = area.Plot;
            var slot = plot.Size.X / bars.Count;
            var eased = 1 - Mathf.Pow(1 - _grow, 3);

            for (int i = 0; i < bars.Count; i++)
            {
                var bar = bars[i];
                var centerX = plot.Position.X + slot * (i + 0.5f);
                var width = slot * BarWidthRatio;

                if (Round.IsUnknown(bar.Param))
                {
                    // full height on purpose: a partial box would read as a value
                    var slotRect = new Rect2(centerX - width / 2, plot.Position.Y, width, plot.Size.Y);
                    DrawDashedRect(slotRect, UnknownColor);
                    this.DrawTextCentered(font, "?", slotRect.GetCenter(), (int)(fontSize * 1.6f), UnknownColor);
                }
                else
                {
                    var value = Round.Values[bar.Param];
                    var top = Mathf.Lerp(plot.End.Y, area.Y(value), eased);
                    DrawRect(new Rect2(centerX - width / 2, top, width, plot.End.Y - top), BarColor);
                    if (Settings.ShowValues && _grow >= 1)
                        this.DrawTextCentered(font, value.ToString(), new Vector2(centerX, top - area.LineHeight / 2 - 2), fontSize, LabelColor);
                }

                this.DrawTextCentered(font, Round.Format(bar.Label), new Vector2(centerX, area.LabelRowY), fontSize, LabelColor);
            }
        }

        void DrawDashedRect(Rect2 rect, Color color)
        {
            Vector2[] corners = [rect.Position, new(rect.End.X, rect.Position.Y), rect.End, new(rect.Position.X, rect.End.Y)];
            for (int i = 0; i < 4; i++)
                DrawDashedLine(corners[i], corners[(i + 1) % 4], color, 2, 6);
        }
    }
}
