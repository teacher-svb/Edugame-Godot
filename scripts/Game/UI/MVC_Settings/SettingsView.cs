using System;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class SettingsView : Control
    {
        [Export] Control _audioPanel;
        [Export] Control _displayPanel;
        [Export] Control _controlsPanel;
        [Export] Control _accessibilityPanel;
        [Export] Button _backButton;

        public event Action OnBack;

        public override void _Ready()
        {
            // ProcessMode = ProcessModeEnum.Always;
            Modulate = Colors.Transparent;
            // Visible = false;
            // _backButton.Pressed += () => OnBack?.Invoke();
        }

        public void ShowPanels(bool audio, bool display, bool controls, bool accessibility)
        {
            // _audioPanel.Visible         = audio;
            // _displayPanel.Visible       = display;
            // _controlsPanel.Visible      = controls;
            // _accessibilityPanel.Visible = accessibility;
        }

        public async Task ShowView(float duration = 0.2f)
        {
            // Visible = true;
            // var start = Modulate;
            // await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
            //     Modulate = start.Lerp(Colors.White, t);
        }

        public async Task HideView(float duration = 0.2f)
        {
            // var start = Modulate;
            // await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
            //     Modulate = start.Lerp(Colors.Transparent, t);
            // Visible = false;
        }
    }
}
