namespace SwordFateCultivationRecord;

public partial class MainUI : Control
{
    private GameManager GM => GameManager.Instance;
    private static readonly string[] TaskNames = { "修炼", "训练", "采集", "炼丹", "炼器", "授课", "守卫", "探索", "休息" };

    // Top bar
    private Label _timeLabel = null!, _sectLabel = null!;
    private Button _nextDayBtn = null!, _ff7Btn = null!, _ff30Btn = null!;
    private readonly Dictionary<ResourceType, Label> _resourceLabels = new();

    // Tabs
    private TabContainer _tabs = null!;
    private VBoxContainer _discipleList = null!, _facilityList = null!;
    private ItemList _logList = null!;

    // Event popup
    private Window _eventPopup = null!, _savePopup = null!, _gameOverPopup = null!, _detailPopup = null!, _hintPopup = null!;
    private Label _hintLabel = null!;
    private VBoxContainer _slotContainer = null!;
    private Label _saveErrorLabel = null!;
    private Window _loadPopup = null!;
    private VBoxContainer _loadSlotContainer = null!;
    private Label _loadErrorLabel = null!;
    private Label _eventTitle = null!, _eventDesc = null!;
    private Button _eventChoice1 = null!, _eventChoice2 = null!, _eventChoice3 = null!;
    private Label _eventEffect1 = null!, _eventEffect2 = null!, _eventEffect3 = null!;
    private Button _dismissedBtn = null!;
    // Detail popup controls
    private VBoxContainer _detailContent = null!;
    private Label _detailName = null!, _detailRealm = null!, _detailAge = null!;
    private Label _detailStats = null!, _detailCombat = null!, _detailTask = null!;
    private Label _detailProgress = null!, _detailMood = null!, _detailStamina = null!;
    private Label _detailSkills = null!, _detailEquip = null!;
    private Label _detailAvatarLabel = null!;
    private PanelContainer _detailAvatarPanel = null!;
    private VBoxContainer _equipSection = null!;
    private Button _bgmIndicator = null!;
    private Window _bgmSelectPopup = null!;
    // Companion tab
    private VBoxContainer _companionList = null!, _matchSection = null!, _questList = null!;
    private OptionButton _matchMaleDrop = null!, _matchFemaleDrop = null!;
    private SettingsWindow _settingsPopup = null!;
    private VBoxContainer _statsContent = null!, _overviewDashboard = null!;


    public override void _Ready()
    {
        UITheme.Init();
        AudioManager.Init(this);
        AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;
        OffsetLeft = 0; OffsetTop = 0; OffsetRight = 0; OffsetBottom = 0;
        BuildUI(); ConnectSignals();
        UITheme.ApplyTo(this);
        if (GM.IsInitialized) RefreshAll();
    }


    // ===================== BUILD =====================

