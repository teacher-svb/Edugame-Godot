using System.Linq;
using TnT.Systems.EventSystem;
using Godot;

namespace TnT.EduGame.Inventory
{
    [GlobalClass]
    public partial class ItemEventChannel : EventChannel
    {
        /// <summary>
        /// False when a listener for <paramref name="item"/> refuses it (<see cref="ItemEventListener.Accepts"/>, e.g.
        /// the inventory when it is full). Ask before <see cref="EventChannel.Invoke"/>: the event itself can't be refused.
        /// </summary>
        public bool CanDeliver(ItemData item) =>
            Listeners.OfType<ItemEventListener>().All(listener => !listener.Refuses(item));
    }
}
