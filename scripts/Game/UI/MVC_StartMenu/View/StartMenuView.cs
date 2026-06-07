using System;
using System.Threading.Tasks;
using Godot;
using TnT.Easings;

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
            _newGameButton.Pressed += () => OnNewGame?.Invoke();
            _loadGameButton.Pressed += () => OnLoadGame?.Invoke();
        }

        public void SetLoadEnabled(bool enabled) => _loadGameButton.Disabled = !enabled;

        public async Task ShowView(float duration = 0.3f)
        {
            Visible = true;
            var start = Modulate;
            await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
                Modulate = start.Lerp(Colors.White, t);
        }

        public async Task HideView(float duration = 0.3f)
        {
            var start = Modulate;
            await foreach (var t in Easings.Easings.Animate(duration, Ease.Linear))
                Modulate = start.Lerp(Colors.Transparent, t);
            Visible = false;
        }
    }
}
