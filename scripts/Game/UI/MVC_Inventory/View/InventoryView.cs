// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Sirenix.Utilities;
// using TnT.EduGame.Inventory;
// using TnT.Extensions;
// using UnityEngine;
// using UnityEngine.UIElements;

// namespace TnT.Systems.UI
// {

//     [Serializable]
//     public class InventoryView
//     {
//         VisualElement root;
//         VisualElement container;
//         [SerializeField]
//         UIDocument document;
//         [SerializeField]
//         StyleSheet styleSheet;

//         public VisualElement Root => document.rootVisualElement;


//         List<InventorySlot> _inventorySlots = new();
//         List<InventorySlot> _gearSlots = new();
//         public List<InventorySlot> AllSlots => _inventorySlots.Concat(_gearSlots).ToList();
//         // public List<InventorySlot> GearSlots => _gearSlots;
//         // public List<InventorySlot> InventorySlots => _inventorySlots;
//         InventorySlot _draggedSlot;


//         public async Task InitializeView(IEnumerable<Item> inventoryItems, IEnumerable<Item> gearItems)
//         {
//             root = document.rootVisualElement;
//             root.Clear();

//             root.styleSheets.Add(styleSheet);

//             container = root.CreateChild("container");

//             var inventory = container.CreateChild<InventoryElement>("inventory");
//             _inventorySlots.AddRange(inventory.CreateSlots(inventoryItems.Count()));

//             var gearContainer = container.CreateChild("gearContainer");

//             var gearSlotsToAdd = gearItems.Count();
//             while (gearSlotsToAdd > 0)
//             {
//                 var gear = gearContainer.CreateChild<InventoryElement>("gear");
//                 var numSlots = Math.Min(2, gearSlotsToAdd);
//                 _gearSlots.AddRange(gear.CreateSlots(numSlots));
//                 gearSlotsToAdd -= numSlots;
//             }
//             // var gear = gearContainer.CreateChild<InventoryElement>("gear");
//             // _gearSlots.AddRange(gear.CreateSlots(2));
//             // gear = gearContainer.CreateChild<InventoryElement>("gear");
//             // _gearSlots.AddRange(gear.CreateSlots(2));
//             // gear = gearContainer.CreateChild<InventoryElement>("gear");
//             // _gearSlots.AddRange(gear.CreateSlots(2));

//             _draggedSlot = root.CreateChild<InventorySlot>("draggedSlot");
//             _draggedSlot.pickingMode = PickingMode.Ignore;

//             Refresh(inventoryItems, gearItems);

//             await Task.Yield();
//         }

//         public void Show() => container.AddToClassList("opened");
//         public void Hide() => container.RemoveFromClassList("opened");
//         public int GetInventoryIndex(InventorySlot slot) => _inventorySlots.FindIndex(s => s == slot);
//         public int GetGearIndex(InventorySlot slot) => _gearSlots.FindIndex(s => s == slot);

//         internal void ShowDraggable(Vector3 pos, Item item)
//         {
//             if (_draggedSlot.ClassListContains("dragging") == false)
//             {
//                 _draggedSlot.AddToClassList("dragging");
//                 _draggedSlot.SetItem(item);
//             }
//             _draggedSlot.style.left = new(pos.x);
//             _draggedSlot.style.top = new(pos.y);
//         }

//         internal void HideDraggable() => _draggedSlot.RemoveFromClassList("dragging");

//         internal void Refresh(IEnumerable<Item> inventoryItems, IEnumerable<Item> gearItems)
//         {
//             for (var i = 0; i < inventoryItems.Count(); ++i)
//             {
//                 var slot = _inventorySlots.ElementAt(i);
//                 var item = inventoryItems.ElementAt(i);
//                 slot.SetItem(item);
//             }

//             for (var i = 0; i < gearItems.Count(); ++i)
//             {
//                 var slot = _gearSlots.ElementAt(i);
//                 var item = gearItems.ElementAt(i);
//                 slot.SetItem(item);
//             }
//         }
//     }

//     public partial class InventoryElement : VisualElement
//     {
//         public IEnumerable<InventorySlot> CreateSlots(int count)
//         {
//             return Enumerable
//                 .Range(0, count)
//                 .Select(i => this.CreateChild<InventorySlot>("inventorySlot"));
//         }
//     }

//     public partial class InventorySlot : VisualElement
//     {
//         Image _icon;
//         Item _item;

//         public Item Item => _item;

//         public InventorySlot()
//         {
//             _icon = this.CreateChild<Image>("icon");
//             _icon.pickingMode = PickingMode.Ignore;
//         }

//         public void SetItem(Item item)
//         {
//             if (item == null || item.Data == null)
//             {
//                 _item = null;
//                 _icon.sprite = null;
//                 return;
//             }

//             _item = item;
//             _icon.sprite = _item?.Icon;
//         }

//         public Item RemoveItem()
//         {
//             _icon.sprite = null;
//             var item = _item;
//             _item = null;
//             return item;
//         }
//     }
// }
