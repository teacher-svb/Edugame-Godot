using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TnT.Systems.UIAnimation;
using Range = Godot.Range;

namespace TnT.EduGame.UI
{
    /// <summary>
    /// The settings screen (<c>Scenes/UI/SettingsUI.tscn</c>). Shows the values the controller hands it
    /// (<see cref="SetValues"/>, without raising events) and reports the player's changes; a slider reports while it is
    /// dragged and raises <see cref="EditFinished"/> when it is let go (or after a click / key / wheel step), so the
    /// controller writes the settings file once per change.
    /// </summary>
    [GlobalClass]
    public partial class SettingsView : Control
    {
        [Export] Control _audioPanel;
        [Export] Control _displayPanel;
        [Export] Control _controlsPanel;        // optional: there are no control settings yet
        [Export] Control _accessibilityPanel;
        // the heading above each panel, hidden with it (optional)
        [Export] Control _audioHeading;
        [Export] Control _displayHeading;
        [Export] Control _controlsHeading;
        [Export] Control _accessibilityHeading;

        [Export] Range _musicSlider;
        [Export] Range _sfxSlider;
        [Export] BaseButton _fullscreenToggle;
        [Export] Range _textSizeSlider;
        [Export] Button _backButton;

        public event Action OnBack;
        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<bool> FullscreenToggled;
        public event Action<float> TextSizeChanged;
        /// <summary>A slider change is done (let go after a drag, or a single step): time to save.</summary>
        public event Action EditFinished;

        // set while the view itself changes a control (range, shown values): no change events then
        bool _silent;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            Modulate = Colors.Transparent;
            Visible = false;
            _backButton.Pressed += () => OnBack?.Invoke();

            ConfigureRange(_musicSlider, 0, 1, 0.05);
            ConfigureRange(_sfxSlider, 0, 1, 0.05);

            WireSlider(_musicSlider, v => MusicVolumeChanged?.Invoke(v));
            WireSlider(_sfxSlider, v => SfxVolumeChanged?.Invoke(v));
            WireSlider(_textSizeSlider, v => TextSizeChanged?.Invoke(v));
            if (_fullscreenToggle != null)
                _fullscreenToggle.Toggled += on =>
                {
                    if (!_silent)
                        FullscreenToggled?.Invoke(on);
                };
        }

        void WireSlider(Range range, Action<float> changed)
        {
            if (range == null)
                return;
            bool dragging = false;
            range.ValueChanged += value =>
            {
                if (_silent)
                    return;
                changed((float)value);
                if (!dragging)
                    EditFinished?.Invoke();
            };
            if (range is Slider slider)
            {
                slider.DragStarted += () => dragging = true;
                slider.DragEnded += _ =>
                {
                    dragging = false;
                    EditFinished?.Invoke();
                };
            }
        }

        // a new range can clamp the value: that is not the player's change
        void ConfigureRange(Range range, double min, double max, double step)
        {
            if (range == null)
                return;
            _silent = true;
            range.MinValue = min;
            range.MaxValue = max;
            range.Step = step;
            _silent = false;
        }

        /// <summary>The text size slider's range (the model's limits).</summary>
        public void SetTextSizeRange(float min, float max, float step) => ConfigureRange(_textSizeSlider, min, max, step);

        /// <summary>Shows these values; raises no change events.</summary>
        public void SetValues(float music, float sfx, bool fullscreen, float textSize)
        {
            _silent = true;
            _musicSlider?.SetValueNoSignal(music);
            _sfxSlider?.SetValueNoSignal(sfx);
            _fullscreenToggle?.SetPressedNoSignal(fullscreen);
            _textSizeSlider?.SetValueNoSignal(textSize);
            _silent = false;
        }

        public void ShowPanels(bool audio, bool display, bool controls, bool accessibility)
        {
            SetVisible(audio, _audioPanel, _audioHeading);
            SetVisible(display, _displayPanel, _displayHeading);
            SetVisible(controls, _controlsPanel, _controlsHeading);
            SetVisible(accessibility, _accessibilityPanel, _accessibilityHeading);
        }

        static void SetVisible(bool visible, params Control[] controls)
        {
            foreach (var control in controls)
                if (control != null)
                    control.Visible = visible;
        }

        public async Task ShowView(float duration = 0.2f)
        {
            Visible = true;
            FocusFirst();
            await this.FadeIn(duration);
        }

        public async Task HideView(float duration = 0.2f)
        {
            if (GetViewport()?.GuiGetFocusOwner() is Control owner && IsAncestorOf(owner))
                owner.ReleaseFocus();
            // not when a ShowView took over during the fade-out
            if (await this.FadeOut(duration))
                Visible = false;
        }

        // keyboard/controller: the first visible setting (arrows change a slider, up/down move), else Back
        void FocusFirst()
        {
            var first = new Control[] { _musicSlider, _sfxSlider, _fullscreenToggle, _textSizeSlider, _backButton }
                .FirstOrDefault(c => c != null && c.IsVisibleInTree() && c.FocusMode != FocusModeEnum.None);
            first?.GrabFocus();
        }
    }
}
