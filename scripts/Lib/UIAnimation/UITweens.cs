using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UIAnimation
{
    public enum SlideDirection { Left, Right, Up, Down }

    public static class UITweens
    {
        // ── Bump ─────────────────────────────────────────────────────────────

        public static Task<bool> Bump(this Control control, float scale = 1.2f, float rotDeg = 15f)
        {
            control.PivotOffset = control.Size / 2f;
            float rotDir = GD.RandRange(0, 1) > 0.5f ? 1f : -1f;

            var tween = control.ReplaceTween("bump", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", scale, 0.1f);
            tween.TweenProperty(control, "scale:y", scale, 0.15f);
            tween.TweenProperty(control, "rotation_degrees", rotDeg * rotDir, 0.1f);
            tween.TweenProperty(control, "scale:x", 1f, 0.2f).SetDelay(0.2f);
            tween.TweenProperty(control, "scale:y", 1f, 0.3f).SetDelay(0.25f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.1f).SetDelay(0.15f);

            return done;
        }

        // ── Fade ─────────────────────────────────────────────────────────────

        public static Task<bool> FadeIn(this Control control, float duration = 0.3f)
        {
            var tween = control.ReplaceTween("fade", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(control, "modulate:a", 1f, duration);

            return done;
        }

        public static Task<bool> FadeOut(this Control control, float duration = 0.3f)
        {
            var tween = control.ReplaceTween("fade", out var done)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(control, "modulate:a", 0f, duration);

            return done;
        }

        // ── Scale ─────────────────────────────────────────────────────────────

        public static Task<bool> ScaleIn(this Control control, float duration = 0.3f)
        {
            control.PivotOffset = control.Size / 2f;
            control.Scale = Vector2.Zero;
            control.Modulate = new Color(control.Modulate, 0f);

            var tween = control.ReplaceTween("scale", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale", Vector2.One, duration);
            tween.TweenProperty(control, "modulate:a", 1f, duration * 0.3f);

            return done;
        }

        public static Task<bool> ScaleOut(this Control control, float duration = 0.25f)
        {
            control.PivotOffset = control.Size / 2f;

            var tween = control.ReplaceTween("scale", out var done)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale", Vector2.Zero, duration);
            tween.TweenProperty(control, "modulate:a", 0f, duration * 0.8f);

            return done;
        }

        // ── Slide ─────────────────────────────────────────────────────────────

        public static Task<bool> SlideIn(this Control control, SlideDirection direction = SlideDirection.Left, float duration = 0.25f)
        {
            Vector2 from = control.Position + SlideOffset(control, direction);
            control.Modulate = new Color(control.Modulate, 0f);

            var tween = control.ReplaceTween("slide", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "position", control.Position, duration).From(from);
            tween.TweenProperty(control, "modulate:a", 1f, duration * 0.4f);

            return done;
        }

        public static Task<bool> SlideOut(this Control control, SlideDirection direction = SlideDirection.Left, float duration = 0.2f)
        {
            Vector2 to = control.Position + SlideOffset(control, direction);

            var tween = control.ReplaceTween("slide", out var done)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Sine);
            tween.SetParallel(true);
            tween.TweenProperty(control, "position", to, duration);
            tween.TweenProperty(control, "modulate:a", 0f, duration * 0.8f);

            return done;
        }

        // ── Hover ─────────────────────────────────────────────────────────────

        public static Task<bool> HoverBounce(this Control control, float scaleRatio = 1f)
        {
            control.PivotOffset = control.Size / 2f;
            float scaleTarget = 1f + 0.2f * scaleRatio;
            float rotDir = GD.RandRange(0, 1) > 0.5f ? 1f : -1f;

            var tween = control.ReplaceTween("hover", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", scaleTarget, 0.2f);
            tween.TweenProperty(control, "scale:y", scaleTarget, 0.35f);
            tween.TweenProperty(control, "rotation_degrees", 5f * scaleRatio * rotDir, 0.1f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.1f).SetDelay(0.1f);

            return done;
        }

        public static Task<bool> UnhoverBounce(this Control control)
        {
            var tween = control.ReplaceTween("hover", out var done)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            tween.SetParallel(true);
            tween.TweenProperty(control, "scale:x", 1f, 0.2f);
            tween.TweenProperty(control, "scale:y", 1f, 0.2f);
            tween.TweenProperty(control, "rotation_degrees", 0f, 0.15f);

            return done;
        }

        // ── Internals ─────────────────────────────────────────────────────────

        // One running tween per (control, slot). Every helper returns a task that completes with true when its
        // tween finished, or with false when it was replaced by a new tween in the same slot or killed in another
        // way (its control freed, Kill() from outside). A killed tween never emits Finished, so an awaiter of
        // Tween.Finished alone would never resume. Callers that change the control after the await (Visible = false
        // after a fade-out) do so only on true: on false another animation owns the control now.
        private sealed class Running
        {
            public Tween Tween;
            public readonly TaskCompletionSource<bool> Done = new();
        }

        private static readonly Dictionary<(ulong Control, string Slot), Running> _running = new();
        private static SceneTree _watchedTree;

        private static Tween ReplaceTween(this Control control, string slot, out Task<bool> done)
        {
            var key = (control.GetInstanceId(), slot);
            if (_running.Remove(key, out var previous))
            {
                if (GodotObject.IsInstanceValid(previous.Tween) && previous.Tween.IsValid())
                    previous.Tween.Kill();
                // deferred: resumed inline, the old awaiter could start another tween in this slot before the
                // caller has built the new one
                Callable.From(() => previous.Done.TrySetResult(false)).CallDeferred();
            }

            var running = new Running { Tween = control.CreateTween() };
            _running[key] = running;
            // inline, like awaiting Tween.Finished did
            running.Tween.Finished += () =>
            {
                if (_running.TryGetValue(key, out var current) && current == running)
                    _running.Remove(key);
                running.Done.TrySetResult(true);
            };
            WatchForKilled();
            done = running.Done.Task;
            return running.Tween;
        }

        // checked once per frame (also while the tree is paused), only while a tween runs
        private static void WatchForKilled()
        {
            if (_watchedTree != null || Engine.GetMainLoop() is not SceneTree tree)
                return;
            _watchedTree = tree;
            tree.ProcessFrame += ResolveKilled;
        }

        private static void ResolveKilled()
        {
            List<Running> killed = null;
            foreach (var (key, running) in _running.ToArray())
            {
                if (GodotObject.IsInstanceValid(running.Tween) && running.Tween.IsValid())
                    continue;
                _running.Remove(key);
                (killed ??= new()).Add(running);
            }

            if (_running.Count == 0 && _watchedTree != null)
            {
                _watchedTree.ProcessFrame -= ResolveKilled;
                _watchedTree = null;
            }

            foreach (var running in killed ?? [])
                running.Done.TrySetResult(false);
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
