using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.UIAnimation;

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
            _resumeButton.Pressed   += () => OnResume?.Invoke();
            _settingsButton.Pressed += () => OnSettings?.Invoke();
            _saveButton.Pressed     += () => OnSave?.Invoke();
            _loadButton.Pressed     += () => OnLoad?.Invoke();
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
