using Godot;

namespace TnT.Input
{
    [GlobalClass]
    public partial class InputAction : InputActionBase
    {
        [Export] public string ActionName { get; set; }

        private bool _wasPressed;

        /// <inheritdoc/>
        public override void Poll()
        {
            Triggered = false;
            WasReleasedThisFrame = false;

            if (!Enabled)
            {
                IsPressed = false;
                _wasPressed = false;
                return;
            }

            bool isPressed = Godot.Input.IsActionPressed(ActionName);
            IsPressed = isPressed;

            // Pressed (first frame)
            if (isPressed && !_wasPressed)
            {
                Triggered = true;
                InvokeOnPressed();
            }

            // Held
            if (isPressed)
                InvokeOnHeld();

            // Released
            if (!isPressed && _wasPressed)
            {
                WasReleasedThisFrame = true;
                InvokeOnReleased();
            }

            _wasPressed = isPressed;
        }
    }
}
