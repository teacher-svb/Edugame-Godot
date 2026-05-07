using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Inventory;
using static TnT.Systems.UI.InventoryModel;

namespace TnT.Systems.UI
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
            model.OnInventoryChanged += () => view.InitializeView(model.Items);
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

            };
            _pickupItemListener.OnItemEvent += model.AddItem;
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
