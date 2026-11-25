using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Inventory;
using static TnT.EduGame.UI.InventoryModel;

namespace TnT.EduGame.UI
{
    public partial class InventoryController : Node
    {
        public static InventoryController Instance { get; private set; }

        [Export] public InventoryView view;
        [Export] public InventoryModel model;
        [Export] ItemEventListener _pickupItemListener;
        [Export] ItemEventChannel _useItemChannel;
        [Export] ItemEventChannel _removeItemChannel;

        public override void _Ready()
        {
            Instance = this;
            model.OnInventoryChanged += () => view.RefreshView(model.Items);
            model.OnItemAction += (item, type) =>
            {
                switch (type)
                {
                    case ItemEventType.ITEM_USED:
                        _useItemChannel?.Invoke(item);
                        break;
                    case ItemEventType.ITEM_REMOVED:
                        _removeItemChannel?.Invoke(item);
                        break;
                    default:
                        break;
                }
                // no RefreshView here: OnInventoryChanged (raised just before) already rebuilt the slots
            };
            // the pickup channel asks first (Accepts), so a full inventory leaves the item in the world
            _pickupItemListener.OnItemEvent += item => model.AddItem(item);
            _pickupItemListener.Accepts = _ => model.HasFreeSlot;
            view.InitializeView(model.Items);
        }

        public async Task Show()
        {
            await view.ShowView();
        }

        public async Task Hide()
        {
            await view.HideView();
        }
    }
}
