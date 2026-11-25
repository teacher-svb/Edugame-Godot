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
        // a list, not a set: listeners are raised in the order they registered (entered the tree), every time
        readonly List<EventListener> observers = new();

        // In registration order, over a copy: a reaction may add or remove listeners (e.g. free a node) while the
        // event is raised
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

        /// <summary>The listeners registered now (still valid), for a channel that asks them before it raises an event.</summary>
        protected IEnumerable<EventListener> Listeners => observers.Where(GodotObject.IsInstanceValid);

        public void Register(EventListener observer)
        {
            if (observer != null && !observers.Contains(observer))
                observers.Add(observer);
        }

        public void Deregister(EventListener observer) => observers.Remove(observer);
    }
}