using Godot;
using System;

namespace TnT.Input
{
    [GlobalClass]
    public partial class InputAction : InputActionBase
    {
        [Export] public string InputReference { get; set; }

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

            if (string.IsNullOrEmpty(InputReference) || !InputMap.HasAction(InputReference))
                throw new InvalidOperationException($"InputAction: no InputMap action named \"{InputReference}\". Add it in Project > Project Settings > Input Map.");

            bool isPressed = Godot.Input.IsActionPressed(InputReference);
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
