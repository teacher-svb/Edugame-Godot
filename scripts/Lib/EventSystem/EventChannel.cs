using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.Systems.EventSystem {
    /// <summary>
    /// Represents a single type channel for events of type T. This is used to subscribe and unsubscribe listeners to receive notifications when an event occurs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [GlobalClass]
    public abstract partial class EventChannel : Resource
    {
        readonly HashSet<EventListener> observers = new();

        // Over a copy: a reaction may add or remove listeners (e.g. free a node) while the event is raised
        public void Invoke(params Variant[] values)
        {
            foreach (var observer in observers.ToArray())
            {
                if (GodotObject.IsInstanceValid(observer))
                    observer.Raise(values);
                else
                    observers.Remove(observer);
            }
        }

        public void Register(EventListener observer) => observers.Add(observer);
        public void Deregister(EventListener observer) => observers.Remove(observer);
    }
}