
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class InventoryUI : Control
    {
        [Export]
        Control[] _itemContainers = new Control[10];
    }
}