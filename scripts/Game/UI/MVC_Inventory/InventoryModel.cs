using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Inventory;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class InventoryModel : Node
    {
        [Export]
        ItemData[] _items = new ItemData[10];

        public event Action OnInventoryChanged;
        public event Action<ItemData, ItemEventType> OnItemAction;

        public IEnumerable<ItemData> Items => _items.AsEnumerable();

        public enum ItemEventType { ITEM_ADDED, ITEM_USED, ITEM_REMOVED }

        public void AddItem(ItemData item)
        {
            GD.Print($"picked up item {item.Id}");
            var index = Array.IndexOf(_items, null);
            if (index < 0) return;
            _items[index] = item;
            Notify(item, ItemEventType.ITEM_ADDED);
        }

        public void RemoveItem(ItemData item)
        {
            var index = Array.IndexOf(_items, item);
            if (index < 0) return;
            _items[index] = null;
            Notify(item, ItemEventType.ITEM_REMOVED);
        }

        private void Notify(ItemData item, ItemEventType type)
        {
            OnInventoryChanged?.Invoke();
            OnItemAction?.Invoke(item, type);
        }
    }
}
