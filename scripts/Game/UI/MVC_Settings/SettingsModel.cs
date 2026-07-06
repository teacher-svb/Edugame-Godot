using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class SettingsModel : Node
    {
        private const string SavePath = "user://settings.cfg";

        // Audio — bus names must match Project Settings > Audio > Buses
        public float MusicVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;

        // Display
        public bool IsFullscreen { get; private set; } = false;

        // Accessibility
        public float TextSizeScale { get; private set; } = 1f;

        public override void _Ready() => Load();

        // ── Audio ────────────────────────────────────────────────────────────

        public void SetMusicVolume(float linear)
        {
            MusicVolume = Mathf.Clamp(linear, 0f, 1f);
            int bus = AudioServer.GetBusIndex("Music");
            if (bus >= 0)
                AudioServer.SetBusVolumeDb(bus, Mathf.LinearToDb(MusicVolume));
            Save();
        }

        public void SetSfxVolume(float linear)
        {
            SfxVolume = Mathf.Clamp(linear, 0f, 1f);
            int bus = AudioServer.GetBusIndex("SFX");
            if (bus >= 0)
                AudioServer.SetBusVolumeDb(bus, Mathf.LinearToDb(SfxVolume));
            Save();
        }

        // ── Display ──────────────────────────────────────────────────────────

        public void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            DisplayServer.WindowSetMode(fullscreen
                ? DisplayServer.WindowMode.Fullscreen
                : DisplayServer.WindowMode.Windowed);
            Save();
        }

        // ── Accessibility ────────────────────────────────────────────────────

        public void SetTextSizeScale(float scale)
        {
            TextSizeScale = Mathf.Clamp(scale, 0.5f, 2f);
            // Apply to your theme via ThemeDB or a custom method
            Save();
        }

        // ── Controls ─────────────────────────────────────────────────────────
        // Rebinding: add per-action methods here when ready

        // ── Persistence ──────────────────────────────────────────────────────

        private void Save()
        {
            var cfg = new ConfigFile();
            cfg.SetValue("audio", "music", MusicVolume);
            cfg.SetValue("audio", "sfx", SfxVolume);
            cfg.SetValue("display", "fullscreen", IsFullscreen);
            cfg.SetValue("accessibility", "text_size_scale", TextSizeScale);
            cfg.Save(SavePath);
        }

        private void Load()
        {
            var cfg = new ConfigFile();
            if (cfg.Load(SavePath) != Error.Ok)
                return;

            MusicVolume = (float)cfg.GetValue("audio", "music", 1f);
            SfxVolume   = (float)cfg.GetValue("audio", "sfx", 1f);
            IsFullscreen = (bool)cfg.GetValue("display", "fullscreen", false);
            TextSizeScale = (float)cfg.GetValue("accessibility", "text_size_scale", 1f);

            SetMusicVolume(MusicVolume);
            SetSfxVolume(SfxVolume);
            SetFullscreen(IsFullscreen);
            SetTextSizeScale(TextSizeScale);
        }
    }
}
