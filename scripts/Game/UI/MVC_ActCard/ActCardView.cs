using System.Threading.Tasks;
using Godot;
using TnT.Extensions;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    /// <summary>
    /// A full-screen card between acts: the screen fades to black, then the title and subtitle fade in; the hint
    /// shows once the card can be closed. Runs while the game is paused.
    /// </summary>
    [GlobalClass]
    public partial class ActCardView : Control
    {
        [Export] ColorRect _background;
        [Export] Control _text;
        [Export] Label _title;
        [Export] Label _subtitle;
        [Export] Control _hint;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Visible = false;
        }

        public async Task ShowView(string title, string subtitle, float fade = 1f)
        {
            _title.Text = title;
            _subtitle.Text = subtitle;
            _background.Modulate = Colors.Transparent;
            _text.Modulate = Colors.Transparent;
            _hint.Modulate = Colors.Transparent;
            Modulate = Colors.White;
            Visible = true;
            var canvas = this.FindAncestorOfType<CanvasLayer>();
            if (canvas != null)
                canvas.Layer = 999;   // above the game, like the fade

            await _background.FadeIn(fade);
            await _text.FadeIn(fade);
        }

        public async Task ShowHint(float duration = 0.5f) => await _hint.FadeIn(duration);

        public async Task HideView(float fade = 1f)
        {
            await this.FadeOut(fade);
            Visible = false;
            var canvas = this.FindAncestorOfType<CanvasLayer>();
            if (canvas != null)
                canvas.Layer = -1;
        }
    }
}
