using Godot;
using System;
using TnT;

namespace TnT.Input
{
    [Tool]
    public partial class InputActionReference : Resource
    {
        [Export] public string ActionName { get; set; }

        // Signals (for Godot editor)
        [Signal] public delegate void ActionPressedEventHandler();
        [Signal] public delegate void ActionReleasedEventHandler();
        [Signal] public delegate void ActionHeldEventHandler();

        // C# events
        public event Action OnPressed;
        public event Action OnReleased;
        public event Action OnHeld;

        private bool _wasPressed;

        public void Poll()
        {
            bool isPressed = Godot.Input.IsActionPressed(ActionName);

            // Pressed event (first frame of press)
            if (isPressed && !_wasPressed)
            {
                EmitSignal(nameof(ActionPressedEventHandler));
                OnPressed?.Invoke();
            }

            // Held event (every frame while pressed)
            if (isPressed)
            {
                EmitSignal(nameof(ActionHeldEventHandler));
                OnHeld?.Invoke();
            }

            // Released event (first frame of release)
            if (!isPressed && _wasPressed)
            {
                EmitSignal(nameof(ActionReleasedEventHandler));
                OnReleased?.Invoke();
            }

            _wasPressed = isPressed;
        }
    }
}
