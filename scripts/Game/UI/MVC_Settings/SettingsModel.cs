using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.UI
{
    /// <summary>
    /// The player's settings, kept in <c>user://settings.cfg</c> (not in a save game). Loaded and applied at start
    /// (the managers autoload), written only when a value changed: a setter applies the value at once, and with
    /// <c>save: false</c> (a slider being dragged) the file is written later by <see cref="SaveIfChanged"/>.
    /// </summary>
    [GlobalClass]
    public partial class SettingsModel : Node
    {
        private const string SavePath = "user://settings.cfg";

        // bus names as in default_bus_layout.tres (MusicManager plays on "Music")
        public const string MusicBus = "Music";
        public const string SfxBus = "SFX";

        public const float MinTextSizeScale = 1f;
        public const float MaxTextSizeScale = 1.5f;
        public const float TextSizeScaleStep = 0.25f;

        /// <summary>
        /// The UI theme whose font sizes the text size scales (every <c>font_size</c> in it and its default font
        /// size). Text with its own size in a scene (<c>theme_override_font_sizes</c>, LabelSettings) is not scaled.
        /// </summary>
        [Export] Theme _textTheme;

        // Audio (linear, 0..1)
        public float MusicVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;

        // Display
        public bool IsFullscreen { get; private set; } = false;

        // Accessibility
        public float TextSizeScale { get; private set; } = 1f;

        bool _dirty;
        // the theme's own font sizes, before any scaling: (theme type, name) → size; the default font size under ("", "")
        Dictionary<(string type, string name), int> _baseFontSizes;

        public override void _Ready() => Load();

        // ── Audio ────────────────────────────────────────────────────────────

        public void SetMusicVolume(float linear, bool save = true)
        {
            float value = Mathf.Clamp(linear, 0f, 1f);
            MarkChanged(!Mathf.IsEqualApprox(MusicVolume, value));
            MusicVolume = value;
            ApplyBusVolume(MusicBus, MusicVolume);
            if (save)
                SaveIfChanged();
        }

        public void SetSfxVolume(float linear, bool save = true)
        {
            float value = Mathf.Clamp(linear, 0f, 1f);
            MarkChanged(!Mathf.IsEqualApprox(SfxVolume, value));
            SfxVolume = value;
            ApplyBusVolume(SfxBus, SfxVolume);
            if (save)
                SaveIfChanged();
        }

        static void ApplyBusVolume(string busName, float linear)
        {
            int bus = AudioServer.GetBusIndex(busName);
            if (bus < 0)
            {
                GD.PushWarning($"Settings: no audio bus '{busName}'");
                return;
            }
            AudioServer.SetBusVolumeDb(bus, Mathf.LinearToDb(linear));   // 0 → -inf dB (silent)
        }

        // ── Display ──────────────────────────────────────────────────────────

        public void SetFullscreen(bool fullscreen, bool save = true)
        {
            MarkChanged(IsFullscreen != fullscreen);
            IsFullscreen = fullscreen;
            ApplyFullscreen();
            if (save)
                SaveIfChanged();
        }

        void ApplyFullscreen()
        {
            if (DisplayServer.GetName() == "headless")
                return;
            var current = DisplayServer.WindowGetMode();
            bool isFullscreen = current is DisplayServer.WindowMode.Fullscreen or DisplayServer.WindowMode.ExclusiveFullscreen;
            if (isFullscreen != IsFullscreen)
                DisplayServer.WindowSetMode(IsFullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        }

        // ── Accessibility ────────────────────────────────────────────────────

        public void SetTextSizeScale(float scale, bool save = true)
        {
            float value = Mathf.Clamp(scale, MinTextSizeScale, MaxTextSizeScale);
            bool changed = !Mathf.IsEqualApprox(TextSizeScale, value);
            MarkChanged(changed);
            TextSizeScale = value;
            if (changed)
                ApplyTextSize();
            if (save)
                SaveIfChanged();
        }

        // Scales every font size of the UI theme from its original value (no rounding drift over repeated changes).
        void ApplyTextSize()
        {
            if (_textTheme == null || (_baseFontSizes == null && Mathf.IsEqualApprox(TextSizeScale, 1f)))
                return;   // nothing scaled yet: the theme stays untouched
            if (_baseFontSizes == null)
            {
                _baseFontSizes = new();
                if (_textTheme.HasDefaultFontSize())
                    _baseFontSizes[("", "")] = _textTheme.DefaultFontSize;
                foreach (var type in _textTheme.GetFontSizeTypeList())
                    foreach (var name in _textTheme.GetFontSizeList(type))
                        _baseFontSizes[(type, name)] = _textTheme.GetFontSize(name, type);
            }

            // one "changed" for the whole theme, not one per font size
            _textTheme.SetBlockSignals(true);
            foreach (var ((type, name), size) in _baseFontSizes)
            {
                int scaled = Mathf.RoundToInt(size * TextSizeScale);
                if (type == "" && name == "")
                    _textTheme.DefaultFontSize = scaled;
                else
                    _textTheme.SetFontSize(name, type, scaled);
            }
            _textTheme.SetBlockSignals(false);
            _textTheme.EmitChanged();
        }

        // ── Controls ─────────────────────────────────────────────────────────
        // Rebinding: add per-action methods here when ready

        // ── Persistence ──────────────────────────────────────────────────────

        void MarkChanged(bool changed)
        {
            if (changed)
                _dirty = true;
        }

        /// <summary>Writes the settings file when a value changed since the last write; true when it wrote.</summary>
        public bool SaveIfChanged()
        {
            if (!_dirty)
                return false;
            var cfg = new ConfigFile();
            cfg.SetValue("audio", "music", MusicVolume);
            cfg.SetValue("audio", "sfx", SfxVolume);
            cfg.SetValue("display", "fullscreen", IsFullscreen);
            cfg.SetValue("accessibility", "text_size_scale", TextSizeScale);
            var error = cfg.Save(SavePath);
            if (error != Error.Ok)
            {
                GD.PushError($"Settings: could not write {SavePath} ({error})");
                return false;
            }
            _dirty = false;
            return true;
        }

        // reads the file (missing: the defaults) and applies every value, without writing anything
        private void Load()
        {
            var cfg = new ConfigFile();
            if (cfg.Load(SavePath) == Error.Ok)
            {
                MusicVolume   = Mathf.Clamp(cfg.GetValue("audio", "music", 1f).AsSingle(), 0f, 1f);
                SfxVolume     = Mathf.Clamp(cfg.GetValue("audio", "sfx", 1f).AsSingle(), 0f, 1f);
                IsFullscreen  = cfg.GetValue("display", "fullscreen", false).AsBool();
                TextSizeScale = Mathf.Clamp(cfg.GetValue("accessibility", "text_size_scale", 1f).AsSingle(), MinTextSizeScale, MaxTextSizeScale);
            }

            ApplyBusVolume(MusicBus, MusicVolume);
            ApplyBusVolume(SfxBus, SfxVolume);
            ApplyFullscreen();
            ApplyTextSize();
            _dirty = false;
        }
    }
}
