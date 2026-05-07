using System.Threading.Tasks;
using Godot;

namespace TnT.Systems.UI
{
    public partial class InventoryController : Node
    {
        public static InventoryController Instance { get; private set; }

        [Export] public InventoryView view = new();
        [Export] public InventoryModel model = new();

        public override void _Ready()
        {
            Instance = this;
            model.Initialize();
        }

        public async Task Show()
        {
            await view.InitializeView(model.Items);
            await view.ShowView();
        }

        public async Task Hide()
        {
            await view.HideView();
        }
    }
}
