using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Inventory
{
    /// <summary>
    /// The logic handler for the in-game inventory. This is not the visual representation of the inventory (see Inventory.MVC.InventoryController), but rather a container for all Items that the player has collected.
    /// </summary>
    public partial class Inventory : Node
    {
        [Export]
        Item[] _items = new Item[10];
        [Export]
        Item[] _gear = new Item[6];
        public IEnumerable<Item> Items => _items.AsEnumerable();
        public IEnumerable<Item> Gear => _gear.AsEnumerable();

        [Signal]
        public delegate void OnItemActionEventHandler(Item item, ItemEventType itemEventType);

        public enum ItemEventType
        {
            ITEM_ADDED,
            ITEM_USED,
            ITEM_REMOVED
        }

        public void AddItem(Item item)
        {
            var index = Array.IndexOf(_items, null);
            if (index < 0) return;
            _items[index] = item;

            item.OnItemUsed += UseItem;

            // OnItemEvent.Invoke(item, ItemEventType.ITEM_ADDED);
            EmitSignal(SignalName.OnItemAction, item, (int)ItemEventType.ITEM_ADDED);
        }

        public void UseItem(Item item)
        {
            if (_items.FirstOrDefault(i => i == item) == null)
                return;
            
            EmitSignal(SignalName.OnItemAction, item, (int)ItemEventType.ITEM_USED);

            if (item.IsPersistent)
                return;

            RemoveItem(item);
        }

        public void RemoveItem(Item item)
        {
            var index = Array.IndexOf(_items, item);
            if (index < 0) return;
            _items[index] = null;

            EmitSignal(SignalName.OnItemAction, item, (int)ItemEventType.ITEM_REMOVED);
        }

        internal void MoveItemToInventory(Item item, int index)
        {
            var oldInventoryIndex = Array.IndexOf(_items, item);
            var oldGearIndex = Array.IndexOf(_gear, item);

            if (oldInventoryIndex >= 0)
                _items[oldInventoryIndex] = null;
            if (oldGearIndex >= 0)
                _gear[oldGearIndex] = null;

            _items[index] = item;
        }

        internal void MoveItemToGear(Item item, int index)
        {
            var oldInventoryIndex = Array.IndexOf(_items, item);
            var oldGearIndex = Array.IndexOf(_gear, item);

            if (oldInventoryIndex >= 0)
                _items[oldInventoryIndex] = null;
            if (oldGearIndex >= 0)
                _gear[oldGearIndex] = null;

            _gear[index] = item;
        }
    }

}
