namespace SwordFateCultivationRecord;

public enum BgmLoopMode { List, Single, Random }

public static class GameSettings
{
    public record ResolutionPreset(string Label, int Width, int Height);

    public static readonly ResolutionPreset[] Presets =
    {
        new("1280×720 (HD)", 1280, 720),
        new("1600×900 (HD+)", 1600, 900),
        new("1920×1080 (Full HD)", 1920, 1080),
        new("2560×1440 (2K)", 2560, 1440),
    };

    private static readonly string ConfigPath = "user://settings.cfg";

    // Display
    public static int ResolutionIndex { get; set; } = 1; // 1600×900
    public static bool Fullscreen { get; set; }
    public static bool VSync { get; set; } = true;

    // Audio
    public static float MasterVolume { get; set; } = 0.8f;
    public static float MusicVolume { get; set; } = 0.7f;
    public static float SfxVolume { get; set; } = 1.0f;
    public static int BgmIndex { get; set; }
    public static BgmLoopMode BgmMode { get; set; } = BgmLoopMode.List;

    // Gameplay
    public static bool AutoSave { get; set; } = true;
    public static int AutoSaveInterval { get; set; } = 7; // days

    public static void Load()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(ConfigPath) == Error.Ok)
        {
            ResolutionIndex = ClampI((int)cfg.GetValue("display", "resolution", 1).AsInt32(), 0, Presets.Length - 1);
            Fullscreen = (bool)cfg.GetValue("display", "fullscreen", false).AsBool();
            VSync = (bool)cfg.GetValue("display", "vsync", true).AsBool();
            MasterVolume = ClampF((float)cfg.GetValue("audio", "master_vol", 0.8).AsDouble(), 0, 1);
            MusicVolume = ClampF((float)cfg.GetValue("audio", "music_vol", 0.7).AsDouble(), 0, 1);
            SfxVolume = ClampF((float)cfg.GetValue("audio", "sfx_vol", 1.0).AsDouble(), 0, 1);
            AutoSave = (bool)cfg.GetValue("game", "auto_save", true).AsBool();
            AutoSaveInterval = ClampI((int)cfg.GetValue("game", "auto_save_interval", 7).AsInt32(), 1, 30);
            BgmIndex = (int)cfg.GetValue("audio", "bgm_index", 0).AsInt32();
            BgmMode = (BgmLoopMode)(int)cfg.GetValue("audio", "bgm_mode", 0).AsInt32();
        }
        ApplyDisplay();
        ApplyAudio();
    }

    public static void Save()
    {
        var cfg = new ConfigFile();
        cfg.SetValue("display", "resolution", ResolutionIndex);
        cfg.SetValue("display", "fullscreen", Fullscreen);
        cfg.SetValue("display", "vsync", VSync);
        cfg.SetValue("audio", "master_vol", MasterVolume);
        cfg.SetValue("audio", "music_vol", MusicVolume);
        cfg.SetValue("audio", "sfx_vol", SfxVolume);
        cfg.SetValue("game", "auto_save", AutoSave);
        cfg.SetValue("game", "auto_save_interval", AutoSaveInterval);
        cfg.SetValue("audio", "bgm_index", BgmIndex);
        cfg.SetValue("audio", "bgm_mode", (int)BgmMode);
        cfg.Save(ConfigPath);
    }

    public static void ApplyDisplay()
    {
        var preset = Presets[ResolutionIndex];
        bool inEditor = OS.HasFeature("editor");

        if (Fullscreen && !inEditor)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            if (!inEditor)
            {
                var screenSize = DisplayServer.ScreenGetSize();
                int x = Math.Max(0, (screenSize.X - preset.Width) / 2);
                int y = Math.Max(0, (screenSize.Y - preset.Height) / 2);
                DisplayServer.WindowSetPosition(new Vector2I(x, y));
                DisplayServer.WindowSetSize(new Vector2I(preset.Width, preset.Height));
            }
        }
        DisplayServer.WindowSetVsyncMode(VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }

    public static void ApplyAudio()
    {
        AudioManager.UpdateBGMVolume();
    }

    public static void ResetDefaults()
    {
        ResolutionIndex = 1;
        Fullscreen = false;
        VSync = true;
        MasterVolume = 0.8f;
        MusicVolume = 0.7f;
        SfxVolume = 1.0f;
        AutoSave = true;
        AutoSaveInterval = 7;
        BgmMode = BgmLoopMode.List;
        Save();
        ApplyDisplay();
        ApplyAudio();
    }

    private static int ClampI(int v, int min, int max) => Math.Max(min, Math.Min(max, v));
    private static float ClampF(float v, float min, float max) => Math.Max(min, Math.Min(max, v));

    public static int CurrentWidth => Presets[ResolutionIndex].Width;
    public static int CurrentHeight => Presets[ResolutionIndex].Height;
}
