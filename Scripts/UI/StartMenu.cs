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
        // Fullscreen dark background
        var bg = new Panel();
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        bg.AddThemeStyleboxOverride("panel", UITheme.BgStyle());
        AddChild(bg);

        // Subtle top accent line
        var accent = new ColorRect
        {
            AnchorLeft = 0.0f, AnchorRight = 1.0f,
            AnchorTop = 0.0f, AnchorBottom = 0.0f,
            CustomMinimumSize = new Vector2I(0, 3),
            Color = UIColums.Gold,
        };
        AddChild(accent);

        // Centered content area (matches MainUI: 10% margins each side)
        var center = new Control();
        AddChild(center);
        center.AnchorLeft = 0.10f;
        center.AnchorTop = 0.005f;
        center.AnchorRight = 0.90f;
        center.AnchorBottom = 0.995f;

        // Main menu panel fills the center area
        var panel = new Panel();
        center.AddChild(panel);
        panel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        panel.AddThemeStyleboxOverride("panel", UITheme.PanelStyle());

        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Top padding
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 40) });

        // Sword decoration (text art)
        var art = new Label
        {
            Text = "⚔",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        art.AddThemeFontSizeOverride("font_size", 40);
        art.AddThemeColorOverride("font_color", UIColums.Gold);
        vbox.AddChild(art);

        // Title
        var title = new Label
        {
            Text = "剑缘修仙录",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        title.AddThemeFontSizeOverride("font_size", 52);
        title.AddThemeColorOverride("font_color", UIColums.Gold);
        title.AddThemeColorOverride("font_outline_color", UIColums.DarkBg);
        vbox.AddChild(title);

        // Subtitle
        var subtitle = new Label
        {
            Text = "Sword Fate · Cultivation Record",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        subtitle.AddThemeFontSizeOverride("font_size", 16);
        subtitle.AddThemeColorOverride("font_color", UIColums.TextDim);
        vbox.AddChild(subtitle);

        // Tagline
        var tagline = new Label
        {
            Text = "— 宗门模拟经营 —",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        tagline.AddThemeFontSizeOverride("font_size", 13);
        tagline.AddThemeColorOverride("font_color", UIColums.TextDim);
        vbox.AddChild(tagline);

        // Spacer
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 50) });

        // === Buttons ===
        var newBtn = UIColums.MakeButton("新  游  戏", "开始一段新的修仙宗门之旅");
        var loadBtn = UIColums.MakeButton("继 续 游 戏", "读取已有的存档");
        var settingsBtn = UIColums.MakeButton("设  置", "调整游戏设置");
        var quitBtn = UIColums.MakeButton("退  出", "离开游戏");

        foreach (var btn in new[] { newBtn, loadBtn, settingsBtn, quitBtn })
        {
            btn.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            btn.CustomMinimumSize = new Vector2I(300, 48);
            btn.AddThemeFontSizeOverride("font_size", 18);
        }

        vbox.AddChild(newBtn);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });
        vbox.AddChild(loadBtn);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });
        vbox.AddChild(settingsBtn);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });
        vbox.AddChild(quitBtn);

        newBtn.Pressed += () => { AudioManager.PlayClick(); OnNewGame(); };
        loadBtn.Pressed += () => { AudioManager.PlayClick(); OnLoadGame(); };
        settingsBtn.Pressed += () => { AudioManager.PlayClick(); _settingsPopup.PopupCentered(); };
        quitBtn.Pressed += () => { AudioManager.PlayClick(); GetTree().Quit(); };

        // Bottom padding
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 50) });

        // Footer
        var footer = new Label
        {
            Text = "v1.0  ·  Godot 4.5  ·  C#",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        footer.AddThemeFontSizeOverride("font_size", 10);
        footer.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.4f));
        vbox.AddChild(footer);

        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 20) });

        // === Settings Popup ===
        _settingsPopup = new SettingsWindow(); // no game actions on main menu
        AddChild(_settingsPopup);

        // === Name Popup ===
        BuildNamePopup();

        // === Load Popup ===
        BuildLoadPopup();
    }

    private void BuildNamePopup()
    {
        _namePopup = new Window
        {
            Title = "创建宗门",
            Size = new Vector2I(440, 370),
            Visible = false,
            Exclusive = true,
            Unresizable = true,
        };
        _namePopup.CloseRequested += () => _namePopup.Hide();
        AddChild(_namePopup);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _namePopup.AddChild(vbox);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 12) });

        var title = new Label { Text = "为你的宗门命名", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.AddThemeColorOverride("font_color", UIColums.Gold);
        vbox.AddChild(title);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 10) });

        // Text input
        _nameInput = new LineEdit { Text = "无名剑宗", CustomMinimumSize = new Vector2I(0, 36) };
        _nameInput.AddThemeFontSizeOverride("font_size", 16);
        _nameInput.AddThemeColorOverride("font_color", UIColums.Gold);
        var inputCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        inputCc.AddChild(_nameInput);
        vbox.AddChild(inputCc);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 12) });

        // Presets
        var presetsLabel = new Label { Text = "— 预设名称 —", HorizontalAlignment = HorizontalAlignment.Center };
        presetsLabel.AddThemeFontSizeOverride("font_size", 12);
        presetsLabel.AddThemeColorOverride("font_color", UIColums.TextDim);
        vbox.AddChild(presetsLabel);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 6) });

        string[] presets = { "无名剑宗", "太虚剑派", "青云仙门", "紫霄剑阁", "天道玄宗", "万象仙府", "碧落剑庐" };
        var grid = new GridContainer { Columns = 3 };
        foreach (var p in presets)
        {
            var btn = UIColums.MakeSmallButton(p);
            btn.CustomMinimumSize = new Vector2I(120, 30);
            var captured = p;
            btn.Pressed += () => _nameInput.Text = captured;
            grid.AddChild(btn);
        }
        var gridCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        gridCc.AddChild(grid);
        vbox.AddChild(gridCc);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 16) });

        // Confirm button
        var confirmBtn = UIColums.MakeButton("开 始 游 戏", "创建宗门，踏上修仙之旅");
        confirmBtn.CustomMinimumSize = new Vector2I(240, 44);
        confirmBtn.AddThemeFontSizeOverride("font_size", 18);
        var btnCc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        btnCc.AddChild(confirmBtn);
        vbox.AddChild(btnCc);
        confirmBtn.Pressed += StartNewGame;
    }

    private void BuildLoadPopup()
    {
        _loadPopup = new Window
        {
            Title = "选择存档",
            Size = new Vector2I(540, 420),
            Visible = false,
            Exclusive = true,
        };
        _loadPopup.CloseRequested += () => _loadPopup.Hide();
        AddChild(_loadPopup);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _loadPopup.AddChild(vbox);

        _slotContainer = new VBoxContainer();
        var scroll = new ScrollContainer();
        scroll.AddChild(_slotContainer);
        vbox.AddChild(scroll);

        _errorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _errorLabel.AddThemeFontSizeOverride("font_size", 13);
        _errorLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
        vbox.AddChild(_errorLabel);

        var closeBtn = UIColums.MakeButton("返回", "");
        vbox.AddChild(closeBtn);
        closeBtn.Pressed += () => _loadPopup.Hide();
    }

    private void OnNewGame()
    {
        _namePopup.PopupCentered();
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
        _loadPopup.PopupCentered();
    }

    private void RefreshSaveSlots()
    {
        _slotContainer.FreeChildren();
        _errorLabel.Text = "";

        var slots = GameManager.Instance.SaveLoad.GetOccupiedSlots();
        if (slots.Length == 0)
        {
            _slotContainer.AddChild(new Label
            {
                Text = "  没有找到存档。",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            });
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
                Text = $"存档位 {slotIdx + 1}  |  第{data.CurrentYear}年  |  " +
                       $"{data.SectName}  |  弟子{data.Disciples.Count}人  |  " +
                       $"灵石{data.Resources.GetValueOrDefault(ResourceType.SpiritStone)}",
            };
            info.AddThemeFontSizeOverride("font_size", 13);
            hbox.AddChild(info);

            var loadBtn = UIColums.MakeSmallButton("读档");
            int captured = slotIdx;
            loadBtn.Pressed += () => OnLoadSlot(captured);
            hbox.AddChild(loadBtn);

            var delBtn = UIColums.MakeSmallButton("删除");
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
            _errorLabel.Text = "读档失败，存档可能已损坏。";
            return;
        }
        _loadPopup.Hide();
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
