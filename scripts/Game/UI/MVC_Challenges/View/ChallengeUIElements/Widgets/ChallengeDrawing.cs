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
    }
}
