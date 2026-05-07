using System;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;
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

            _fadeRect = root.GetNode(_fadeRectPath) as ColorRect;
            _fadeRect.Color = _to;
            _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            await Task.Yield();
        }

        public async Task Show(float durationSeconds = 1f)
        {
            _root.ZIndex = 10;
            var startColor = _fadeRect.Color;

            await foreach (var t in Easings.Easings.Animate(durationSeconds, Ease.Linear))
            {
                _fadeRect.Color = startColor.Lerp(_from, t);
            }
        }

        public async Task Hide(float durationSeconds = 1f)
        {
            var startColor = _fadeRect.Color;

            await foreach (var t in Easings.Easings.Animate(durationSeconds, Ease.Linear))
            {
                _fadeRect.Color = startColor.Lerp(_to, t);
            }

            _root.ZIndex = 0;
        }
    }
}