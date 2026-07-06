using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class FadeView : Control
    {
        Node _root;
        ColorRect _fadeRect;

        public async Task InitializeView()
        {
            _root = this.FindAncestorOfType<FadeController>();


            _fadeRect = this.FindAnyObjectByType<ColorRect>();
            _fadeRect.Modulate = new Color(1f, 1f, 1f, 0f);
            _fadeRect.SetAnchorsPreset(LayoutPreset.FullRect);
            _fadeRect.Visible = false;

            await Task.Yield();
        }

        public async Task Show(float durationSeconds = 1f)
        {
            var canvas = this.FindAncestorOfType<CanvasLayer>();
            canvas.Layer = 999;

            _fadeRect.Visible = true;
            await _fadeRect.FadeIn(durationSeconds);
        }

        public async Task Hide(float durationSeconds = 1f)
        {
            await _fadeRect.FadeOut(durationSeconds);
            _fadeRect.Visible = false;

            var canvas = this.FindAncestorOfType<CanvasLayer>();
            canvas.Layer = -1;
        }
    }
}
