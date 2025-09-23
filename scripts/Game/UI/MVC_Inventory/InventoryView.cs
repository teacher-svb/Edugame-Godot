using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Inventory;
using TnT.Extensions;

namespace TnT.Systems.UI
{

    [GlobalClass]
    public partial class InventoryView : Control
    {
        public async Task InitializeView(IEnumerable<Item> inventoryItems, IEnumerable<Item> gearItems)
        {
        }
    }
}
