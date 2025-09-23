using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Inventory;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class InventoryModel : Node
    {
        Inventory _inventory;
        public IEnumerable<Item> Items => _inventory.Items;
        public IEnumerable<Item> Gear => _inventory.Gear;
        public void Initialize()
        {
            _inventory = GetTree().FindAnyObjectByType<Inventory>();
        }

        internal void MoveItemToGear(Item item, int index)
        {
            _inventory.MoveItemToGear(item, index);
        }

        internal void MoveItemToInventory(Item item, int index)
        {
            _inventory.MoveItemToInventory(item, index);
        }
    }
}