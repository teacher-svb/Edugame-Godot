

using System;
using System.Linq;
using System.Threading.Tasks;
using TnT.Extensions;
using TnT.EduGame.Inventory;
using static TnT.EduGame.Inventory.Inventory;
using Godot;

namespace TnT.Systems.UI
{
    public partial class InventoryController : Node
    {
        [Export]
        public InventoryView view = new();
        [Export]
        public InventoryModel model = new();
    }
}