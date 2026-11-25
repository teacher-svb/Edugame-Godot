using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class FadeView : Control
    {
        Node _root;
        ColorRect _fadeRect;

        public void InitializeView()
        {
            _root = this.FindAncestorOfType<FadeController>();

            _fadeRect = this.FindAnyObjectByType<ColorRect>(includeSelf: true);
            _fadeRect.Modulate = new Color(1f, 1f, 1f, 0f);
            _fadeRect.SetAnchorsPreset(LayoutPreset.FullRect);
            this.Visible = false;
        }

        // drawn on its own CanvasLayer (UIManager/FadeLayer in managers.tscn), above every other UI
        public async Task Show(float durationSeconds = 1f)
        {
            this.Visible = true;
            await _fadeRect.FadeIn(durationSeconds);
        }

        public async Task Hide(float durationSeconds = 1f)
        {
            // not when a Show took over during the fade-out
            if (await _fadeRect.FadeOut(durationSeconds))
                this.Visible = false;
        }
    }
}
