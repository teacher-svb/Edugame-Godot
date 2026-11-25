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

            bool pressedNow = isPressed && !_wasPressed;
            bool releasedNow = !isPressed && _wasPressed;
            // state first: a throwing handler must not leave _wasPressed stale (the action would re-trigger every frame)
            _wasPressed = isPressed;
            Triggered = pressedNow;   // reset each poll (also when the handlers below throw)
            WasReleasedThisFrame = releasedNow;

            if (pressedNow)
                InvokeOnPressed();
            if (isPressed)
                InvokeOnHeld();
            if (releasedNow)
                InvokeOnReleased();
        }
    }
}
