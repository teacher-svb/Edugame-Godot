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
        [Export] NodePath _fadeRectPath;

        Color _from = Color.FromHsv(0, 0, 0, 1);
        Color _to = Color.FromHsv(0, 0, 0, 0);

        public async Task InitializeView(Control root)
        {
            _root = root;
            // _root.Clear();

            _fadeRect = root.GetNode(_fadeRectPath) as ColorRect;
            // _fadeRect = _root.CreateChild<ColorRect>();
            _fadeRect.Color = _to;
            _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            await Task.Yield();
        }

        public async Task Show()
        {
            _root.ZIndex = 10;

            var steps = 100;

            for (float i = 0; i < 1; i += 1f/steps)
            {
                await Task.Delay(1000/steps);
                _fadeRect.Color = _fadeRect.Color.Lerp(_from, i);
            }
        }

        public async Task Hide()
        {
            var steps = 100;

            for (float i = 0; i < 1; i += 1f/steps)
            {
                await Task.Delay(1000/steps);
                _fadeRect.Color = _fadeRect.Color.Lerp(_to, i);
            }
            _root.ZIndex = 0;
        }
    }
}