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
        [Export] Button _settingsButton;
        [Export] Button _quitButton;

        public event Action OnNewGame;
        public event Action OnLoadGame;
        public event Action OnSettings;
        public event Action OnQuit;

        public override void _Ready()
        {
            _newGameButton.Pressed  += () => OnNewGame?.Invoke();
            _loadGameButton.Pressed += () => OnLoadGame?.Invoke();
            if (_settingsButton != null)
                _settingsButton.Pressed += () => OnSettings?.Invoke();
            if (_quitButton != null)
                _quitButton.Pressed += () => OnQuit?.Invoke();
        }

        public void SetLoadEnabled(bool enabled) => _loadGameButton.Disabled = !enabled;

        /// <summary>Keyboard/controller focus on "Laden" when a save exists, else "Nieuw spel" (so Space/Enter never starts a new game over a save by accident).</summary>
        public void FocusFirst()
        {
            foreach (var button in new[] { _loadGameButton, _newGameButton })
            {
                if (button != null && button.IsVisibleInTree() && !button.Disabled)
                {
                    button.GrabFocus();
                    return;
                }
            }
        }

        public async Task ShowView(float duration = 0.3f)
        {
            Visible = true;
            FocusFirst();
            await this.FadeIn(duration);
        }

        public async Task HideView(float duration = 0.3f)
        {
            // no focus behind a screen opened on top (settings): Space/Enter would press a menu button
            if (GetViewport()?.GuiGetFocusOwner() is Control owner && IsAncestorOf(owner))
                owner.ReleaseFocus();
            if (await this.FadeOut(duration))
                Visible = false;
        }
    }
}
