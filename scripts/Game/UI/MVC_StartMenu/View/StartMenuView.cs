using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.UIAnimation;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class StartMenuView : Control
    {
        [Export] Button _newGameButton;
        [Export] Button _loadGameButton;

        public event Action OnNewGame;
        public event Action OnLoadGame;

        public override void _Ready()
        {
            _newGameButton.Pressed  += () => OnNewGame?.Invoke();
            _loadGameButton.Pressed += () => OnLoadGame?.Invoke();
        }

        public void SetLoadEnabled(bool enabled) => _loadGameButton.Disabled = !enabled;

        public async Task ShowView(float duration = 0.3f)
        {
            Visible = true;
            await this.FadeIn(duration);
        }

        public async Task HideView(float duration = 0.3f)
        {
            await this.FadeOut(duration);
            Visible = false;
        }
    }
}
