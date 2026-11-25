using System;
using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class SettingsController : Node
    {
        public static SettingsController Instance { get; private set; }

        [Export] public SettingsView view;
        [Export] public SettingsModel model;

        public event Action OnBack;

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            view.OnBack += () => OnBack?.Invoke();
            view.SetTextSizeRange(SettingsModel.MinTextSizeScale, SettingsModel.MaxTextSizeScale, SettingsModel.TextSizeScaleStep);

            // applied at once (a volume is heard while dragging); the file is written when the edit is done
            view.MusicVolumeChanged += v => model.SetMusicVolume(v, save: false);
            view.SfxVolumeChanged   += v => model.SetSfxVolume(v, save: false);
            view.TextSizeChanged    += v => model.SetTextSizeScale(v, save: false);
            view.FullscreenToggled  += on => model.SetFullscreen(on);
            view.EditFinished       += () => model.SaveIfChanged();
        }

        public async Task Show(bool audio, bool display, bool controls, bool accessibility)
        {
            view.SetValues(model.MusicVolume, model.SfxVolume, model.IsFullscreen, model.TextSizeScale);
            view.ShowPanels(audio, display, controls, accessibility);
            await view.ShowView();
        }

        public async Task Hide()
        {
            model.SaveIfChanged();   // a change not finished yet (e.g. closed with Escape while dragging)
            await view.HideView();
        }
    }
}
