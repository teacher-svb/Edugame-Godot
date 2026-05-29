using System;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class PauseMenuView : Control
    {
        [Export] Button _resumeButton;
        [Export] Button _settingsButton;
        [Export] Button _saveButton;
        [Export] Button _loadButton;

        public event Action OnResume;
        public event Action OnSettings;
        public event Action OnSave;
        public event Action OnLoad;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Modulate = Colors.Transparent;
            Visible = false;
            _resumeButton.Pressed  += () => OnResume?.Invoke();
            _settingsButton.Pressed += () => OnSettings?.Invoke();
            _saveButton.Pressed    += () => OnSave?.Invoke();
            _loadButton.Pressed    += () => OnLoad?.Invoke();
        }

        public async Task ShowView(float duration = 0.2f)
        {
            Visible = true;
            var start = Modulate;
            await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
                Modulate = start.Lerp(Colors.White, t);
        }

        public async Task HideView(float duration = 0.2f)
        {
            var start = Modulate;
            await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
                Modulate = start.Lerp(Colors.Transparent, t);
            Visible = false;
        }
    }
}
