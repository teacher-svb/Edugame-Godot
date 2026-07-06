using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UIAnimation
{
    public enum SlideDirection { Left, Right, Up, Down }

    public static class UITweens
    {
        private const string TweenMetaKey = "__ui_tween__";

        // ── Bump ─────────────────────────────────────────────────────────────

        public static async Task Bump(this Control control, float scale = 1.2f, float rotDeg = 15f)
        {
            control.PivotOffset = control.Size / 2f;
            float rotDir = GD.RandRange(0, 1) > 0.5f ? 1f : -1f;

            var tween = control.ReplaceTween("bump")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", scale, 0.1f);
            tween.TweenProperty(control, "scale:y", scale, 0.15f);
            tween.TweenProperty(control, "rotation_degrees", rotDeg * rotDir, 0.1f);
            tween.TweenProperty(control, "scale:x", 1f, 0.2f).SetDelay(0.2f);
            tween.TweenProperty(control, "scale:y", 1f, 0.3f).SetDelay(0.25f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.1f).SetDelay(0.15f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        // ── Fade ─────────────────────────────────────────────────────────────

        public static async Task FadeIn(this Control control, float duration = 0.3f)
        {
            var tween = control.ReplaceTween("fade")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(control, "modulate:a", 1f, duration);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        public static async Task FadeOut(this Control control, float duration = 0.3f)
        {
            var tween = control.ReplaceTween("fade")
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(control, "modulate:a", 0f, duration);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        // ── Scale ─────────────────────────────────────────────────────────────

        public static async Task ScaleIn(this Control control, float duration = 0.3f)
        {
            control.PivotOffset = control.Size / 2f;
            control.Scale = Vector2.Zero;
            control.Modulate = new Color(control.Modulate, 0f);

            var tween = control.ReplaceTween("scale")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale", Vector2.One, duration);
            tween.TweenProperty(control, "modulate:a", 1f, duration * 0.3f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        public static async Task ScaleOut(this Control control, float duration = 0.25f)
        {
            control.PivotOffset = control.Size / 2f;

            var tween = control.ReplaceTween("scale")
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale", Vector2.Zero, duration);
            tween.TweenProperty(control, "modulate:a", 0f, duration * 0.8f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        // ── Slide ─────────────────────────────────────────────────────────────

        public static async Task SlideIn(this Control control, SlideDirection direction = SlideDirection.Left, float duration = 0.25f)
        {
            Vector2 from = control.Position + SlideOffset(control, direction);
            control.Modulate = new Color(control.Modulate, 0f);

            var tween = control.ReplaceTween("slide")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "position", control.Position, duration).From(from);
            tween.TweenProperty(control, "modulate:a", 1f, duration * 0.4f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        public static async Task SlideOut(this Control control, SlideDirection direction = SlideDirection.Left, float duration = 0.2f)
        {
            Vector2 to = control.Position + SlideOffset(control, direction);

            var tween = control.ReplaceTween("slide")
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Sine);
            tween.SetParallel(true);
            tween.TweenProperty(control, "position", to, duration);
            tween.TweenProperty(control, "modulate:a", 0f, duration * 0.8f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        // ── Hover ─────────────────────────────────────────────────────────────

        public static async Task HoverBounce(this Control control, float scaleRatio = 1f)
        {
            control.PivotOffset = control.Size / 2f;
            float scaleTarget = 1f + 0.2f * scaleRatio;
            float rotDir = GD.RandRange(0, 1) > 0.5f ? 1f : -1f;

            var tween = control.ReplaceTween("hover")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", scaleTarget, 0.2f);
            tween.TweenProperty(control, "scale:y", scaleTarget, 0.35f);
            tween.TweenProperty(control, "rotation_degrees", 5f * scaleRatio * rotDir, 0.1f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.1f).SetDelay(0.1f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        public static async Task UnhoverBounce(this Control control)
        {
            var tween = control.ReplaceTween("hover")
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", 1f, 0.2f);
            tween.TweenProperty(control, "scale:y", 1f, 0.2f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.15f);

            await control.ToSignal(tween, Tween.SignalName.Finished);
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private static Tween ReplaceTween(this Control control, string slot)
        {
            string key = TweenMetaKey + slot;
            if (control.HasMeta(key) && control.GetMeta(key).As<GodotObject>() is Tween existing && existing.IsValid())
                existing.Kill();

            var tween = control.CreateTween();
            control.SetMeta(key, tween);
            return tween;
        }

        private static Vector2 SlideOffset(Control control, SlideDirection direction) => direction switch
        {
            SlideDirection.Left  => new Vector2(-control.Size.X, 0f),
            SlideDirection.Right => new Vector2(control.Size.X, 0f),
            SlideDirection.Up    => new Vector2(0f, -control.Size.Y),
            SlideDirection.Down  => new Vector2(0f, control.Size.Y),
            _                    => Vector2.Zero
        };
    }
}
