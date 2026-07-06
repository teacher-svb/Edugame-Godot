using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Inventory;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class InventoryView : Control
    {
        [Export]
        Control[] _itemContainers = new Control[10];

        public void InitializeView(IEnumerable<ItemData> inventoryItems)
        {
            Modulate = Colors.Transparent;
            FillSlots(inventoryItems);
        }

        public void RefreshView(IEnumerable<ItemData> inventoryItems)
        {
            FillSlots(inventoryItems);
        }

        private void FillSlots(IEnumerable<ItemData> inventoryItems)
        {
            var items = inventoryItems.ToArray();

            for (int i = 0; i < _itemContainers.Length; i++)
            {
                var slot = _itemContainers[i];
                if (slot == null)
                    continue;
                foreach (var child in slot.GetChildren())
                    child.QueueFree();

                if (i < items.Length && items[i] != null)
                    slot.AddChild(MakeIcon(items[i].Icon));
            }
        }

        public async Task ShowView(float duration = 0.2f)
        {
            await this.FadeIn(duration);
            Visible = true;
        }

        public async Task HideView(float duration = 0.2f)
        {
            await this.FadeOut(duration);
            Visible = false;
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
