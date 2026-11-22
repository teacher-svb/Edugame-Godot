using Godot;

namespace TnT.Systems.EventSystem
{
    /// <summary>
    /// Represents a single type listener for events of type T. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [GlobalClass]
    public abstract partial class EventListener : Node
    {
        [Signal]
        public delegate void OnListenEventHandler(Variant value);
        [Export] EventChannel eventChannel;

        // Registered only while in the tree: a listener of a freed level must not stay on the channel
        // (the channel is a Resource and outlives the scene), or the next event reaches a disposed node.
        public override void _EnterTree() => eventChannel?.Register(this);
        public override void _ExitTree() => eventChannel?.Deregister(this);

        public virtual void Raise(params Variant[] values) => EmitSignal(SignalName.OnListen, values);

    }
}