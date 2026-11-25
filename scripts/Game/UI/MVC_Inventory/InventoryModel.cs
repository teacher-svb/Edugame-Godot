using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Inventory;

namespace TnT.EduGame.UI
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

        /// <summary>True while a slot is free (the pickup gate: a world item is only picked up when it fits).</summary>
        public bool HasFreeSlot => Array.IndexOf(_items, null) >= 0;

        /// <summary>Puts <paramref name="item"/> in the first free slot; false when the inventory is full.</summary>
        public bool AddItem(ItemData item)
        {
            if (item == null)
                return false;
            var index = Array.IndexOf(_items, null);
            if (index < 0)
            {
                GD.PushWarning($"Inventory: full, item '{item.Id}' not added");
                return false;
            }
            _items[index] = item;
            Notify(item, ItemEventType.ITEM_ADDED);
            return true;
        }

        /// <summary>
        /// Takes <paramref name="item"/> out of the inventory (e.g. a quest step that hands it over) and raises
        /// ITEM_REMOVED (the controller sends it on the remove channel). False when it isn't in the inventory.
        /// </summary>
        public bool RemoveItem(ItemData item)
        {
            var index = item == null ? -1 : Array.IndexOf(_items, item);
            if (index < 0) return false;
            _items[index] = null;
            Notify(item, ItemEventType.ITEM_REMOVED);
            return true;
        }

        /// <summary>
        /// Uses <paramref name="item"/>: raises ITEM_USED (sent on the use channel); the item stays in the inventory
        /// (call <see cref="RemoveItem"/> for one that is used up). False when it isn't in the inventory.
        /// </summary>
        public bool UseItem(ItemData item)
        {
            if (item == null || Array.IndexOf(_items, item) < 0)
                return false;
            OnItemAction?.Invoke(item, ItemEventType.ITEM_USED);
            return true;
        }

        /// <summary>The filled slots, for the save file (see <see cref="ItemSaveData"/>).</summary>
        public List<ItemSaveData> GetSaveData()
        {
            var data = new List<ItemSaveData>();
            for (int slot = 0; slot < _items.Length; slot++)
            {
                var item = _items[slot];
                if (item == null)
                    continue;
                if (string.IsNullOrEmpty(item.ResourcePath) || item.ResourcePath.Contains("::"))
                {
                    GD.PushWarning($"Inventory: item '{item.Id}' is not a saved resource file and isn't saved");
                    continue;
                }
                data.Add(new ItemSaveData { Slot = slot, ItemId = item.Id, Resource = ResourceUid.PathToUid(item.ResourcePath) });
            }
            return data;
        }

        /// <summary>
        /// Replaces the whole inventory with <paramref name="data"/> (null or empty: an empty inventory). Call it on
        /// a new game and a load, so no item of the running session carries over. Raises no item events.
        /// </summary>
        public void Restore(IEnumerable<ItemSaveData> data)
        {
            Array.Fill(_items, null);
            foreach (var saved in data ?? [])
            {
                if (saved == null || string.IsNullOrEmpty(saved.Resource))
                    continue;
                var item = ResourceLoader.Exists(saved.Resource) ? ResourceLoader.Load<ItemData>(saved.Resource) : null;
                if (item == null)
                {
                    GD.PushWarning($"Inventory: saved item '{saved.ItemId}' ({saved.Resource}) not found, dropped");
                    continue;
                }
                // the saved slot when it is free, else the first free one
                int slot = saved.Slot >= 0 && saved.Slot < _items.Length && _items[saved.Slot] == null
                    ? saved.Slot
                    : Array.IndexOf(_items, null);
                if (slot < 0)
                {
                    GD.PushWarning($"Inventory: no free slot for saved item '{saved.ItemId}', dropped");
                    continue;
                }
                _items[slot] = item;
            }
            OnInventoryChanged?.Invoke();
        }

        private void Notify(ItemData item, ItemEventType type)
        {
            OnInventoryChanged?.Invoke();
            OnItemAction?.Invoke(item, type);
        }
    }
}
