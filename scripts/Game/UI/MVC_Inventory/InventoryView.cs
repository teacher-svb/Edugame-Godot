using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Inventory;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class InventoryView : Control
    {
        [Export]
        Control[] _itemContainers = new Control[10];

        public async Task InitializeView(IEnumerable<Item> inventoryItems)
        {
            var items = inventoryItems.ToArray();
            for (int i = 0; i < _itemContainers.Length; i++)
            {
                var slot = _itemContainers[i];
                foreach (var child in slot.GetChildren())
                    child.QueueFree();

                if (i < items.Length && items[i] != null)
                    slot.AddChild(MakeIcon(items[i].Icon));
            }
            await Task.CompletedTask;
        }

        public async Task ShowView(float duration = .2f)
        {
            Visible = true;
            await Task.CompletedTask;
        }

        public async Task HideView(float duration = .2f)
        {
            Visible = false;
            await Task.CompletedTask;
        }

        private TextureRect MakeIcon(Texture2D texture)
        {
            var icon = new TextureRect();
            icon.Texture = texture;
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            return icon;
        }
    }
}
