using System;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.UIAnimation;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class PauseMenuView : Control
    {
        [Export] Button _resumeButton;
        [Export] Button _settingsButton;
        [Export] Button _saveButton;
        [Export] Button _loadButton;
        [Export] Button _quitButton;

        public event Action OnResume;
        public event Action OnSettings;
        public event Action OnSave;
        public event Action OnLoad;
        public event Action OnQuit;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Modulate = Colors.Transparent;
            Visible = false;
            _resumeButton.Pressed   += () => OnResume?.Invoke();
            _settingsButton.Pressed += () => OnSettings?.Invoke();
            _saveButton.Pressed     += () => OnSave?.Invoke();
            _loadButton.Pressed     += () => OnLoad?.Invoke();
            if (_quitButton != null)
                _quitButton.Pressed += () => OnQuit?.Invoke();
        }

        public void SetLoadEnabled(bool enabled) => SetEnabled(_loadButton, enabled);

        // disabled with a reason on hover: while something in the game still runs (a quest reaction)
        public void SetSaveEnabled(bool enabled, string reason = "")
        {
            SetEnabled(_saveButton, enabled);
            _saveButton.TooltipText = enabled ? "" : reason;
        }

        // a disabled button keeps the keyboard focus but can't be pressed: hand it to the first one that can
        void SetEnabled(Button button, bool enabled)
        {
            button.Disabled = !enabled;
            if (!enabled && button.HasFocus())
                FocusFirst();
        }

        /// <summary>Keyboard/controller focus on the first enabled button, top to bottom.</summary>
        public void FocusFirst()
        {
            if (!IsVisibleInTree())
                return;
            foreach (var button in new[] { _resumeButton, _saveButton, _loadButton, _settingsButton })
            {
                if (button != null && button.IsVisibleInTree() && !button.Disabled)
                {
                    button.GrabFocus();
                    return;
                }
            }
        }

        /// <summary>Takes the focus away while a screen on top (settings) is used: Space/Enter would press the
        /// pause menu's focused button behind it.</summary>
        public void ReleaseMenuFocus()
        {
            if (GetViewport()?.GuiGetFocusOwner() is Control owner && IsAncestorOf(owner))
                owner.ReleaseFocus();
        }

        public async Task ShowView(float duration = 0.2f)
        {
            Visible = true;
            FocusFirst();
            await this.FadeIn(duration);
        }

        public async Task HideView(float duration = 0.2f)
        {
            ReleaseMenuFocus();
            // not when a ShowView took over during the fade-out
            if (await this.FadeOut(duration))
                Visible = false;
        }
    }
}
