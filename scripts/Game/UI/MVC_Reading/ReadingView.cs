using System.Threading.Tasks;
using Godot;
using TnT.EduGame.Reading;
using TnT.Systems.UIAnimation;

namespace TnT.EduGame.UI
{
    /// <summary>
    /// The reading panel: the game dims, a sheet (letter paper, notice, or plain) shows the title and one page of the
    /// text at a readable size, with the page number and what next/close do. Runs while the game is paused.
    /// </summary>
    [GlobalClass]
    public partial class ReadingView : Control
    {
        [Export] ColorRect _dim;
        [Export] PanelContainer _sheet;
        [Export] Label _title;
        [Export] RichTextLabel _body;
        [Export] Label _page;
        [Export] Label _hint;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Visible = false;
        }

        public async Task ShowView(string title, ReadableLook look, float fade = 0.3f)
        {
            ApplyLook(look);
            _title.Text = title;
            _title.Visible = !string.IsNullOrEmpty(title);
            Modulate = Colors.Transparent;
            // on its own CanvasLayer (UIManager/ReadingLayer in managers.tscn): above the game UI, below the act card
            Visible = true;
            await this.FadeIn(fade);
        }

        public void ShowPage(string text, int index, int count, bool isLast)
        {
            _body.Text = text;
            _body.ScrollToLine(0);
            _page.Text = count > 1 ? $"{index + 1} / {count}" : "";
            _hint.Text = isLast ? "Spatie: sluiten" : "Spatie: volgende bladzijde   ·   Esc: sluiten";
        }

        public async Task TurnPage(string text, int index, int count, bool isLast, float fade = 0.15f)
        {
            var tween = _body.CreateTween();
            tween.TweenProperty(_body, "modulate:a", 0f, fade);
            await ToSignal(tween, Tween.SignalName.Finished);
            ShowPage(text, index, count, isLast);
            tween = _body.CreateTween();
            tween.TweenProperty(_body, "modulate:a", 1f, fade);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public async Task HideView(float fade = 0.3f)
        {
            // not when a ShowView took over during the fade-out
            if (await this.FadeOut(fade))
                Visible = false;
        }

        public string ShownText => _body.Text;

        void ApplyLook(ReadableLook look)
        {
            // paper and ink per look; the sheet's own StyleBoxFlat is copied so the scene resource isn't changed
            var (paper, border, ink) = look switch
            {
                ReadableLook.Letter => (new Color(0.96f, 0.91f, 0.79f), new Color(0.55f, 0.38f, 0.2f), new Color(0.2f, 0.13f, 0.07f)),
                ReadableLook.Notice => (new Color(0.93f, 0.88f, 0.74f), new Color(0.36f, 0.24f, 0.13f), new Color(0.12f, 0.1f, 0.08f)),
                _ => (new Color(0.12f, 0.12f, 0.14f), new Color(0.6f, 0.6f, 0.65f), new Color(0.95f, 0.95f, 0.95f)),
            };
            var style = (_sheet.GetThemeStylebox("panel") as StyleBoxFlat)?.Duplicate() as StyleBoxFlat ?? new StyleBoxFlat();
            style.BgColor = paper;
            style.BorderColor = border;
            _sheet.AddThemeStyleboxOverride("panel", style);
            _title.AddThemeColorOverride("font_color", ink);
            _body.AddThemeColorOverride("default_color", ink);
            _page.AddThemeColorOverride("font_color", ink with { A = 0.7f });
            _hint.AddThemeColorOverride("font_color", ink with { A = 0.7f });
        }
    }
}
