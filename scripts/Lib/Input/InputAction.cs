using Godot;
using System;

namespace TnT.Input
{
    [GlobalClass]
    public partial class InputAction : Resource
    {
        [Export] public string ActionName { get; set; }
        [Export] public bool Enabled { get; set; } = false;

        // Properties for Unity-style usage
        public bool Triggered { get; private set; }            // True only this frame when pressed
        public bool WasReleasedThisFrame { get; private set; } // True only this frame when released
        public bool IsPressed { get; private set; }            // True while held down

        // Signals (for Godot editor)
        [Signal] public delegate void ActionPressedEventHandler();
        [Signal] public delegate void ActionReleasedEventHandler();
        [Signal] public delegate void ActionHeldEventHandler();

        // C# events
        public event Action OnPressed;
        public event Action OnReleased;
        public event Action OnHeld;

        private bool _wasPressed;

        public void Enable() => Enabled = true;
        public void Disable() => Enabled = false;

        public void Poll()
        {
            // Reset per-frame properties
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
                EmitSignal(SignalName.ActionPressed);
                OnPressed?.Invoke();
            }

            // Held
            if (isPressed)
            {
                EmitSignal(SignalName.ActionHeld);
                OnHeld?.Invoke();
            }

            // Released (first frame)
            if (!isPressed && _wasPressed)
            {
                WasReleasedThisFrame = true;
                EmitSignal(SignalName.ActionReleased);
                OnReleased?.Invoke();
            }

            _wasPressed = isPressed;
        }
    }
}
