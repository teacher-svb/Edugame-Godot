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

        public override void _Ready() => eventChannel.Register(this);

        // protected void OnDestroy() => eventChannel.Deregister(this);

        public virtual void Raise(params Variant[] values) => EmitSignal(SignalName.OnListen, values);

    }
}