    private void BuildUI()
    {
        // Full dark background
        var bgPanel = new Panel(); bgPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        bgPanel.AddThemeStyleboxOverride("panel", UITheme.BgStyle());
        AddChild(bgPanel);

        // Centered content area (10% margins each side ≈ 80% width center)
        var center = new Control();
        AddChild(center);
        center.AnchorLeft = 0.10f;
        center.AnchorTop = 0.005f;
        center.AnchorRight = 0.90f;
        center.AnchorBottom = 0.995f;

        // Window panel with rounded corners
        var window = new Panel();
        center.AddChild(window);
        window.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        window.AddThemeStyleboxOverride("panel", UITheme.PanelStyle());

        // VBox fills the window
        var vbox = new VBoxContainer();
        window.AddChild(vbox);
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // TopBar
        var bar = new Panel { CustomMinimumSize = new Vector2I(0, 80) };
        bar.AddThemeStyleboxOverride("panel", UITheme.PanelStyle());
        vbox.AddChild(bar);
        var hbox = new HBoxContainer(); bar.AddChild(hbox);
        hbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var lv = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        hbox.AddChild(lv);
        _sectLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; _sectLabel.AddThemeFontSizeOverride("font_size", 17); _sectLabel.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f)); lv.AddChild(_sectLabel);
        _timeLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; _timeLabel.AddThemeFontSizeOverride("font_size", 13); _timeLabel.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f)); lv.AddChild(_timeLabel);

        var cv = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        hbox.AddChild(cv);
        var cl = new Label { Text = "— 推进时间 —", HorizontalAlignment = HorizontalAlignment.Center };
        cl.AddThemeFontSizeOverride("font_size", 11); cl.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f)); cv.AddChild(cl);
        var cr = new HBoxContainer(); cv.AddChild(cr);
        _nextDayBtn = Btn("下一天"); _ff7Btn = Btn("7天"); _ff30Btn = Btn("30天");
        cr.AddChild(_nextDayBtn); cr.AddChild(_ff7Btn); cr.AddChild(_ff30Btn);
        _nextDayBtn.Pressed += () => { GM.NextDay(); RefreshAll(); };
        _ff7Btn.Pressed += () => { GM.FastForward(7); RefreshAll(); };
        _ff30Btn.Pressed += () => { GM.FastForward(30); RefreshAll(); };
        // Recruit button in top bar
        var recruitTopBtn = SmallBtn("招募弟子"); cv.AddChild(recruitTopBtn);
        recruitTopBtn.Pressed += () => GM.RecruitDisciple();

        var rg = new GridContainer { Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        hbox.AddChild(rg);
        foreach (ResourceType rt in Enum.GetValues<ResourceType>())
        {
            var lb = new Label(); lb.AddThemeFontSizeOverride("font_size", 12); _resourceLabels[rt] = lb; rg.AddChild(lb);
        }

        // Settings gear button
        var settingsBtn = new Button { Text = "⚙", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(36, 36) };
        settingsBtn.AddThemeFontSizeOverride("font_size", 18);
        settingsBtn.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        settingsBtn.AddThemeColorOverride("font_hover_color", C(0.91f, 0.72f, 0.29f));
        settingsBtn.Flat = true;
        settingsBtn.Pressed += () => { AudioManager.PlayClick(); _settingsPopup.PopupCentered(); };
        hbox.AddChild(settingsBtn);

        // Tabs
        _tabs = new TabContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _tabs.AddThemeColorOverride("tab_fg", C(0.88f, 0.88f, 0.92f));
        _tabs.AddThemeColorOverride("tab_bg", C(0.08f, 0.06f, 0.12f));
        _tabs.AddThemeColorOverride("tab_selected", C(0.22f, 0.18f, 0.30f));
        vbox.AddChild(_tabs);

        // 概况 (rich dashboard)
        var ov = new VBoxContainer { Name = "概况" }; _tabs.AddChild(ov);
        var os = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; ov.AddChild(os);
        var oc = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; os.AddChild(oc);
        oc.AddChild(SP(10));
        oc.AddChild(HL("宗门概况", 22, C(0.91f, 0.72f, 0.29f)));
        oc.AddChild(SP(10));
        // Keep overviewStats for dynamic refresh
        _overviewDashboard = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        oc.AddChild(_overviewDashboard);
        oc.AddChild(SP(12));

        // 弟子
        var dt = new VBoxContainer { Name = "弟子" }; _tabs.AddChild(dt);
        var ds = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; dt.AddChild(ds); _discipleList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ds.AddChild(_discipleList);

        // 建造
        var buildTab = new VBoxContainer { Name = "建造" }; _tabs.AddChild(buildTab);
        var buildScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; buildTab.AddChild(buildScroll);
        var buildContent = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; buildScroll.AddChild(buildContent);
        buildContent.AddChild(HL("建造设施（1天）", 18, C(0.91f, 0.72f, 0.29f)));
        buildContent.AddChild(SP(8));
        // Build buttons — sorted: unlocked first
        var facilities = Enum.GetValues<FacilityType>()
            .Select(ft => (type: ft, info: FacilityTable.GetInfo(ft)))
            .OrderBy(f => f.info.MinSectLevel > GM.SectLevel ? 1 : 0) // unlocked first
            .ToList();
        var buildGrid = new GridContainer { Columns = 4 }; buildContent.AddChild(buildGrid);
        foreach (var (ft, info) in facilities)
        {
            bool locked = GM.SectLevel < info.MinSectLevel;
            string btnText = locked
                ? $"{info.Name}\n🔒Lv.{info.MinSectLevel}"
                : $"{info.Name}\n{info.BaseBuildCost}灵石\n{info.BuildDays}天";
            var btn = Btn(btnText); var f2 = ft;
            btn.Disabled = locked;
            if (!locked) btn.Pressed += () => GM.StartBuild(f2);
            buildGrid.AddChild(btn);
        }
        buildContent.AddChild(SP(8));
        buildContent.AddChild(TB($"共有{facilities.Count(f => !(GM.SectLevel < f.info.MinSectLevel))}种设施可建造"));

        // 设施
        var ft2 = new VBoxContainer { Name = "设施" }; _tabs.AddChild(ft2);
        var fs = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; ft2.AddChild(fs); _facilityList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; fs.AddChild(_facilityList);

        // 道侣
        var compTab = new VBoxContainer { Name = "道侣" }; _tabs.AddChild(compTab);
        var compScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; compTab.AddChild(compScroll);
        _companionList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; compScroll.AddChild(_companionList);

        // 任务
        var questTab = new VBoxContainer { Name = "任务" }; _tabs.AddChild(questTab);
        var questScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; questTab.AddChild(questScroll);
        _questList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; questScroll.AddChild(_questList);

        // 日志
        var lt = new VBoxContainer { Name = "日志" }; _tabs.AddChild(lt);
        _logList = new ItemList { SizeFlagsVertical = SizeFlags.ExpandFill }; lt.AddChild(_logList);

        // 统计
        var statTab = new VBoxContainer { Name = "统计" }; _tabs.AddChild(statTab);
        var statScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; statTab.AddChild(statScroll);
        _statsContent = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; statScroll.AddChild(_statsContent);

        // Event popup
        _eventPopup = new Window { Title = "事件", Size = new Vector2I(600, 520), Visible = false, Exclusive = true };
        _eventPopup.CloseRequested += () => { _eventPopup.Hide(); GM.DismissEvent(); RefreshAll(); };
        AddChild(_eventPopup);
        var ev = new VBoxContainer(); ev.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _eventPopup.AddChild(ev);
        _eventTitle = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _eventTitle.AddThemeFontSizeOverride("font_size", 22); _eventTitle.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f)); ev.AddChild(_eventTitle);
        _eventDesc = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        _eventDesc.AddThemeFontSizeOverride("font_size", 14); _eventDesc.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f)); ev.AddChild(_eventDesc);
        ev.AddChild(SP(8));
        _eventChoice1 = Btn(""); ev.AddChild(_eventChoice1);
        _eventEffect1 = EL(); ev.AddChild(_eventEffect1);
        _eventChoice2 = Btn(""); ev.AddChild(_eventChoice2);
        _eventEffect2 = EL(); ev.AddChild(_eventEffect2);
        _eventChoice3 = Btn(""); ev.AddChild(_eventChoice3);
        _eventEffect3 = EL(); ev.AddChild(_eventEffect3);
        _dismissedBtn = Btn("关闭"); ev.AddChild(_dismissedBtn); _dismissedBtn.Hide();
        _dismissedBtn.Pressed += DismissEventPopup;
        _eventChoice1.Pressed += () => ResolveChoice(0);
        _eventChoice2.Pressed += () => ResolveChoice(1);
        _eventChoice3.Pressed += () => ResolveChoice(2);

        // Save popup
        _savePopup = new Window { Title = "保存游戏", Size = new Vector2I(540, 420), Visible = false, Exclusive = true };
        _savePopup.CloseRequested += () => _savePopup.Hide();
        AddChild(_savePopup);
        var sv = new VBoxContainer(); sv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _savePopup.AddChild(sv);
        _slotContainer = new VBoxContainer();
        var sc = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; sc.AddChild(_slotContainer); sv.AddChild(sc);
        _saveErrorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _saveErrorLabel.AddThemeFontSizeOverride("font_size", 13);
        _saveErrorLabel.AddThemeColorOverride("font_color", C(1, 0.3f, 0.3f));
        sv.AddChild(_saveErrorLabel);
        var closeSv = Btn("返回"); sv.AddChild(closeSv);
        closeSv.Pressed += () => _savePopup.Hide();

        // Game over popup
        _gameOverPopup = new Window { Title = "宗门覆灭", Size = new Vector2I(400, 250), Visible = false, Exclusive = true };
        _gameOverPopup.CloseRequested += () => _gameOverPopup.Hide();
        AddChild(_gameOverPopup);
        var gv = new VBoxContainer(); gv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _gameOverPopup.AddChild(gv);
        var gt = new Label { Text = "宗门覆灭", HorizontalAlignment = HorizontalAlignment.Center };
        gt.AddThemeFontSizeOverride("font_size", 24); gt.AddThemeColorOverride("font_color", C(1, 0.3f, 0.3f)); gv.AddChild(gt);
        gv.AddChild(SP(8));
        var gd = new Label { Text = "所有弟子都已离去，宗门名存实亡...\n一个修仙门派就此陨落。", HorizontalAlignment = HorizontalAlignment.Center };
        gd.AddThemeFontSizeOverride("font_size", 14); gd.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f)); gv.AddChild(gd);
        gv.AddChild(SP(16));
        var gr = Btn("重新开始"); var grc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grc.AddChild(gr); gv.AddChild(grc);
        gr.Pressed += () => { _gameOverPopup.Hide(); GM.InitializeNewGame(); RefreshAll(); };
        var gm2 = Btn("返回主菜单"); var gm2c = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        gm2c.AddChild(gm2); gv.AddChild(gm2c);
        gm2.Pressed += () => { GetTree().ChangeSceneToFile("res://Scenes/StartMenu.tscn"); };

        // Load popup
        _loadPopup = new Window { Title = "载入游戏", Size = new Vector2I(540, 420), Visible = false, Exclusive = true };
        _loadPopup.CloseRequested += () => _loadPopup.Hide();
        AddChild(_loadPopup);
        var lvWin = new VBoxContainer(); lvWin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _loadPopup.AddChild(lvWin);
        _loadSlotContainer = new VBoxContainer();
        var lsc = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; lsc.AddChild(_loadSlotContainer); lvWin.AddChild(lsc);
        _loadErrorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _loadErrorLabel.AddThemeFontSizeOverride("font_size", 13);
        _loadErrorLabel.AddThemeColorOverride("font_color", C(1, 0.3f, 0.3f));
        lvWin.AddChild(_loadErrorLabel);
        var closeLv = Btn("返回"); lvWin.AddChild(closeLv);
        closeLv.Pressed += () => _loadPopup.Hide();

        // Hint popup (for system messages like "设施已达上限")
        _hintPopup = new Window { Title = "提示", Size = new Vector2I(360, 160), Visible = false, Exclusive = true, Unresizable = true };
        _hintPopup.CloseRequested += () => _hintPopup.Hide();
        AddChild(_hintPopup);
        var hv = new VBoxContainer(); hv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _hintPopup.AddChild(hv);
        _hintLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, SizeFlagsVertical = SizeFlags.ExpandFill };
        _hintLabel.AddThemeFontSizeOverride("font_size", 14); _hintLabel.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        hv.AddChild(_hintLabel);
        var hb = Btn("确定"); var hc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        hc.AddChild(hb); hv.AddChild(hc);
        hb.Pressed += () => _hintPopup.Hide();

        // Disciple detail popup
        _detailPopup = new Window { Title = "弟子详情", Size = new Vector2I(480, 420), Visible = false, Exclusive = true };
        _detailPopup.CloseRequested += () => _detailPopup.Hide();
        AddChild(_detailPopup);
        var dv = new VBoxContainer(); dv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _detailPopup.AddChild(dv);
        _detailContent = new VBoxContainer();
        var sc2 = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; sc2.AddChild(_detailContent); dv.AddChild(sc2);

        // Avatar in detail
        _detailAvatarPanel = new PanelContainer { CustomMinimumSize = new Vector2I(64, 64) };
        var daBg = new StyleBoxFlat { BgColor = C(0.18f, 0.15f, 0.28f), CornerRadiusBottomLeft = 32, CornerRadiusBottomRight = 32, CornerRadiusTopLeft = 32, CornerRadiusTopRight = 32 };
        _detailAvatarPanel.AddThemeStyleboxOverride("panel", daBg);
        _detailAvatarLabel = new Label { Text = "?", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _detailAvatarLabel.AddThemeFontSizeOverride("font_size", 28);
        _detailAvatarLabel.AddThemeColorOverride("font_color", C(0.7f, 0.7f, 0.8f));
        _detailAvatarPanel.AddChild(_detailAvatarLabel);
        var daCenter = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        daCenter.AddChild(_detailAvatarPanel);
        _detailContent.AddChild(daCenter);
        _detailContent.AddChild(SP(4));

        _detailName = new Label();
        _detailName.AddThemeFontSizeOverride("font_size", 20); _detailName.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f));
        _detailContent.AddChild(_detailName);

        _detailRealm = new Label();
        _detailRealm.AddThemeFontSizeOverride("font_size", 14); _detailRealm.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _detailContent.AddChild(_detailRealm);

        _detailAge = new Label();
        _detailAge.AddThemeFontSizeOverride("font_size", 12); _detailAge.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f));
        _detailContent.AddChild(_detailAge);

        _detailContent.AddChild(SP(4));

        _detailStats = new Label();
        _detailStats.AddThemeFontSizeOverride("font_size", 13); _detailStats.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _detailContent.AddChild(_detailStats);

        _detailCombat = new Label();
        _detailCombat.AddThemeFontSizeOverride("font_size", 13); _detailCombat.AddThemeColorOverride("font_color", C(1, 0.7f, 0.3f));
        _detailContent.AddChild(_detailCombat);

        _detailTask = new Label();
        _detailTask.AddThemeFontSizeOverride("font_size", 13); _detailTask.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _detailContent.AddChild(_detailTask);

        _detailProgress = new Label();
        _detailProgress.AddThemeFontSizeOverride("font_size", 13); _detailProgress.AddThemeColorOverride("font_color", C(0.3f, 1, 0.3f));
        _detailContent.AddChild(_detailProgress);

        _detailMood = new Label();
        _detailMood.AddThemeFontSizeOverride("font_size", 13); _detailMood.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _detailContent.AddChild(_detailMood);

        _detailStamina = new Label();
        _detailStamina.AddThemeFontSizeOverride("font_size", 13); _detailStamina.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _detailContent.AddChild(_detailStamina);

        _detailSkills = new Label();
        _detailSkills.AddThemeFontSizeOverride("font_size", 12); _detailSkills.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f));
        _detailContent.AddChild(_detailSkills);

        _detailEquip = new Label();
        _detailEquip.AddThemeFontSizeOverride("font_size", 12); _detailEquip.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f));
        _detailContent.AddChild(_detailEquip);

        _equipSection = new VBoxContainer();
        _detailContent.AddChild(_equipSection);

        dv.AddChild(SP(8));
        var closeDetail = Btn("关闭"); dv.AddChild(closeDetail);
        closeDetail.Pressed += () => _detailPopup.Hide();

        // Settings popup
        _settingsPopup = new SettingsWindow(
            onSave: () => { RefreshSaveSlots(); _savePopup.PopupCentered(); },
            onLoad: () => { RefreshLoadSlots(); _loadPopup.PopupCentered(); },
            onMainMenu: () => GetTree().ChangeSceneToFile("res://Scenes/StartMenu.tscn")
        );
        AddChild(_settingsPopup);

        // BGM indicator (bottom-right corner)
        _bgmIndicator = new Button
        {
            Text = "♪",
            Alignment = HorizontalAlignment.Center,
            AnchorLeft = 1.0f, AnchorTop = 1.0f,
            AnchorRight = 1.0f, AnchorBottom = 1.0f,
            OffsetLeft = -180, OffsetTop = -28, OffsetRight = -8, OffsetBottom = -4,
        };
        _bgmIndicator.AddThemeFontSizeOverride("font_size", 11);
        _bgmIndicator.AddThemeColorOverride("font_color", C(0.5f, 0.7f, 0.5f));
        _bgmIndicator.AddThemeColorOverride("font_hover_color", C(0.7f, 1.0f, 0.7f));
        _bgmIndicator.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = C(0.08f, 0.06f, 0.12f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 });
        _bgmIndicator.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = C(0.15f, 0.12f, 0.22f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 });
        _bgmIndicator.Flat = true;
        _bgmIndicator.Pressed += () =>
        {
            RefreshBgmPopup();
            _bgmSelectPopup.PopupCentered();
        };
        AddChild(_bgmIndicator);

        // BGM select popup
        _bgmSelectPopup = new Window { Title = "选择背景音乐", Size = new Vector2I(280, 260), Visible = false, Exclusive = true, Unresizable = true };
        _bgmSelectPopup.CloseRequested += () => _bgmSelectPopup.Hide();
        AddChild(_bgmSelectPopup);
        var bgmV = new VBoxContainer(); bgmV.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _bgmSelectPopup.AddChild(bgmV);
        // Content filled by RefreshBgmPopup
    }


    // ===================== SIGNALS =====================

    void ConnectSignals()
    {
        EventBus.DayPassed += (_, _, _) => RefreshAll();
        EventBus.ResourceChanged += (_, _, _) => RefreshResources();
        EventBus.EventChoiceRequired += OnEventTriggered;
        EventBus.GameNotification += (t, m) => {
            AddLog(m);
            if (t == "提示") { _hintLabel.Text = m; _hintPopup.PopupCentered(); }
            if (t == "宗门覆灭") _gameOverPopup.PopupCentered();
        };
        EventBus.DiscipleRecruited += _ => RefreshDisciples();
        EventBus.DiscipleDeparted += _ => RefreshDisciples();
    }

    // ===================== REFRESH =====================

    void RefreshAll() { if (!IsInsideTree()) return; RefreshTime(); RefreshResources(); RefreshSectInfo(); RefreshDisciples(); RefreshFacilities(); RefreshCompanions(); RefreshQuests(); RefreshLog(); RefreshBgmIndicator(); RefreshButtons(); RefreshOverviewStats(); }
    void RefreshTime() => _timeLabel.Text = GM.Time.GetDateString();
    void RefreshSectInfo() => _sectLabel.Text = $"{GM.FullSectName} Lv.{GM.SectLevel}  |  声望{GM.SectReputation} 战力{GM.SectPower} 弟子{GM.Disciples.Count}/{GM.MaxDisciples}人";
    void RefreshButtons() { bool b = GM.PendingEvent != null; _nextDayBtn.Disabled = b; _ff7Btn.Disabled = b; _ff30Btn.Disabled = b; }

    void RefreshOverviewStats()
    {
        _overviewDashboard.FreeChildren();

        // Sect info cards
        var infoGrid = new GridContainer { Columns = 4 };
        _overviewDashboard.AddChild(infoGrid);
        InfoCard(infoGrid, "宗门等级", $"Lv.{GM.SectLevel}", C(0.91f, 0.72f, 0.29f));
        InfoCard(infoGrid, "宗门称号", GM.SectTitle, C(0.91f, 0.72f, 0.29f));
        InfoCard(infoGrid, "声望", GM.SectReputation.ToString(), C(0.5f, 0.8f, 1.0f));
        InfoCard(infoGrid, "战力", GM.SectPower.ToString(), C(1.0f, 0.6f, 0.3f));
        _overviewDashboard.AddChild(SP(10));

        // Disciple & facility summary
        var summaryGrid = new GridContainer { Columns = 4 };
        _overviewDashboard.AddChild(summaryGrid);
        InfoCard(summaryGrid, "弟子", $"{GM.Disciples.Count}/{GM.MaxDisciples}", C(0.3f, 1.0f, 0.3f));
        InfoCard(summaryGrid, "设施", $"{GM.Facilities.AllFacilities.Count(f => f.IsBuilt)}座", C(0.3f, 1.0f, 0.3f));
        InfoCard(summaryGrid, "建造中", $"{GM.Facilities.AllFacilities.Count(f => f.IsUnderConstruction)}座", C(1.0f, 0.8f, 0.2f));
        InfoCard(summaryGrid, "道侣", $"{GM.Companions.AllCompanions.Count(c => c.IsMarried)}对", C(1.0f, 0.5f, 0.7f));
        _overviewDashboard.AddChild(SP(10));

        // Resource overview
        _overviewDashboard.AddChild(HL("资源储备", 15, C(0.91f, 0.72f, 0.29f)));
        _overviewDashboard.AddChild(SP(4));
        var resGrid = new GridContainer { Columns = 4 };
        _overviewDashboard.AddChild(resGrid);
        foreach (ResourceType rt in Enum.GetValues<ResourceType>())
        {
            int val = GM.Resources.Get(rt);
            int inc = GM.Resources.GetIncome(rt);
            string text = inc > 0 ? $"{ResName(rt)}: {val} (+{inc}/d)" : $"{ResName(rt)}: {val}";
            var lb = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
            lb.AddThemeFontSizeOverride("font_size", 12);
            lb.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
            resGrid.AddChild(lb);
        }
        _overviewDashboard.AddChild(SP(8));

        // Daily income detail
        _overviewDashboard.AddChild(HL("弟子分工与产出", 15, C(0.91f, 0.72f, 0.29f)));
        _overviewDashboard.AddChild(SP(4));

        // Task distribution
        var taskCounts = new int[9];
        foreach (var d in GM.Disciples.AllDisciples) taskCounts[(int)d.CurrentTask]++;
        var taskLine = "";
        for (int i = 0; i < TaskNames.Length; i++)
            if (taskCounts[i] > 0) taskLine += $"{TaskNames[i]}×{taskCounts[i]}  ";
        if (taskLine == "") taskLine = "暂无分工";
        var tl = new Label { Text = taskLine, HorizontalAlignment = HorizontalAlignment.Center }; tl.AddThemeFontSizeOverride("font_size", 12); tl.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _overviewDashboard.AddChild(tl);

        // Facility summary
        int built = GM.Facilities.AllFacilities.Count(f => f.IsBuilt);
        int building = GM.Facilities.AllFacilities.Count(f => f.IsUnderConstruction);
        var fl = new Label { Text = $"设施: 已建{built}座 建造中{building}座", HorizontalAlignment = HorizontalAlignment.Center };
        fl.AddThemeFontSizeOverride("font_size", 12); fl.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        _overviewDashboard.AddChild(fl);

        // Daily income
        var incLine = "每日产出: ";
        foreach (ResourceType rt in Enum.GetValues<ResourceType>())
        {
            int inc = GM.Resources.GetIncome(rt);
            if (inc > 0) incLine += $"【{ResName(rt)} +{inc}】 ";
        }
        if (incLine == "每日产出: ") incLine += "暂无（建造设施后产出）";
        var il = new Label { Text = incLine, HorizontalAlignment = HorizontalAlignment.Center }; il.AddThemeFontSizeOverride("font_size", 12); il.AddThemeColorOverride("font_color", C(0.3f, 1, 0.3f));
        _overviewDashboard.AddChild(il);

        // Facility-task synergies
        var synergyLine = "设施加成: ";
        var tempSyn = new Dictionary<DiscipleTaskType, double>();
        foreach (var f in GM.Facilities.AllFacilities.Where(f => f.IsBuilt))
        {
            foreach (DiscipleTaskType tt in Enum.GetValues<DiscipleTaskType>())
            {
                double fb = FacilityTable.GetTaskBonus(f.Type, f.Level, tt);
                if (fb > 0)
                {
                    tempSyn.TryGetValue(tt, out var v);
                    tempSyn[tt] = v + fb;
                }
            }
        }
        if (tempSyn.Count > 0)
        {
            foreach (var kv in tempSyn)
                synergyLine += $"{TaskNames[(int)kv.Key]}+{kv.Value * 100:F0}% ";
        }
        else synergyLine += GM.Facilities.Count > 0 ? "建造对应设施激活加成" : "暂无";
        var sl2 = new Label { Text = synergyLine, HorizontalAlignment = HorizontalAlignment.Center }; sl2.AddThemeFontSizeOverride("font_size", 11); sl2.AddThemeColorOverride("font_color", C(0.3f, 1, 0.8f));
        _overviewDashboard.AddChild(sl2);
    }

    void RefreshResources()
    {
        var m = new Dictionary<ResourceType, (string, Color)>
        {
            [ResourceType.SpiritStone] = ("灵石", C(0.91f, 0.72f, 0.29f)),
            [ResourceType.Herb] = ("灵草", C(0.3f, 0.8f, 0.3f)),
            [ResourceType.Ore] = ("矿石", C(0.7f, 0.6f, 0.4f)),
            [ResourceType.Pill] = ("丹药", C(0.9f, 0.4f, 0.6f)),
            [ResourceType.Equipment] = ("法器", C(0.5f, 0.5f, 1.0f)),
            [ResourceType.Contribution] = ("贡献", C(0.8f, 0.8f, 0.3f)),
            [ResourceType.SpiritEssence] = ("灵气", C(0.4f, 0.7f, 1.0f)),
        };
        foreach (var kv in _resourceLabels)
        {
            int a = GM.Resources.Get(kv.Key), ic = GM.Resources.GetIncome(kv.Key);
            if (m.TryGetValue(kv.Key, out var nc))
            { kv.Value.Text = $"{nc.Item1}:{a}{(ic != 0 ? $"({ic:+0;-0}/d)" : "")}"; kv.Value.AddThemeColorOverride("font_color", nc.Item2); }
            else { kv.Value.Text = $"{kv.Key}:{a}"; }
        }
    }

    void RefreshDisciples()
    {
        _discipleList.FreeChildren();
        _discipleList.AddChild(HL($"弟子列表（共{GM.Disciples.Count}人）", 16, C(0.91f, 0.72f, 0.29f)));
        foreach (var d in GM.Disciples.AllDisciples)
        {
            int did = d.Id;
            var wrapper = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            wrapper.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = C(0.16f, 0.13f, 0.22f), CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6 });

            // Card row: avatar + content
            var cardRow = new HBoxContainer(); wrapper.AddChild(cardRow);

            // Avatar placeholder
            var avatar = new PanelContainer { CustomMinimumSize = new Vector2I(52, 52) };
            var avatarBg = new StyleBoxFlat { BgColor = d.IsMale ? C(0.18f, 0.22f, 0.35f) : C(0.30f, 0.16f, 0.25f), CornerRadiusBottomLeft = 26, CornerRadiusBottomRight = 26, CornerRadiusTopLeft = 26, CornerRadiusTopRight = 26 };
            avatar.AddThemeStyleboxOverride("panel", avatarBg);
            var avatarLabel = new Label { Text = d.IsMale ? "♂" : "♀", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            avatarLabel.AddThemeFontSizeOverride("font_size", 22);
            avatarLabel.AddThemeColorOverride("font_color", d.IsMale ? C(0.5f, 0.7f, 1.0f) : C(1.0f, 0.5f, 0.7f));
            avatar.AddChild(avatarLabel);
            cardRow.AddChild(avatar);
            cardRow.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

            // Content column
            var card = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            cardRow.AddChild(card);
            _discipleList.AddChild(wrapper);

            // Row 1: name (clickable) + realm + badge
            var r1 = new HBoxContainer(); card.AddChild(r1);
            string genderIcon = d.IsMale ? "♂" : "♀";
            bool isChild = d.Skills.ContainsKey(10);
            string rootTag = d.SpiritRoot != SpiritualRoot.None ? $" [{d.SpiritRootName}]" : "";
            string title = isChild ? $"{genderIcon}{d.Name} · {d.FullRealmName} 🔯{rootTag}" : $"{genderIcon}{d.Name} · {d.FullRealmName}{rootTag}";
            var nl = new Button { Text = title, SizeFlagsHorizontal = SizeFlags.ExpandFill, Alignment = HorizontalAlignment.Center };
            nl.AddThemeFontSizeOverride("font_size", 15); nl.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f));
            nl.AddThemeColorOverride("font_hover_color", C(1, 1, 1));
            nl.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
            nl.AddThemeStyleboxOverride("hover", new StyleBoxEmpty());
            nl.AddThemeStyleboxOverride("pressed", new StyleBoxEmpty());
            nl.AddThemeStyleboxOverride("disabled", new StyleBoxEmpty());
            nl.Flat = true;
            var capturedD = d; // capture for lambda
            nl.Pressed += () => ShowDiscipleDetail(capturedD);
            r1.AddChild(nl);
            if (d.CompanionId >= 0)
            {
                var comp = GM.Companions.Get(d.CompanionId);
                if (comp != null)
                {
                    string compIndicator = comp.IsMarried ? " 💍" : " ❤";
                    var cl = new Label { Text = compIndicator };
                    cl.AddThemeFontSizeOverride("font_size", 12);
                    cl.AddThemeColorOverride("font_color", comp.IsMarried ? C(1, 0.5f, 0.7f) : C(1, 0.7f, 0.4f));
                    r1.AddChild(cl);
                }
            }
            if (d.IsInBreakthrough) { var ba = new Label { Text = " ⚠突破中" }; ba.AddThemeFontSizeOverride("font_size", 11); ba.AddThemeColorOverride("font_color", C(1, 0.6f, 0)); r1.AddChild(ba); }

            // Row 2: task dropdown + dismiss
            var r2 = new HBoxContainer(); card.AddChild(r2);
            r2.AddChild(new Label { Text = "任务: " });
            var drop = new OptionButton();
            drop.AddThemeFontSizeOverride("font_size", 11);
            drop.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            drop.CustomMinimumSize = new Vector2I(100, 0);
            foreach (var tn in TaskNames) drop.AddItem(tn);
            drop.Select((int)d.CurrentTask);
            drop.ItemSelected += (idx) => {
                var task = (DiscipleTaskType)(int)idx;
                // CallDeferred avoids modifying controls during event callback
                Callable.From(() => { GM.AssignTask(did, task); }).CallDeferred();
            };
            r2.AddChild(drop);
            var dis = SmallBtn("遣散"); r2.AddChild(dis);
            dis.Pressed += () => GM.DismissDisciple(did);

            // Row 3: stats as single label
            var r3 = new Label { Text = $"天赋{d.Talent}  悟性{d.Comprehension}  体质{d.Constitution}  神识{d.Spirit}  战力{d.CombatPower}", HorizontalAlignment = HorizontalAlignment.Center };
            r3.AddThemeFontSizeOverride("font_size", 12); r3.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
            card.AddChild(r3);

            // Row 4: status with color warnings
            string moodWarn = d.Mood < 20 ? "⚠" : "";
            string loyaltyWarn = d.Loyalty < 20 ? "⚠" : "";
            string staminaWarn = d.CurrentStamina <= 10 ? "⚠" : "";
            var r4 = new Label { Text = $"心情{moodWarn}{d.Mood:F0}  忠诚{loyaltyWarn}{d.Loyalty}  体力{staminaWarn}{d.CurrentStamina}/{d.MaxStamina}  气血{d.Health}/{d.MaxHealth}  修为{d.CultivationProgress:F0}", HorizontalAlignment = HorizontalAlignment.Center };
            r4.AddThemeFontSizeOverride("font_size", 12);
            Color r4Color = d.Mood < 20 || d.Loyalty < 20 ? C(1, 0.5f, 0.3f) : d.CurrentStamina <= 10 ? C(1, 0.7f, 0.2f) : C(0.88f, 0.88f, 0.92f);
            r4.AddThemeColorOverride("font_color", r4Color);
            card.AddChild(r4);

            // Row 5: contribution + proficiency + skills
            string profText = d.TaskProficiency.TryGetValue(d.CurrentTask, out int prof) && prof > 0
                ? $"专精{TaskNames[(int)d.CurrentTask]}Lv.{prof}  " : "";
            var skillText = d.Skills.Count > 0
                ? $"技能: {string.Join(" ", d.Skills.Select(s => $"{SkillName(s.Key)}Lv.{s.Value}"))}"
                : "";
            var r5 = new Label { Text = $"{profText}贡献 {d.TotalContribution}  {skillText}", HorizontalAlignment = HorizontalAlignment.Center };
            r5.AddThemeFontSizeOverride("font_size", 11); r5.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f));
            card.AddChild(r5);

            // Row 6: facility bonus indicator
            double taskBonus = FacilityTable.GetTotalTaskBonus(d.CurrentTask, GM.Facilities.AllFacilities);
            if (taskBonus > 0)
            {
                var r6 = new Label { Text = $"设施加成: +{taskBonus * 100:F0}% 效率", HorizontalAlignment = HorizontalAlignment.Center };
                r6.AddThemeFontSizeOverride("font_size", 11); r6.AddThemeColorOverride("font_color", C(0.3f, 1, 0.8f));
                card.AddChild(r6);
            }

            _discipleList.AddChild(SP(3));
        }
    }

    void RefreshFacilities()
    {
        _facilityList.FreeChildren();
        _facilityList.AddChild(HL($"设施列表（共{GM.Facilities.Count}座）", 16, C(0.91f, 0.72f, 0.29f)));
        if (GM.Facilities.Count == 0) { _facilityList.AddChild(SP(8)); _facilityList.AddChild(TB("还没有建造任何设施。在「概况」页点击建造。")); return; }
        foreach (var f in GM.Facilities.AllFacilities.OrderByDescending(f => f.Level))
        {
            var card = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            card.AddThemeConstantOverride("separation", 2);
            var wrapper = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            wrapper.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = C(0.16f, 0.13f, 0.22f), CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6 });
            wrapper.AddChild(card);
            _facilityList.AddChild(wrapper);
            var r1 = new HBoxContainer(); card.AddChild(r1);
            // Facility icon placeholder
            var facAvatar = new PanelContainer { CustomMinimumSize = new Vector2I(40, 40) };
            var facBg = new StyleBoxFlat { BgColor = C(0.15f, 0.18f, 0.28f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
            facAvatar.AddThemeStyleboxOverride("panel", facBg);
            string facIcon = f.Type switch { FacilityType.MeditationChamber => "🧘", FacilityType.AlchemyRoom => "⚗", FacilityType.TrainingGround => "⚔", FacilityType.Library => "📚", FacilityType.PillRefinery => "💊", FacilityType.SpiritGarden => "🌿", FacilityType.OreMine => "⛏", FacilityType.FormationHall => "🔮", FacilityType.DiningHall => "🍽", FacilityType.GuestHall => "🏠", _ => "🏗" };
            var facLabel = new Label { Text = facIcon, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            facLabel.AddThemeFontSizeOverride("font_size", 16);
            facAvatar.AddChild(facLabel);
            r1.AddChild(facAvatar);
            r1.AddChild(new Control { CustomMinimumSize = new Vector2I(6, 0) });

            var nl = new Label { Text = $"{f.TypeName} Lv.{f.Level}", HorizontalAlignment = HorizontalAlignment.Center };
            nl.AddThemeFontSizeOverride("font_size", 15); nl.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f)); r1.AddChild(nl);
            var sl = new Label { Text = f.StatusText, HorizontalAlignment = HorizontalAlignment.Center };
            sl.AddThemeFontSizeOverride("font_size", 11);
            sl.AddThemeColorOverride("font_color", f.IsBuilt ? C(0.3f, 1, 0.3f) : f.IsUnderConstruction ? C(1, 0.8f, 0.2f) : C(0.55f, 0.55f, 0.65f));
            r1.AddChild(sl);

            var r2 = new HBoxContainer(); card.AddChild(r2);
            if (f.IsBuilt)
            {
                r2.AddChild(ST($"{f.MaxDisciples}人", C(0.88f, 0.88f, 0.92f)));
                r2.AddChild(ST($"产{FacilityTable.GetOutput(f.Type,f.Level)}/d", C(0.3f, 1, 0.3f)));
                r2.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
                if (f.Level < f.MaxLevel)
                {
                    int cost = FacilityTable.GetUpgradeCost(f.Type, f.Level);
                    var ub = SmallBtn($"升级{cost}灵石");
                    int fid = f.Id; ub.Pressed += () => GM.UpgradeFacility(fid); r2.AddChild(ub);
                }
                else r2.AddChild(ST("已满级", C(0.55f, 0.55f, 0.65f)));
            }
            else if (f.IsUnderConstruction)
            {
                string statusLabel = f.IsUpgrading ? "升级中" : "建造中";
                r2.AddChild(ST($"{statusLabel} {f.ConstructionProgress}/{FacilityTable.GetInfo(f.Type).BuildDays}天", C(1, 0.8f, 0.2f)));
            }
            else r2.AddChild(ST($"容纳{f.MaxDisciples}人", C(0.88f, 0.88f, 0.92f)));

            _facilityList.AddChild(SP(4));
        }
    }

    void RefreshCompanions()
    {
        _companionList.FreeChildren();
        var allComps = GM.Companions.AllCompanions;
        _companionList.AddChild(HL($"道侣因缘（共{allComps.Count}对）", 16, C(0.91f, 0.72f, 0.29f)));

        // --- Existing companions ---
        if (allComps.Count > 0)
        {
            foreach (var c in allComps)
            {
                var d1 = GM.Disciples.Get(c.DiscipleId1);
                var d2 = GM.Disciples.Get(c.DiscipleId2);
                if (d1 == null || d2 == null) continue;

                var card = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                var wrapper = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                wrapper.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = c.IsMarried ? C(0.17f, 0.10f, 0.20f) : C(0.13f, 0.11f, 0.18f), CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6 });
                wrapper.AddChild(card);
                _companionList.AddChild(wrapper);

                // Row 1: names + status
                var r1 = new HBoxContainer(); card.AddChild(r1);
                string gender1 = d1.IsMale ? "♂" : "♀";
                string gender2 = d2.IsMale ? "♂" : "♀";
                var nameLabel = new Label { Text = $"{gender1}{d1.Name}  ❤  {gender2}{d2.Name}", HorizontalAlignment = HorizontalAlignment.Center };
                nameLabel.AddThemeFontSizeOverride("font_size", 14); nameLabel.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f));
                r1.AddChild(nameLabel);
                r1.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
                string status = c.IsMarried ? "💍已结为道侣" : "未结道侣";
                var statusLabel = new Label { Text = status };
                statusLabel.AddThemeFontSizeOverride("font_size", 12);
                statusLabel.AddThemeColorOverride("font_color", c.IsMarried ? C(1, 0.6f, 0.8f) : C(0.55f, 0.55f, 0.65f));
                r1.AddChild(statusLabel);

                // Row 2: affection bar + label
                var r2 = new HBoxContainer(); card.AddChild(r2);
                var affLabel = new Label { Text = $"好感度: {c.Affection:F0}/100 [{c.AffectionLabel}]  共修{c.YearsTogether}天", HorizontalAlignment = HorizontalAlignment.Center };
                affLabel.AddThemeFontSizeOverride("font_size", 12);
                affLabel.AddThemeColorOverride("font_color", c.Affection >= 80 ? C(1, 0.5f, 0.7f) : c.Affection >= 60 ? C(1, 0.7f, 0.3f) : C(0.88f, 0.88f, 0.92f));
                r2.AddChild(affLabel);

                // Affection bar
                var bar = new ColorRect { CustomMinimumSize = new Vector2I((int)(c.Affection * 2), 6) };
                bar.Color = c.Affection >= 80 ? C(1, 0.3f, 0.6f) : c.Affection >= 60 ? C(1, 0.6f, 0.2f) : C(0.5f, 0.5f, 0.8f);
                r2.AddChild(bar);

                // Row 3: dual cultivation info + actions
                var r3 = new HBoxContainer(); card.AddChild(r3);
                bool bothCultivate = d1.CurrentTask == DiscipleTaskType.Cultivate && d2.CurrentTask == DiscipleTaskType.Cultivate;
                var dualLabel = new Label { Text = bothCultivate ? $"双修加成: +{c.DualCultivationBonus * 100:F0}% (生效中)" : $"双修加成: +{c.DualCultivationBonus * 100:F0}% (需两人同时修炼)", HorizontalAlignment = HorizontalAlignment.Center };
                dualLabel.AddThemeFontSizeOverride("font_size", 11);
                dualLabel.AddThemeColorOverride("font_color", bothCultivate ? C(0.3f, 1, 0.3f) : C(0.55f, 0.55f, 0.65f));
                r3.AddChild(dualLabel);
                r3.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

                // Gift buttons
                var giftPill = SmallBtn("赠丹药(5好感)");
                int cId = c.Id;
                giftPill.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.Pill, 1);
                r3.AddChild(giftPill);
                var giftStone = SmallBtn("赠灵石(50=5好感)");
                giftStone.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.SpiritStone, 50);
                r3.AddChild(giftStone);

                // Row 4: Marry / Breakup
                var r4 = new HBoxContainer(); card.AddChild(r4);
                r4.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
                if (!c.IsMarried)
                {
                    var marryBtn = SmallBtn("结为道侣(需好感60)");
                    marryBtn.Disabled = c.Affection < 60;
                    marryBtn.Pressed += () => GM.ProposeMarriage(cId);
                    r4.AddChild(marryBtn);
                }
                var breakBtn = SmallBtn("和离");
                breakBtn.Pressed += () => GM.BreakUpCompanion(cId);
                r4.AddChild(breakBtn);

                _companionList.AddChild(SP(6));
            }
        }
        else
        {
            _companionList.AddChild(SP(8));
            _companionList.AddChild(TB("尚无道侣因缘。「牵线搭桥」让两位单身弟子结下道缘。"));
        }

        // --- Matchmaking ---
        _companionList.AddChild(SP(12));
        _companionList.AddChild(HL("牵线搭桥", 16, C(0.91f, 0.72f, 0.29f)));
        _companionList.AddChild(SP(6));

        var singles = GM.Disciples.AllDisciples.Where(d => d.CompanionId < 0).ToList();
        var males = singles.Where(d => d.IsMale).ToList();
        var females = singles.Where(d => !d.IsMale).ToList();

        if (males.Count == 0 || females.Count == 0)
        {
            _companionList.AddChild(TB(males.Count + females.Count == 0
                ? "没有单身弟子可牵线。招募更多弟子吧。"
                : "单身弟子性别比例失衡，需要更多异性弟子才能牵线。"));
        }
        else
        {
            var matchRow = new HBoxContainer(); _companionList.AddChild(matchRow);
            matchRow.AddChild(new Label { Text = "选择两位单身弟子牵线结缘（消耗1天）: " }.WithFont(12, C(0.88f, 0.88f, 0.92f)));

            _matchMaleDrop = new OptionButton();
            foreach (var m in males) _matchMaleDrop.AddItem($"♂{m.Name} ({m.FullRealmName})");
            matchRow.AddChild(_matchMaleDrop);

            matchRow.AddChild(new Label { Text = " + " }.WithFont(13, C(0.91f, 0.72f, 0.29f)));

            _matchFemaleDrop = new OptionButton();
            foreach (var f in females) _matchFemaleDrop.AddItem($"♀{f.Name} ({f.FullRealmName})");
            matchRow.AddChild(_matchFemaleDrop);

            var introduceBtn = SmallBtn("牵线");
            introduceBtn.Pressed += () =>
            {
                if (_matchMaleDrop.Selected < 0 || _matchFemaleDrop.Selected < 0) return;
                var m = males[_matchMaleDrop.Selected];
                var f = females[_matchFemaleDrop.Selected];
                GM.IntroduceCompanions(m.Id, f.Id);
            };
            matchRow.AddChild(introduceBtn);

            _companionList.AddChild(SP(4));
            _companionList.AddChild(TB("牵线成功率受境界相近度、属性互补、年龄相仿、忠诚度影响。"));
        }
    }

    void RefreshSaveSlots()
    {
        _slotContainer.FreeChildren();
        _saveErrorLabel.Text = "";
        var slots = GM.SaveLoad.GetOccupiedSlots().ToHashSet();
        for (int i = 0; i < SaveLoadManager.MaxSlots; i++)
        {
            int si = i;
            var frame = new HBoxContainer();
            var info = new Label { SizeFlagsHorizontal = SizeFlags.ExpandFill, HorizontalAlignment = HorizontalAlignment.Center };
            info.AddThemeFontSizeOverride("font_size", 13);
            if (slots.Contains(i))
            {
                var data = GM.SaveLoad.LoadFromSlot(i);
                if (data != null)
                    info.Text = $"存档位 {i + 1}  |  第{data.CurrentYear}年  |  {data.SectName}  |  弟子{data.Disciples.Count}人  |  灵石{data.Resources.GetValueOrDefault(ResourceType.SpiritStone)}";
                else
                    info.Text = $"存档位 {i + 1}  |  空";
            }
            else info.Text = $"存档位 {i + 1}  |  空";
            frame.AddChild(info);
            var saveBtn2 = SmallBtn("保存到此");
            saveBtn2.Pressed += () => OnSaveSlot(si);
            frame.AddChild(saveBtn2);
            if (slots.Contains(i))
            {
                var delBtn = SmallBtn("删除");
                delBtn.Pressed += () => OnDeleteSlot(si);
                frame.AddChild(delBtn);
            }
            _slotContainer.AddChild(frame);
        }
    }

    void OnDeleteSlot(int slot)
    {
        GM.SaveLoad.DeleteSlot(slot);
        _saveErrorLabel.Text = $"存档位 {slot + 1} 已删除。";
        _saveErrorLabel.AddThemeColorOverride("font_color", C(1, 0.7f, 0.3f));
        RefreshSaveSlots();
    }

    void OnSaveSlot(int slot)
    {
        GM.SaveGame(slot);
        _saveErrorLabel.Text = $"已保存到存档位 {slot + 1}。";
        _saveErrorLabel.AddThemeColorOverride("font_color", C(0.3f, 1, 0.3f));
        RefreshSaveSlots();
    }

    void RefreshLoadSlots()
    {
        _loadSlotContainer.FreeChildren();
        _loadErrorLabel.Text = "";
        var slots = GM.SaveLoad.GetOccupiedSlots().ToHashSet();
        if (slots.Count == 0)
        {
            _loadSlotContainer.AddChild(new Label { Text = "  没有找到存档。", SizeFlagsHorizontal = SizeFlags.ExpandFill });
            return;
        }
        for (int i = 0; i < SaveLoadManager.MaxSlots; i++)
        {
            if (!slots.Contains(i)) continue;
            int si = i;
            var data = GM.SaveLoad.LoadFromSlot(i);
            if (data == null) continue;
            var frame = new HBoxContainer();
            var info = new Label { SizeFlagsHorizontal = SizeFlags.ExpandFill, HorizontalAlignment = HorizontalAlignment.Center };
            info.AddThemeFontSizeOverride("font_size", 13);
            info.Text = $"存档位 {i + 1}  |  第{data.CurrentYear}年  |  {data.SectName}  |  弟子{data.Disciples.Count}人  |  灵石{data.Resources.GetValueOrDefault(ResourceType.SpiritStone)}";
            frame.AddChild(info);
            var loadBtn = SmallBtn("读档");
            loadBtn.Pressed += () => OnLoadSlot(si);
            frame.AddChild(loadBtn);
            var delBtn = SmallBtn("删除");
            delBtn.Pressed += () => { GM.SaveLoad.DeleteSlot(si); RefreshLoadSlots(); };
            frame.AddChild(delBtn);
            _loadSlotContainer.AddChild(frame);
        }
    }

    void OnLoadSlot(int slot)
    {
        if (!GM.LoadGame(slot))
        {
            _loadErrorLabel.Text = "读档失败，存档可能已损坏。";
            return;
        }
        _loadPopup.Hide();
        RefreshAll();
    }

    void RefreshQuests()
    {
        _questList.FreeChildren();
        _questList.AddChild(HL($"宗门任务（{GM.Quests.AllQuests.Count(q => q.Completed)}/{GM.Quests.AllQuests.Count}完成）", 16, C(0.91f, 0.72f, 0.29f)));
        _questList.AddChild(SP(6));
        foreach (var q in GM.Quests.AllQuests)
        {
            var card = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var wrapper = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var bgColor = q.Completed ? C(0.12f, 0.18f, 0.12f) : C(0.16f, 0.13f, 0.22f);
            wrapper.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = bgColor, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6 });
            wrapper.AddChild(card);
            _questList.AddChild(wrapper);

            var title = new Label { Text = q.Completed ? $"✓ {q.Title}" : q.Title, HorizontalAlignment = HorizontalAlignment.Center };
            title.AddThemeFontSizeOverride("font_size", 14);
            title.AddThemeColorOverride("font_color", q.Completed ? C(0.3f, 1, 0.3f) : C(0.91f, 0.72f, 0.29f));
            card.AddChild(title);

            var desc = new Label { Text = $"{q.Description}  [{q.ProgressText}]", HorizontalAlignment = HorizontalAlignment.Center };
            desc.AddThemeFontSizeOverride("font_size", 12);
            desc.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
            card.AddChild(desc);

            var reward = new Label { Text = $"奖励: {q.RewardText}", HorizontalAlignment = HorizontalAlignment.Center };
            reward.AddThemeFontSizeOverride("font_size", 11);
            reward.AddThemeColorOverride("font_color", C(0.5f, 0.8f, 0.5f));
            card.AddChild(reward);

            _questList.AddChild(SP(4));
        }
    }

    void RefreshBgmIndicator()
    {
        if (AudioManager.BgmNames.Count > 0)
        {
            int idx = AudioManager.CurrentBgmIndex % AudioManager.BgmNames.Count;
            _bgmIndicator.Text = $"♪ {AudioManager.BgmNames[idx]}";
        }
    }

    void RefreshBgmPopup()
    {
        var vbox = (VBoxContainer)_bgmSelectPopup.GetChild(0);
        vbox.FreeChildren();
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 6) });
        var title = new Label { Text = "选择背景音乐", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 14);
        title.AddThemeColorOverride("font_color", C(0.91f, 0.72f, 0.29f));
        vbox.AddChild(title);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });

        for (int i = 0; i < AudioManager.BgmNames.Count; i++)
        {
            int idx = i;
            var isCurrent = i == AudioManager.CurrentBgmIndex;
            var btn = new Button
            {
                Text = isCurrent ? $"▶ {AudioManager.BgmNames[i]}" : AudioManager.BgmNames[i],
                Alignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2I(0, 30),
            };
            btn.AddThemeFontSizeOverride("font_size", 12);
            btn.AddThemeColorOverride("font_color", isCurrent ? C(0.3f, 1, 0.3f) : C(0.88f, 0.88f, 0.92f));
            btn.AddThemeColorOverride("font_hover_color", C(0.91f, 0.72f, 0.29f));
            var sb = new StyleBoxFlat { BgColor = isCurrent ? C(0.12f, 0.20f, 0.12f) : C(0.15f, 0.12f, 0.20f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
            var sh = new StyleBoxFlat { BgColor = C(0.25f, 0.20f, 0.35f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 };
            btn.AddThemeStyleboxOverride("normal", sb);
            btn.AddThemeStyleboxOverride("hover", sh);
            btn.Pressed += () =>
            {
                AudioManager.SetBgm(idx);
                _bgmSelectPopup.Hide();
                RefreshBgmIndicator();
            };
            vbox.AddChild(btn);
            vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 2) });
        }
    }

    void RefreshLog() { _logList.Clear(); foreach (var e in GM.EventLogEntries) _logList.AddItem($"第{e.Day}日|{e.Title}:{e.Message}"); }
    void AddLog(string m) { GM.EventLogEntries.Add(new LogEntry { Title = "日志", Message = m, Day = GM.Time.Day }); if (GM.EventLogEntries.Count > 200) GM.EventLogEntries.RemoveAt(0); _logList?.AddItem($"第{GM.Time.Day}日|{m}"); }

    void ShowDiscipleDetail(DiscipleData d)
    {
        _detailPopup.Title = $"{d.Name} · 详情";
        _detailAvatarLabel.Text = d.IsMale ? "♂" : "♀";
        _detailAvatarLabel.AddThemeColorOverride("font_color", d.IsMale ? C(0.5f, 0.7f, 1.0f) : C(1.0f, 0.5f, 0.7f));
        _detailName.Text = d.Name;
        _detailRealm.Text = $"境界: {d.FullRealmName}  (第{d.Age}岁 入门{d.YearsInSect}年)";
        _detailAge.Text = $"灵根: {d.SpiritRootName}  天赋{d.Talent} 悟性{d.Comprehension} 体质{d.Constitution} 神识{d.Spirit}";
        _detailStats.Text = $"心情 {d.Mood:F0}/100  忠诚 {d.Loyalty}/100";
        _detailCombat.Text = $"战力 {d.CombatPower}  贡献 {d.TotalContribution}";
        _detailTask.Text = $"当前任务: {TaskNames[(int)d.CurrentTask]}  修为进度: {d.CultivationProgress:F0}";
        double dbonus = FacilityTable.GetTotalTaskBonus(d.CurrentTask, GM.Facilities.AllFacilities);
        if (dbonus > 0)
            _detailTask.Text += $"  设施加成: +{dbonus * 100:F0}%";
        var realmInfo = CultivationTable.GetInfo(d.Realm);
        _detailProgress.Text = d.IsInBreakthrough
            ? "⚠ 正在突破中..."
            : $"突破需求: {realmInfo.BreakthroughRequiredProgress}  当前进度: {d.CultivationProgress:F0}  (突破概率 {realmInfo.BreakthroughChance * 100:F0}%)";
        _detailMood.Text = $"体力 {d.CurrentStamina}/{d.MaxStamina}  气血 {d.Health}/{d.MaxHealth}";
        _detailStamina.Text = $"心情值影响修炼效率 {(d.Mood / 200.0 + 1.0):F2}倍";
        // Companion info
        if (d.CompanionId >= 0)
        {
            var comp = GM.Companions.Get(d.CompanionId);
            if (comp != null)
            {
                var partnerId = comp.DiscipleId1 == d.Id ? comp.DiscipleId2 : comp.DiscipleId1;
                var partner = GM.Disciples.Get(partnerId);
                string partnerName = partner?.Name ?? "?";
                _detailStamina.Text += $"  道侣: {partnerName} (好感{comp.Affection:F0})";
                if (comp.IsMarried) _detailStamina.Text += " [已结道侣]";
            }
        }
        string skillStr = d.Skills.Count > 0
            ? $"技能: {string.Join(", ", d.Skills.Select(s => $"{SkillName(s.Key)}Lv.{s.Value}"))}"
            : "技能: 无";
        string profStr = d.TaskProficiency.Count > 0
            ? $" | 专精: {string.Join(" ", d.TaskProficiency.Where(kv => kv.Value > 0).Select(kv => $"{TaskNames[(int)kv.Key]}Lv.{kv.Value}"))}"
            : "";
        _detailSkills.Text = skillStr + profStr;

        // Equipment section
        _detailEquip.Text = "";
        _equipSection.FreeChildren();
        // Show equipped items
        var equippedItems = GM.AllEquipment.Where(e => d.EquipmentIds.Contains(e.Id)).ToList();
        if (equippedItems.Count > 0)
        {
            _equipSection.AddChild(new Label { Text = "已装备:" }.WithFont(13, C(0.91f, 0.72f, 0.29f)));
            foreach (var eq in equippedItems)
            {
                var row = new HBoxContainer();
                row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, C(0.3f, 1, 0.3f)));
                var unequipBtn = SmallBtn("卸下");
                int eqId = eq.Id;
                unequipBtn.Pressed += () => { GM.UnequipItem(eqId); ShowDiscipleDetail(d); RefreshDisciples(); };
                row.AddChild(unequipBtn);
                _equipSection.AddChild(row);
            }
        }
        else _equipSection.AddChild(new Label { Text = "未装备法器" }.WithFont(11, C(0.55f, 0.55f, 0.65f)));

        // Show available equipment (not equipped by anyone)
        var available = GM.AllEquipment.Where(e => e.EquippedById < 0).ToList();
        if (available.Count > 0 && d.EquipmentIds.Count < 2)
        {
            _equipSection.AddChild(new Label { Text = "可装备:" }.WithFont(13, C(0.91f, 0.72f, 0.29f)));
            foreach (var eq in available.Take(5))
            {
                var row = new HBoxContainer();
                row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, C(0.88f, 0.88f, 0.92f)));
                var equipBtn = SmallBtn("装备");
                int eqId = eq.Id; int dId = d.Id;
                equipBtn.Pressed += () => { GM.EquipItem(eqId, dId); ShowDiscipleDetail(d); RefreshDisciples(); };
                row.AddChild(equipBtn);
                _equipSection.AddChild(row);
            }
        }

        // Color-coded mood
        _detailStats.AddThemeColorOverride("font_color", d.Mood >= 70 ? C(0.3f, 1, 0.3f) : d.Mood >= 40 ? C(1, 0.8f, 0.2f) : C(1, 0.3f, 0.3f));

        _detailPopup.PopupCentered();
    }

    void OnEventTriggered(EventData e)
    {
        _eventPopup.Title = e.Title; _eventTitle.Text = e.Title; _eventDesc.Text = e.Description;
        _eventChoice1.Text = e.Choice1Text; _eventChoice1.Show();
        _eventEffect1.Text = FormatOutcome(e.Choice1Outcome); _eventEffect1.Show();
        _eventChoice2.Text = e.Choice2Text; _eventChoice2.Show();
        _eventEffect2.Text = FormatOutcome(e.Choice2Outcome); _eventEffect2.Show();
        _eventChoice3.Text = string.IsNullOrEmpty(e.Choice3Text) ? "无视" : e.Choice3Text; _eventChoice3.Show();
        _eventEffect3.Text = e.Choice3Outcome != null ? FormatOutcome(e.Choice3Outcome) : ""; _eventEffect3.Show();
        _dismissedBtn.Hide(); _eventPopup.PopupCentered();
    }
    void ResolveChoice(int i) { _eventChoice1.Hide(); _eventChoice2.Hide(); _eventChoice3.Hide(); _eventEffect1.Hide(); _eventEffect2.Hide(); _eventEffect3.Hide(); _dismissedBtn.Show(); GM.ResolveEvent(i); _dismissedBtn.Text = "关闭"; }
    void DismissEventPopup() { _eventPopup.Hide(); GM.DismissEvent(); RefreshAll(); }
    public override void _Input(InputEvent e) { if (!IsInsideTree()) return; if (e.IsActionPressed("ui_cancel") && _eventPopup.Visible) DismissEventPopup(); }

    // ===================== HELPERS =====================

    static Control SP(int h) => new Control { CustomMinimumSize = new Vector2I(0, h) };
    static Color C(float r, float g, float b) => new(r, g, b);
    static Label HL(string t, int fs, Color c) { var lb = new Label { Text = t, HorizontalAlignment = HorizontalAlignment.Center }; lb.AddThemeFontSizeOverride("font_size", fs); lb.AddThemeColorOverride("font_color", c); return lb; }
    static Label TB(string t) { var lb = new Label { Text = t, AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center }; lb.AddThemeFontSizeOverride("font_size", 13); lb.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f)); return lb; }
    static Label ST(string t, Color c) { var lb = new Label { Text = $"  {t}  ", HorizontalAlignment = HorizontalAlignment.Center }; lb.AddThemeFontSizeOverride("font_size", 12); lb.AddThemeColorOverride("font_color", c); return lb; }
    static Label EL() // effect label
    {
        var lb = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        lb.AddThemeFontSizeOverride("font_size", 11);
        lb.AddThemeColorOverride("font_color", C(0.5f, 0.8f, 0.5f));
        return lb;
    }

    static string FormatOutcome(EventOutcome o)
    {
        var parts = new List<string>();
        // Resource changes
        if (o.ResourceChanges != null)
        {
            foreach (var kv in o.ResourceChanges)
            {
                string sign = kv.Value >= 0 ? "+" : "";
                parts.Add($"{ResName(kv.Key)}{sign}{kv.Value}");
            }
        }
        // Reputation / Power
        if (o.ReputationChange != 0) parts.Add($"声望{(o.ReputationChange >= 0 ? "+" : "")}{o.ReputationChange}");
        if (o.PowerChange != 0) parts.Add($"战力{(o.PowerChange >= 0 ? "+" : "")}{o.PowerChange}");
        // Disciple stat effects: [loyalty, mood, health]
        if (o.DiscipleStatEffects != null && o.DiscipleStatEffects.Length >= 3)
        {
            if (o.DiscipleStatEffects[0] != 0) parts.Add($"全体忠诚{(o.DiscipleStatEffects[0] >= 0 ? "+" : "")}{o.DiscipleStatEffects[0]}");
            if (o.DiscipleStatEffects[1] != 0) parts.Add($"全体心情{(o.DiscipleStatEffects[1] >= 0 ? "+" : "")}{o.DiscipleStatEffects[1]}");
            if (o.DiscipleStatEffects[2] != 0) parts.Add($"全体气血{(o.DiscipleStatEffects[2] >= 0 ? "+" : "")}{o.DiscipleStatEffects[2]}");
        }
        // Cultivation bonus
        if (o.DiscipleCultivationBonus > 0) parts.Add($"全体修为+{o.DiscipleCultivationBonus:F0}");
        // Disciple count change
        if (o.DiscipleCountChange != 0) parts.Add($"弟子{(o.DiscipleCountChange >= 0 ? "+" : "")}{o.DiscipleCountChange}人");

        return parts.Count > 0 ? $"[ {string.Join("  |  ", parts)} ]" : "";
    }

    static void InfoCard(GridContainer grid, string label, string value, Color valColor)
    {
        var card = new VBoxContainer();
        var wrapper = new PanelContainer();
        wrapper.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = C(0.12f, 0.10f, 0.18f), CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5 });
        wrapper.AddChild(card);
        var lb = new Label { Text = label, HorizontalAlignment = HorizontalAlignment.Center };
        lb.AddThemeFontSizeOverride("font_size", 11);
        lb.AddThemeColorOverride("font_color", C(0.55f, 0.55f, 0.65f));
        card.AddChild(lb);
        var vl = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center };
        vl.AddThemeFontSizeOverride("font_size", 18);
        vl.AddThemeColorOverride("font_color", valColor);
        card.AddChild(vl);
        grid.AddChild(wrapper);
    }

    static string SkillName(int id) => id switch
    {
        1 => "修炼加速", 2 => "战斗技巧", 3 => "炼丹精通",
        4 => "炼器精通", 5 => "采集专精", 6 => "教导有方",
        7 => "探索老手", 10 => "道脉传承", _ => $"技能{id}"
    };

    static string ResName(ResourceType rt) => rt switch
    {
        ResourceType.SpiritStone => "灵石", ResourceType.Herb => "灵草", ResourceType.Ore => "矿石",
        ResourceType.Pill => "丹药", ResourceType.Equipment => "法器", ResourceType.Contribution => "贡献",
        ResourceType.SpiritEssence => "灵气", _ => rt.ToString()
    };

    static Button Btn(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) };
        b.AddThemeFontSizeOverride("font_size", 13);
        b.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", C(0.91f, 0.72f, 0.29f));
        b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
        b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
        b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed());
        return b;
    }

    static Button SmallBtn(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(60, 24) };
        b.AddThemeFontSizeOverride("font_size", 11);
        b.AddThemeColorOverride("font_color", C(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", C(0.91f, 0.72f, 0.29f));
        b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
        b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
        b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed());
        return b;
    }
}
// ===================== EXTENSIONS =====================

internal static class UIColums
{
    public static Color DarkBg => new(0.05f, 0.04f, 0.10f);
    public static Color PanelBg => new(0.10f, 0.08f, 0.15f);
    public static Color Gold => new(0.91f, 0.72f, 0.29f);
    public static Color TextDim => new(0.55f, 0.55f, 0.65f);

    public static Button MakeButton(string text, string tip)
    {
        var b = new Button { Text = text, TooltipText = tip, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) };
        b.AddThemeFontSizeOverride("font_size", 13);
        b.AddThemeColorOverride("font_color", new(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", Gold);
        b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
        b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
        b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed());
        return b;
    }

    public static Button MakeSmallButton(string text)
    {
        var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(60, 24) };
        b.AddThemeFontSizeOverride("font_size", 11);
        b.AddThemeColorOverride("font_color", new(0.88f, 0.88f, 0.92f));
        b.AddThemeColorOverride("font_hover_color", Gold);
        b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
        b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
        b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed());
        return b;
    }
}

internal static class NodeExtensions
{
    public static void FreeChildren(this Node node)
    {
        foreach (var c in node.GetChildren().ToList()) { c.Free(); }
    }

    public static Label WithFont(this Label label, int size, Color color)
    {
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }
}
