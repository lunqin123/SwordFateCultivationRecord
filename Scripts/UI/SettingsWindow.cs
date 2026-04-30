namespace SwordFateCultivationRecord;

public partial class SettingsWindow : Window
{
    private TabContainer _tabs = null!;
    private OptionButton _resDrop = null!;
    private CheckButton _fsToggle = null!, _vsyncToggle = null!;
    private HSlider _masterSlider = null!, _musicSlider = null!, _sfxSlider = null!;
    private Label _masterVal = null!, _musicVal = null!, _sfxVal = null!;
    private CheckButton _autoSaveToggle = null!;
    private OptionButton _autoSaveDrop = null!;

    public Action? OnSave { get; set; }
    public Action? OnLoad { get; set; }
    public Action? OnMainMenu { get; set; }

    public SettingsWindow(Action? onSave = null, Action? onLoad = null, Action? onMainMenu = null)
    {
        OnSave = onSave;
        OnLoad = onLoad;
        OnMainMenu = onMainMenu;
        Title = "设置";
        Size = new Vector2I(440, 420);
        Visible = false;
        Exclusive = true;
        Unresizable = true;
        CloseRequested += Hide;
        BuildUI();
    }

    private void BuildUI()
    {
        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(vbox);

        _tabs = new TabContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        StyleTabs(_tabs);
        vbox.AddChild(_tabs);

        BuildDisplayTab();
        BuildAudioTab();
        BuildGameTab();

        // Bottom bar
        vbox.AddChild(SP(4));
        var sep = new ColorRect { CustomMinimumSize = new Vector2I(0, 1), Color = C(0.25f, 0.22f, 0.35f) };
        vbox.AddChild(sep);
        vbox.AddChild(SP(4));
        var btnRow = new HBoxContainer(); vbox.AddChild(btnRow);
        var resetBtn = SmlBtn("恢复默认");
        resetBtn.Pressed += () => { GameSettings.ResetDefaults(); RefreshAll(); };
        btnRow.AddChild(resetBtn);
        btnRow.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        var closeBtn = SmlBtn("关闭");
        closeBtn.Pressed += Hide;
        btnRow.AddChild(closeBtn);
        btnRow.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
    }

    // ===== Display Tab =====
    private void BuildDisplayTab()
    {
        var tab = new VBoxContainer { Name = "画面" }; _tabs.AddChild(tab);
        tab.AddChild(SP(10));

        tab.AddChild(Section("分辨率"));
        _resDrop = new OptionButton(); _resDrop.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var p in GameSettings.Presets) _resDrop.AddItem(p.Label);
        _resDrop.Select(GameSettings.ResolutionIndex);
        _resDrop.ItemSelected += _ => { GameSettings.ResolutionIndex = (int)_resDrop.Selected; GameSettings.Save(); GameSettings.ApplyDisplay(); };
        tab.AddChild(_resDrop);
        tab.AddChild(SP(8));

        _fsToggle = ToggleRow(tab, "全屏模式", GameSettings.Fullscreen);
        _fsToggle.Toggled += (on) => { GameSettings.Fullscreen = on; GameSettings.Save(); GameSettings.ApplyDisplay(); };
        tab.AddChild(SP(4));

        _vsyncToggle = ToggleRow(tab, "垂直同步", GameSettings.VSync);
        _vsyncToggle.Toggled += (on) => { GameSettings.VSync = on; GameSettings.Save(); GameSettings.ApplyDisplay(); };
        tab.AddChild(SP(10));

