using Godot;
using System;

namespace TnT.Input
{
    public abstract partial class InputActionBase : Resource
    {
        [Export] public bool Enabled { get; set; } = false;
        [Export] public string ActionName { get; set; }

        public bool Triggered { get; protected set; }
        public bool WasReleasedThisFrame { get; protected set; }
        public bool IsPressed { get; protected set; }

        /// <summary>Fired on the first frame the action is pressed. The action itself is passed as the argument.</summary>
        public event Action<InputActionBase> OnPressed;
        /// <summary>Fired on the first frame the action is released. The action itself is passed as the argument.</summary>
        public event Action<InputActionBase> OnReleased;
        /// <summary>Fired every frame the action is held. The action itself is passed as the argument.</summary>
        public event Action<InputActionBase> OnHeld;

        public void Enable() => Enabled = true;
        public void Disable() => Enabled = false;

        protected void InvokeOnPressed()  => OnPressed?.Invoke(this);
        protected void InvokeOnReleased() => OnReleased?.Invoke(this);
        protected void InvokeOnHeld()     => OnHeld?.Invoke(this);

        public abstract void Poll();
    }
}
