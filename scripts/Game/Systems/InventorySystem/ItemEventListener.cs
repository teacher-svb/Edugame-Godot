using System;
using Godot;
using TnT.Systems.EventSystem;

namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// Receives an item event (pickup, use, remove) for one item (<see cref="_checkItemId"/>) or any item. Code
    /// subscribes to <see cref="OnItemEvent"/> (the inventory), scenes connect the OnListen signal (a quest objective).
    /// </summary>
    [GlobalClass]
    public partial class ItemEventListener : EventListener
    {
        [Export] bool _checkItemId;
        [Export] string _itemId;
        public Action<ItemData> OnItemEvent;
        /// <summary>
        /// Optional gate: false refuses the item, so the sender keeps it (the inventory when it is full). Asked
        /// through <see cref="ItemEventChannel.CanDeliver"/> before the event is raised.
        /// </summary>
        public Func<ItemData, bool> Accepts;

        bool Matches(ItemData item) => item != null && (!_checkItemId || item.Id == _itemId);

        /// <summary>True when this listener is for <paramref name="item"/> and its <see cref="Accepts"/> gate says no.</summary>
        public bool Refuses(ItemData item) => Matches(item) && Accepts != null && !Accepts(item);

        public override void Raise(params Variant[] values)
        {
            if (values == null || values.Length == 0 || values[0].Obj is not ItemData item || !Matches(item))
                return;
            OnItemEvent?.Invoke(item);
            base.Raise(item);
        }
    }
}
