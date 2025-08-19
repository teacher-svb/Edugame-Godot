using System;
using System.Threading.Tasks;
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{

    [GlobalClass]
    public partial class FadeView : Resource
    {
        Control _root;
        ColorRect _fadeRect;
        // [Export] NodePath _fadeRectPath;

        public async Task InitializeView(Control root)
        {
            _root = root;
            _root.Clear();

            // _fadeRect = root.GetNode(_fadeRectPath) as ColorRect;
            _fadeRect = _root.CreateChild<ColorRect>();
            _fadeRect.Color = Colors.Black;
            _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            await Task.Yield();
        }

        public async Task Show()
        {
            _root.ZIndex = 10;
            await Task.Delay(1000);
        }

        public async Task Hide()
        {
            await Task.Delay(1000);
            _root.ZIndex = 0;
        }
    }
}