using Godot;

namespace TnT.Systems.UIAnimation
{
    /// <summary>
    /// Drop as child of any Control. Animates the parent on ready or visibility change.
    /// Uses modulate.a instead of visible to avoid container layout recalculation.
    /// </summary>
    public partial class AutoTween : Node
    {
        public enum AnimType { Fade, Scale }
        public enum TriggerMode { Ready, Visible }

        [Export] public AnimType Animation = AnimType.Fade;
        [Export] public TriggerMode Trigger = TriggerMode.Ready;
        [Export] public float Duration = 0.3f;

        private Control _target;

        public override void _Ready()
        {
            _target = GetParent<Control>();

            switch (Animation)
            {
                case AnimType.Fade:  _target.Modulate = new Color(_target.Modulate, 0f); break;
                case AnimType.Scale: _target.Scale = Vector2.Zero; break;
            }

            if (Trigger == TriggerMode.Ready)
                Show();
            else
                _target.VisibilityChanged += OnVisibilityChanged;
        }

        private void OnVisibilityChanged()
        {
            if (_target.Visible) Show();
            else Hide();
        }

        public void Show()
        {
            switch (Animation)
            {
                case AnimType.Fade:  _target.FadeIn(Duration); break;
                case AnimType.Scale: _target.ScaleIn(Duration); break;
            }
        }

        public void Hide()
        {
            switch (Animation)
            {
                case AnimType.Fade:  _target.FadeOut(Duration); break;
                case AnimType.Scale: _target.ScaleOut(Duration); break;
            }
        }
    }
}
