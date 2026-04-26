using Godot;
using System;

namespace TnT.Input
{
    public abstract partial class InputActionBase : Resource
    {
        [Export] public bool Enabled { get; set; } = false;

        public bool Triggered { get; protected set; }
        public bool WasReleasedThisFrame { get; protected set; }
        public bool IsPressed { get; protected set; }

        [Signal] public delegate void ActionPressedEventHandler();
        [Signal] public delegate void ActionReleasedEventHandler();
        [Signal] public delegate void ActionHeldEventHandler();

        public event Action OnPressed;
        public event Action OnReleased;
        public event Action OnHeld;

        public void Enable() => Enabled = true;
        public void Disable() => Enabled = false;

        protected void InvokeOnPressed() { EmitSignal(SignalName.ActionPressed); OnPressed?.Invoke(); }
        protected void InvokeOnReleased() { EmitSignal(SignalName.ActionReleased); OnReleased?.Invoke(); }
        protected void InvokeOnHeld() { EmitSignal(SignalName.ActionHeld); OnHeld?.Invoke(); }

        public abstract void Poll();
    }
}
