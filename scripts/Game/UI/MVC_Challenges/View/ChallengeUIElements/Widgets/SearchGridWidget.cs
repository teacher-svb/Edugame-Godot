using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    // Draws the grid with square cells, axis values along the bottom and left, an
    // optional background picture and a pulsing marker at the round's coordinates.
    public partial class SearchGridWidget : ChallengeVisualizationWidget
    {
        [Export] public Color PaperColor { get; set; } = new(0.97f, 0.94f, 0.86f);
        [Export] public Color LineColor { get; set; } = new(0.35f, 0.25f, 0.15f, 0.45f);
        [Export] public Color BorderColor { get; set; } = new(0.35f, 0.25f, 0.15f);
        [Export] public Color LabelColor { get; set; } = new(0.20f, 0.13f, 0.07f);
        [Export] public Color MarkerColor { get; set; } = new(0.80f, 0.20f, 0.15f);

        // seconds per marker pulse
        [Export] public float PulsePeriod { get; set; } = 1.2f;

        const float Margin = 8f;

        SearchGridVisualization _grid;
        Vector2I _marker;
        float _time;

        public override void _Ready()
        {
            _grid = (SearchGridVisualization)Challenge.Visualization;
            _marker = _grid.MarkerCoordinates(Round);
        }

        public override void _Process(double delta)
        {
            _time += (float)delta;
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (_grid == null)
                return;

            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(8, (int)(Mathf.Min(Size.X, Size.Y) * 0.035f));
            var labelWidth = Mathf.Max(
                font.GetStringSize(_grid.YStart.ToString(), HorizontalAlignment.Left, -1, fontSize).X,
                font.GetStringSize(_grid.YEnd.ToString(), HorizontalAlignment.Left, -1, fontSize).X);
            var labelHeight = font.GetHeight(fontSize);

            // room for Y labels on the left and X labels below, then square cells, centred
            var available = Size - new Vector2(labelWidth + Margin * 3, labelHeight + Margin * 3);
            var cell = Mathf.Min(available.X / _grid.Columns, available.Y / _grid.Rows);
            if (cell <= 0)
                return;
            var gridSize = new Vector2(_grid.Columns, _grid.Rows) * cell;
            var origin = new Vector2(
                labelWidth + Margin * 2 + (available.X - gridSize.X) / 2,
                Margin + (available.Y - gridSize.Y) / 2);
            var area = new Rect2(origin, gridSize);

            DrawRect(area, PaperColor);
            if (_grid.Background != null)
                DrawTextureRect(_grid.Background, area, false);

            for (int i = 0; i <= _grid.Columns; i++)
            {
                var x = origin.X + i * cell;
                DrawLine(new Vector2(x, area.Position.Y), new Vector2(x, area.End.Y), LineColor, 1);
                DrawCentered(font, fontSize, (_grid.XStart + i * _grid.XStep).ToString(), new Vector2(x, area.End.Y + Margin + labelHeight / 2));
            }
            for (int j = 0; j <= _grid.Rows; j++)
            {
                // Y counts upwards
                var y = area.End.Y - j * cell;
                DrawLine(new Vector2(area.Position.X, y), new Vector2(area.End.X, y), LineColor, 1);
                var text = (_grid.YStart + j * _grid.YStep).ToString();
                var width = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize).X;
                DrawCentered(font, fontSize, text, new Vector2(area.Position.X - Margin - width / 2, y));
            }
            DrawRect(area, BorderColor, false, 2);

            DrawMarker(font, fontSize, new Vector2(
                origin.X + (float)(_marker.X - _grid.XStart) / _grid.XStep * cell,
                area.End.Y - (float)(_marker.Y - _grid.YStart) / _grid.YStep * cell), cell);
        }

        void DrawMarker(Font font, int fontSize, Vector2 position, float cell)
        {
            var size = Mathf.Clamp(cell * 0.25f, 5, 14);
            var pulse = (Mathf.Sin(_time / PulsePeriod * Mathf.Tau) + 1) / 2;

            DrawCircle(position, size * (1.6f + 0.6f * pulse), MarkerColor with { A = 0.15f + 0.2f * pulse });
            DrawCircle(position, size, MarkerColor);
            DrawCircle(position, size * 0.4f, PaperColor);

            if (_grid.MarkerLabel != "")
                DrawString(font, position + new Vector2(size * 1.6f, -size * 0.8f), Round.Format(_grid.MarkerLabel),
                    HorizontalAlignment.Left, -1, (int)(fontSize * 1.3f), MarkerColor);
        }

        void DrawCentered(Font font, int fontSize, string text, Vector2 position)
        {
            var textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
            DrawString(font, new Vector2(position.X - textSize.X / 2, position.Y - textSize.Y / 2 + font.GetAscent(fontSize)),
                text, HorizontalAlignment.Left, -1, fontSize, LabelColor);
        }
    }
}
