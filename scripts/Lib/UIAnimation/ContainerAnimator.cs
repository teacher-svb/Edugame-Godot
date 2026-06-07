using System.Collections.Generic;
using Godot;

namespace TnT.Systems.UIAnimation
{
    public partial class ContainerAnimator : Control
    {
        public enum AnimType { Scale, SlideInLeft, SlideInRight }
        public enum Order { TopDown, BottomUp }

        [Export] public AnimType Animation = AnimType.Scale;
        [Export] public Order AnimOrder = Order.TopDown;
        [Export] public float Duration = 0.25f;
        [Export] public float DelayBetween = 0.075f;
        [Export] public float DelayAppear = 0.1f;

        private Tween _tween;

        public override void _Ready() => Appear();

        public void Appear()
        {
            var controls = CollectChildren();

            foreach (var c in controls)
                c.Modulate = new Color(c.Modulate, 0f);

            _tween?.Kill();
            _tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            _tween.SetParallel(true);

            if (DelayAppear > 0f)
                _tween.TweenInterval(DelayAppear);

            if (AnimOrder == Order.BottomUp)
                controls.Reverse();

            for (int i = 0; i < controls.Count; i++)
            {
                var c = controls[i];
                float delay = DelayAppear + DelayBetween * i;

                switch (Animation)
                {
                    case AnimType.Scale:
                        c.PivotOffset = c.Size / 2f;
                        c.Scale = Vector2.Zero;
                        _tween.TweenProperty(c, "scale", Vector2.One, Duration).SetDelay(delay);
                        _tween.TweenProperty(c, "modulate:a", 1f, 0.01f).SetDelay(delay);
                        break;

                    case AnimType.SlideInLeft:
                        _tween.TweenProperty(c, "position:x", c.Position.X, Duration)
                            .From(c.Position.X - c.Size.X).SetDelay(delay);
                        _tween.TweenProperty(c, "modulate:a", 1f, 0.05f).SetDelay(delay);
                        break;

                    case AnimType.SlideInRight:
                        _tween.TweenProperty(c, "position:x", c.Position.X, Duration)
                            .From(c.Position.X + c.Size.X).SetDelay(delay);
                        _tween.TweenProperty(c, "modulate:a", 1f, 0.05f).SetDelay(delay);
                        break;
                }
            }
        }

        private List<Control> CollectChildren()
        {
            var result = new List<Control>();
            foreach (Node n in GetChildren())
                if (n is Control c) result.Add(c);
            return result;
        }
    }
}
