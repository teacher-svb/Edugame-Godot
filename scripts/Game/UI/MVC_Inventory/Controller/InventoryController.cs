// using System;
// using System.Linq;
// using UnityEngine;
// using System.Threading.Tasks;
// using UnityEngine.UIElements;
// using TnT.Extensions;
// using TnT.EduGame.Inventory;
// using static TnT.EduGame.Inventory.Inventory;

// namespace TnT.Systems.UI
// {
//     public class InventoryController : MonoBehaviour
//     {
//         public Action<int, int> OnItemDragged;
//         [SerializeField]
//         public InventoryView view = new();
//         [SerializeField]
//         public InventoryModel model = new();
//         private bool _isDragging;
//         InventorySlot _dragOrigin;

//         void Start()
//         {
//             Initialize();
//         }

//         async void Initialize()
//         {
//             model.Initialize();
//             await view.InitializeView(model.Items, model.Gear);


//             view.AllSlots.ForEach(s =>
//             {
//                 s.RegisterCallback<ClickEvent>(Click);
//                 s.RegisterCallback<PointerDownEvent>(DragStart);
//                 s.RegisterCallback<PointerUpEvent>(DragEnd);
//             });

//             view.Root.RegisterCallback<PointerMoveEvent>(DragMove);
//             view.Root.RegisterCallback<PointerUpEvent>(DragEnd);
//         }

//         public async Task Show()
//         {
//             await Task.Yield();
//             view.Show();
//         }

//         public async Task Hide()
//         {
//             view.Hide();
//             await Task.Yield();
//         }

//         private void Click(ClickEvent evt)
//         {
//             var slot = evt.target as InventorySlot;
//             slot?.Item?.OnItemUsed?.Invoke(slot.Item);
//         }

//         private void DragStart(PointerDownEvent evt)
//         {
//             _dragOrigin = evt.target as InventorySlot;
//             _isDragging = _dragOrigin != null && _dragOrigin.Item != null;
//         }

//         private void DragMove(PointerMoveEvent evt)
//         {
//             if (!_isDragging) return;

//             var pos = evt.position;
//             view.ShowDraggable(pos, _dragOrigin.Item);
//         }

//         private void DragEnd(PointerUpEvent evt)
//         {
//             _isDragging = false;
//             view.HideDraggable();

//             var dragTarget = evt.target as InventorySlot;

//             if (_dragOrigin?.Item == null || dragTarget?.Item != null)
//                 return;

//             var targetInventoryIndex = view.GetInventoryIndex(dragTarget);
//             var targetGearIndex = view.GetGearIndex(dragTarget);

//             if (targetInventoryIndex >= 0)
//                 model.MoveItemToInventory(_dragOrigin?.Item, targetInventoryIndex);
//             else if (targetGearIndex >= 0)
//                 model.MoveItemToGear(_dragOrigin?.Item, targetGearIndex);

//             RefreshView();
//         }

//         void RefreshView()
//         {
//             view.Refresh(model.Items, model.Gear);
//         }
//     }
// }