using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.UIAnimation;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class SettingsView : Control
    {
        [Export] Control _audioPanel;
        [Export] Control _displayPanel;
        [Export] Control _accessibilityPanel;
        [Export] Button _backButton;

        public event Action OnBack;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Modulate = Colors.Transparent;
            Visible = false;
            _backButton.Pressed += () => OnBack?.Invoke();
        }

        public void ShowPanels(bool audio, bool display, bool controls, bool accessibility)
        {
            _audioPanel.Visible         = audio;
            _displayPanel.Visible       = display;
            _accessibilityPanel.Visible = accessibility;
        }

        public async Task ShowView(float duration = 0.2f)
        {
            Visible = true;
            await this.FadeIn(duration);
        }

        public async Task HideView(float duration = 0.2f)
        {
            await this.FadeOut(duration);
            Visible = false;
        }
    }
}
