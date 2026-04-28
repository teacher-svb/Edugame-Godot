using Godot;

namespace TnT.Input
{
    [GlobalClass]
    public partial class MultiInputAction : InputActionBase
    {
        [Export] public InputAction[] Actions { get; set; }

        /// <summary>The sub-action that most recently triggered. Null if none triggered this frame.</summary>
        public InputAction LastTriggered { get; private set; }

        private bool _wasPressed;

        /// <inheritdoc/>
        public override void Poll()
        {
            Triggered = false;
            WasReleasedThisFrame = false;
            LastTriggered = null;
            bool anyPressed = false;

            if (!Enabled || Actions == null)
            {
                IsPressed = false;
                _wasPressed = false;
                return;
            }

            foreach (var action in Actions)
            {
                action.Poll();

                if (action.Triggered)
                {
                    Triggered = true;
                    LastTriggered = action;
                }

                if (action.IsPressed)
                    anyPressed = true;
            }

            if (Triggered)        InvokeOnPressed();
            if (anyPressed)       InvokeOnHeld();
            if (!anyPressed && _wasPressed) { WasReleasedThisFrame = true; InvokeOnReleased(); }

            IsPressed = anyPressed;
            _wasPressed = anyPressed;
        }
    }
}
