// using System;
// using System.Collections.Generic;
// using System.Linq;
// using TnT.EduGame.Inventory;
// using UnityEngine;

// namespace TnT.Systems.UI
// {


//     [Serializable]
//     public class InventoryModel
//     {
//         Inventory _inventory;
//         public IEnumerable<Item> Items => _inventory.Items;
//         public IEnumerable<Item> Gear => _inventory.Gear;
//         public void Initialize()
//         {
//             _inventory = UnityEngine.Object.FindAnyObjectByType<Inventory>();
//         }

//         internal void MoveItemToGear(Item item, int index)
//         {
//             _inventory.MoveItemToGear(item, index);
//         }

//         internal void MoveItemToInventory(Item item, int index)
//         {
//             _inventory.MoveItemToInventory(item, index);
//         }
//     }
// }