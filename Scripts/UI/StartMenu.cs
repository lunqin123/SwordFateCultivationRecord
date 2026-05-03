namespace SwordFateCultivationRecord;

public partial class StartMenu : Control
{
    private Window _loadPopup = null!, _namePopup = null!;
    private SettingsWindow _settingsPopup = null!;
    private VBoxContainer _slotContainer = null!;
    private Label _errorLabel = null!;
    private LineEdit _nameInput = null!;

    public override void _Ready()
    {
        GameSettings.Load();
        UITheme.Init();
        AudioManager.Init(this);
        BuildUI();
        UITheme.ApplyTo(this);
    }

    private void BuildUI()
    {
        // Fullscreen background
        var bg = new Panel();
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        string bgPath = "res://Resources/Textures/BG/main_menu.png";
        if (ResourceLoader.Exists(bgPath))
        {
            var bgTex = ResourceLoader.Load<Texture2D>(bgPath);
            bg.AddThemeStyleboxOverride("panel", new StyleBoxTexture { Texture = bgTex });
        }
        else
        {
            bg.AddThemeStyleboxOverride("panel", UITheme.BgStyle());
        }
        AddChild(bg);

        // Gold accent line at top
        var accent = new ColorRect
        {
            AnchorLeft = 0.0f, AnchorRight = 1.0f,
            AnchorTop = 0.0f, AnchorBottom = 0.0f,
            CustomMinimumSize = new Vector2I(0, 3),
            Color = UITheme.Gold,
        };
        AddChild(accent);

        // Centered content
        var center = new Control();
        AddChild(center);
        center.AnchorLeft = 0.15f;
        center.AnchorTop = 0.03f;
        center.AnchorRight = 0.85f;
        center.AnchorBottom = 0.97f;

        // Main panel
        var panel = new Panel();
        center.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var panelBg = new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.03f, 0.10f, 0.85f),
            CornerRadiusTopLeft = 16, CornerRadiusTopRight = 16,
            CornerRadiusBottomLeft = 16, CornerRadiusBottomRight = 16,
            BorderWidthBottom = 2, BorderWidthLeft = 2,
            BorderWidthRight = 2, BorderWidthTop = 2,
            BorderColor = UITheme.GoldDark,
        };
        panel.AddThemeStyleboxOverride("panel", panelBg);

        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Top decorative element
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 30) });

        var ornamentLine = new HBoxContainer();
        var oc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ornamentLine.AddChild(oc);
        var leftOrn = new Label { Text = "― ◆ ―", HorizontalAlignment = HorizontalAlignment.Center };
        leftOrn.AddThemeFontSizeOverride("font_size", 14); leftOrn.AddThemeColorOverride("font_color", UITheme.GoldDark);
        oc.AddChild(leftOrn);
        vbox.AddChild(ornamentLine);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });

        // Sword emblem
        var art = new Label { Text = "⚔", HorizontalAlignment = HorizontalAlignment.Center };
        art.AddThemeFontSizeOverride("font_size", 44);
        art.AddThemeColorOverride("font_color", UITheme.Gold);
        vbox.AddChild(art);

        // Title
        var title = new Label { Text = "剑缘修仙录", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 52);
        title.AddThemeColorOverride("font_color", UITheme.Gold);
        vbox.AddChild(title);

        // Subtitle
        var subtitle = new Label { Text = "Sword Fate · Cultivation Record", HorizontalAlignment = HorizontalAlignment.Center };
        subtitle.AddThemeFontSizeOverride("font_size", 14);
        subtitle.AddThemeColorOverride("font_color", UITheme.TextDim);
        vbox.AddChild(subtitle);

        // Tagline
        var tagline = new Label { Text = "— 宗门模拟经营 —", HorizontalAlignment = HorizontalAlignment.Center };
        tagline.AddThemeFontSizeOverride("font_size", 12);
        tagline.AddThemeColorOverride("font_color", UITheme.TextDim);
        vbox.AddChild(tagline);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 40) });

        // Buttons with gold styling
        var newBtn = MakeMenuButton("新  游  戏");
        var loadBtn = MakeMenuButton("继 续 游 戏");
        var settingsBtn = MakeMenuButton("设  置");
        var quitBtn = MakeMenuButton("退  出");

        newBtn.Pressed += () => { AudioManager.PlayClick(); OnNewGame(); };
        loadBtn.Pressed += () => { AudioManager.PlayClick(); OnLoadGame(); };
        settingsBtn.Pressed += () => { AudioManager.PlayClick(); _settingsPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_settingsPopup.GetChild(0)); };
        quitBtn.Pressed += () => { AudioManager.PlayClick(); GetTree().Quit(); };

        foreach (var btn in new[] { newBtn, loadBtn, settingsBtn, quitBtn })
        {
            var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            cc.AddChild(btn);
            vbox.AddChild(cc);
            vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });
        }

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 30) });

        // Footer
        var footer = new Label { Text = "v1.0 · Godot 4.5 · C#", HorizontalAlignment = HorizontalAlignment.Center };
        footer.AddThemeFontSizeOverride("font_size", 10);
        footer.AddThemeColorOverride("font_color", new Color(0.3f, 0.28f, 0.38f));
        vbox.AddChild(footer);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 12) });

        // Popups
        _settingsPopup = new SettingsWindow();
        AddChild(_settingsPopup);
        BuildNamePopup();
        BuildLoadPopup();
    }

    private static Button MakeMenuButton(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(320, 52) };
        b.AddThemeFontSizeOverride("font_size", 18);
        b.AddThemeColorOverride("font_color", UITheme.TextPrimary);
        b.AddThemeColorOverride("font_hover_color", UITheme.Gold);
        b.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.12f, 0.08f, 0.16f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.GoldDark });
        b.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = new Color(0.20f, 0.14f, 0.26f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.Gold });
        b.AddThemeStyleboxOverride("pressed", new StyleBoxFlat { BgColor = new Color(0.08f, 0.05f, 0.10f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.GoldDark });
        return b;
    }

    private void BuildNamePopup()
    {
        _namePopup = new Window { Title = "开宗立派", Size = new Vector2I(440, 370), Visible = false, Exclusive = true, Unresizable = true };
        _namePopup.CloseRequested += () => _namePopup.Hide();
        AddChild(_namePopup);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _namePopup.AddChild(vbox);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 12) });

        var title = new Label { Text = "为你的宗门命名", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.AddThemeColorOverride("font_color", UITheme.Gold);
        vbox.AddChild(title);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 10) });

        _nameInput = new LineEdit { Text = "无名剑宗", CustomMinimumSize = new Vector2I(300, 38) };
        _nameInput.AddThemeFontSizeOverride("font_size", 16);
        _nameInput.AddThemeColorOverride("font_color", UITheme.Gold);
        var inputCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        inputCc.AddChild(_nameInput);
        vbox.AddChild(inputCc);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 12) });

        var presetsLabel = new Label { Text = "— 预设名称 —", HorizontalAlignment = HorizontalAlignment.Center };
        presetsLabel.AddThemeFontSizeOverride("font_size", 12);
        presetsLabel.AddThemeColorOverride("font_color", UITheme.TextDim);
        vbox.AddChild(presetsLabel);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 6) });

        string[] presets = { "无名剑宗", "太虚剑派", "青云仙门", "紫霄剑阁", "天道玄宗", "万象仙府", "碧落剑庐" };
        var grid = new GridContainer { Columns = 3 };
        foreach (var p in presets)
        {
            var btn = UIColumns.MakeSmallButton(p);
            btn.CustomMinimumSize = new Vector2I(120, 30);
            var captured = p;
            btn.Pressed += () => _nameInput.Text = captured;
            grid.AddChild(btn);
        }
        var gridCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        gridCc.AddChild(grid);
        vbox.AddChild(gridCc);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 16) });

        var confirmBtn = MakeMenuButton("开 始 游 戏");
        confirmBtn.CustomMinimumSize = new Vector2I(240, 44);
        confirmBtn.AddThemeFontSizeOverride("font_size", 18);
        var btnCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        btnCc.AddChild(confirmBtn);
        vbox.AddChild(btnCc);
        confirmBtn.Pressed += StartNewGame;
    }

    private void BuildLoadPopup()
    {
        _loadPopup = new Window { Title = "择卷", Size = new Vector2I(540, 420), Visible = false, Exclusive = true };
        _loadPopup.CloseRequested += () => _loadPopup.Hide();
        AddChild(_loadPopup);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _loadPopup.AddChild(vbox);

        _slotContainer = new VBoxContainer();
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        scroll.AddChild(_slotContainer);
        vbox.AddChild(scroll);

        _errorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _errorLabel.AddThemeFontSizeOverride("font_size", 13);
        _errorLabel.AddThemeColorOverride("font_color", UITheme.Crimson);
        vbox.AddChild(_errorLabel);

        var closeBtn = UIColumns.MakeButton("归去", "");
        vbox.AddChild(closeBtn);
        closeBtn.Pressed += () => _loadPopup.Hide();
    }

    private void OnNewGame()
    {
        _namePopup.PopupCentered(); UIAnimator.WindowOpen((Control)_namePopup.GetChild(0));
        _nameInput.Text = "无名剑宗";
        _nameInput.GrabFocus();
    }

    private void StartNewGame()
    {
        _namePopup.Hide();
        string name = _nameInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) name = "无名剑宗";
        if (name.Length > 12) name = name[..12];

        GameManager.Instance.InitializeNewGame();
        GameManager.Instance.SectName = name;
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    private void OnLoadGame()
    {
        RefreshSaveSlots();
        _loadPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_loadPopup.GetChild(0));
    }

    private void RefreshSaveSlots()
    {
        _slotContainer.FreeChildren();
        _errorLabel.Text = "";

        var slots = GameManager.Instance.SaveLoad.GetOccupiedSlots();
        if (slots.Length == 0)
        {
            _slotContainer.AddChild(new Label { Text = "  没有找到存档。", SizeFlagsHorizontal = SizeFlags.ExpandFill }.WithFont(13, UITheme.TextDim));
            return;
        }

        foreach (int slotIdx in slots.Order())
        {
            var data = GameManager.Instance.SaveLoad.LoadFromSlot(slotIdx);
            if (data == null) continue;

            var frame = new MarginContainer();
            var hbox = new HBoxContainer();
            frame.AddChild(hbox);

            var info = new Label
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"存档位 {slotIdx + 1}  |  第{data.CurrentYear}年  |  {data.SectName}  |  弟子{data.Disciples.Count}人  |  灵石{data.Resources.GetValueOrDefault(ResourceType.SpiritStone)}",
            };
            info.AddThemeFontSizeOverride("font_size", 13);
            info.AddThemeColorOverride("font_color", UITheme.TextPrimary);
            hbox.AddChild(info);

            var loadBtn = UIColumns.MakeSmallButton("开卷");
            int captured = slotIdx;
            loadBtn.Pressed += () => OnLoadSlot(captured);
            hbox.AddChild(loadBtn);

            var delBtn = UIColumns.MakeSmallButton("删除");
            int capturedDel = slotIdx;
            delBtn.Pressed += () => { GameManager.Instance.SaveLoad.DeleteSlot(capturedDel); RefreshSaveSlots(); };
            hbox.AddChild(delBtn);

            _slotContainer.AddChild(frame);
        }
    }

    private void OnLoadSlot(int slot)
    {
        if (!GameManager.Instance.LoadGame(slot))
        {
            _errorLabel.Text = "开卷无果，存档可能已损坏。";
            return;
        }
        _loadPopup.Hide();
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
