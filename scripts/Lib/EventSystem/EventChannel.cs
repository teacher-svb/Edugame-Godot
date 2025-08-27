using System.Collections.Generic;
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

        public void Invoke(params Variant[] values)
        {
            foreach (var observer in observers)
            {
                observer.Raise(values);
            }
        }

        public void Register(EventListener observer) => observers.Add(observer);
        public void Deregister(EventListener observer) => observers.Remove(observer);
    }
}