        tab.AddChild(Note($"当前分辨率: {GameSettings.Presets[GameSettings.ResolutionIndex].Label}"));
    }

    // ===== Audio Tab =====
    private void BuildAudioTab()
    {
        var tab = new VBoxContainer { Name = "音频" }; _tabs.AddChild(tab);
        tab.AddChild(SP(10));

        _masterSlider = VolumeRow(tab, "主音量", GameSettings.MasterVolume, out _masterVal);
        _masterSlider.ValueChanged += (v) => { GameSettings.MasterVolume = (float)v; GameSettings.Save(); GameSettings.ApplyAudio(); _masterVal.Text = Pct(v); };
        tab.AddChild(SP(6));

        _musicSlider = VolumeRow(tab, "音乐", GameSettings.MusicVolume, out _musicVal);
        _musicSlider.ValueChanged += (v) => { GameSettings.MusicVolume = (float)v; GameSettings.Save(); GameSettings.ApplyAudio(); _musicVal.Text = Pct(v); };
        tab.AddChild(SP(6));

        _sfxSlider = VolumeRow(tab, "音效", GameSettings.SfxVolume, out _sfxVal);
        _sfxSlider.ValueChanged += (v) => { GameSettings.SfxVolume = (float)v; GameSettings.Save(); GameSettings.ApplyAudio(); _sfxVal.Text = Pct(v); };
        tab.AddChild(SP(10));

        // BGM selection
        tab.AddChild(Section("背景音乐"));
        tab.AddChild(SP(4));
        var bgmDrop = new OptionButton(); bgmDrop.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        var bgmNames = AudioManager.BgmNames;
        if (bgmNames.Count == 0)
            bgmDrop.AddItem("（无BGM文件）");
        else
            foreach (var n in bgmNames) bgmDrop.AddItem(n);
        bgmDrop.Select(Math.Min(AudioManager.CurrentBgmIndex, bgmDrop.ItemCount - 1));
        bgmDrop.ItemSelected += (idx) => AudioManager.SetBgm((int)idx);
        tab.AddChild(bgmDrop);
        tab.AddChild(SP(11));

        tab.AddChild(Note("将MP3文件放入 Resources/Audio/BGM 文件夹即可"));
    }

    // ===== Game Tab =====
    private void BuildGameTab()
    {
        var tab = new VBoxContainer { Name = "游戏" }; _tabs.AddChild(tab);
        tab.AddChild(SP(10));

        _autoSaveToggle = ToggleRow(tab, "自动存档", GameSettings.AutoSave);
        _autoSaveToggle.Toggled += (on) => { GameSettings.AutoSave = on; GameSettings.Save(); };
        tab.AddChild(SP(6));

        tab.AddChild(Section("自动存档间隔"));
        _autoSaveDrop = new OptionButton(); _autoSaveDrop.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        int[] intervals = { 3, 5, 7, 10, 15, 20, 30 };
        foreach (int d in intervals) _autoSaveDrop.AddItem($"每 {d} 天自动存档");
        int sel = Array.IndexOf(intervals, GameSettings.AutoSaveInterval);
        _autoSaveDrop.Select(sel >= 0 ? sel : 2);
        _autoSaveDrop.ItemSelected += _ => { GameSettings.AutoSaveInterval = intervals[(int)_autoSaveDrop.Selected]; GameSettings.Save(); };
        tab.AddChild(_autoSaveDrop);
        tab.AddChild(SP(14));

        // --- Save/Load/Menu buttons ---
        if (OnSave != null || OnLoad != null || OnMainMenu != null)
        {
            tab.AddChild(Section("存档管理"));
            tab.AddChild(SP(4));
        }
        if (OnSave != null) { var cc = new CenterContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; var b = BigBtn("保存游戏"); b.Pressed += () => { Hide(); OnSave?.Invoke(); }; cc.AddChild(b); tab.AddChild(cc); tab.AddChild(SP(3)); }
        if (OnLoad != null) { var cc = new CenterContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; var b = BigBtn("载入游戏"); b.Pressed += () => { Hide(); OnLoad?.Invoke(); }; cc.AddChild(b); tab.AddChild(cc); tab.AddChild(SP(3)); }
        if (OnMainMenu != null) { var cc = new CenterContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; var b = BigBtn("返回主菜单"); b.Pressed += () => OnMainMenu?.Invoke(); cc.AddChild(b); tab.AddChild(cc); tab.AddChild(SP(3)); }
        tab.AddChild(SP(6));

        tab.AddChild(Note("「剑缘修仙录」v1.0\nGodot 4.5 Mono · C#\n字体: 马善政毛笔楷书 (SIL OFL 1.1)"));
    }

    // ===== Refresh =====
    public void RefreshAll()
    {
        _resDrop.Select(GameSettings.ResolutionIndex);
        _fsToggle.ButtonPressed = GameSettings.Fullscreen;
        _vsyncToggle.ButtonPressed = GameSettings.VSync;
        _masterSlider.Value = GameSettings.MasterVolume;
        _musicSlider.Value = GameSettings.MusicVolume;
        _sfxSlider.Value = GameSettings.SfxVolume;
        _masterVal.Text = Pct(GameSettings.MasterVolume);
        _musicVal.Text = Pct(GameSettings.MusicVolume);
        _sfxVal.Text = Pct(GameSettings.SfxVolume);
        _autoSaveToggle.ButtonPressed = GameSettings.AutoSave;
    }

    // ===== Helpers =====
    private static Control SP(int h) => new Control { CustomMinimumSize = new Vector2I(0, h) };
    private static Color C(float r, float g, float b) => new(r, g, b);
    private static string Pct(double v) => $"{v * 100:F0}%";

    private static Label Section(string t)
    {
        var lb = new Label { Text = t };
        lb.AddThemeFontSizeOverride("font_size", 13);
        lb.AddThemeColorOverride("font_color", new Color(0.91f, 0.72f, 0.29f));
        return lb;
    }

    private static Label Note(string t)
    {
        var lb = new Label { Text = t, HorizontalAlignment = HorizontalAlignment.Center };
        lb.AddThemeFontSizeOverride("font_size", 11);
        lb.AddThemeColorOverride("font_color", new Color(0.45f, 0.45f, 0.55f));
        return lb;
    }

    private static CheckButton ToggleRow(Control parent, string label, bool initial)
    {
        var row = new HBoxContainer(); parent.AddChild(row);
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2I(130, 0) }
            .WithFont(13, new Color(0.88f, 0.88f, 0.92f)));
        row.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        var cb = new CheckButton { ButtonPressed = initial };
        row.AddChild(cb);
        return cb;
    }

    private static HSlider VolumeRow(Control parent, string label, float initial, out Label valLabel)
    {
        var row = new HBoxContainer(); parent.AddChild(row);
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2I(60, 0) }
            .WithFont(13, new Color(0.88f, 0.88f, 0.92f)));
        var slider = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.05, Value = initial, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        row.AddChild(slider);
        valLabel = new Label { Text = Pct(initial), CustomMinimumSize = new Vector2I(48, 0) };
        valLabel.AddThemeFontSizeOverride("font_size", 11);
        valLabel.AddThemeColorOverride("font_color", new Color(0.55f, 0.55f, 0.65f));
        row.AddChild(valLabel);
        return slider;
    }

    private static Button SmlBtn(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(90, 30) };
        b.AddThemeFontSizeOverride("font_size", 12);
        b.AddThemeColorOverride("font_color", new Color(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", new Color(0.91f, 0.72f, 0.29f));
        var sb = new StyleBoxFlat { BgColor = new Color(0.25f, 0.20f, 0.33f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
        var sh = new StyleBoxFlat { BgColor = new Color(0.33f, 0.27f, 0.42f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
        b.AddThemeStyleboxOverride("normal", sb); b.AddThemeStyleboxOverride("hover", sh);
        return b;
    }

    private static Button BigBtn(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(200, 36) };
        b.AddThemeFontSizeOverride("font_size", 14);
        b.AddThemeColorOverride("font_color", new Color(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", new Color(0.91f, 0.72f, 0.29f));
        var sb = new StyleBoxFlat { BgColor = new Color(0.22f, 0.18f, 0.30f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
        var sh = new StyleBoxFlat { BgColor = new Color(0.30f, 0.25f, 0.40f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
        b.AddThemeStyleboxOverride("normal", sb); b.AddThemeStyleboxOverride("hover", sh);
        return b;
    }

    private static void StyleTabs(TabContainer tabs)
    {
        tabs.AddThemeColorOverride("font_color", new Color(0.88f, 0.88f, 0.92f));
        tabs.AddThemeColorOverride("tab_fg", new Color(0.88f, 0.88f, 0.92f));
        tabs.AddThemeColorOverride("tab_bg", new Color(0.08f, 0.06f, 0.12f));
        tabs.AddThemeColorOverride("tab_selected", new Color(0.22f, 0.18f, 0.30f));
        tabs.AddThemeFontSizeOverride("font_size", 14);
    }
}
