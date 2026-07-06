using Godot;

namespace TnT.Input
{
    [GlobalClass]
    public partial class InputAction2D : InputActionBase
    {
        [Export] public InputAction NegativeX { get; set; }
        [Export] public InputAction PositiveX { get; set; }
        [Export] public InputAction NegativeY { get; set; }
        [Export] public InputAction PositiveY { get; set; }

        /// <summary>The current movement vector, normalized to the unit circle on diagonals.</summary>
        public Vector2 Value { get; private set; }

        /// <inheritdoc/>
        public override void Poll()
        {
            NegativeX?.Poll();
            PositiveX?.Poll();
            NegativeY?.Poll();
            PositiveY?.Poll();

            var raw = new Vector2(
                (PositiveX?.IsPressed == true ? 1f : 0f) - (NegativeX?.IsPressed == true ? 1f : 0f),
                (PositiveY?.IsPressed == true ? 1f : 0f) - (NegativeY?.IsPressed == true ? 1f : 0f)
            );
            Value = raw.LengthSquared() > 1f ? raw.Normalized() : raw;

            bool moving = Value != Vector2.Zero;

            Triggered = moving && !IsPressed;
            WasReleasedThisFrame = !moving && IsPressed;
            IsPressed = moving;

            if (Triggered) InvokeOnPressed();
            if (IsPressed) InvokeOnHeld();
            if (WasReleasedThisFrame) InvokeOnReleased();
        }
    }
}
