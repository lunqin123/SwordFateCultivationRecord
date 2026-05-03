namespace SwordFateCultivationRecord;

public partial class MainUI : Control
{
	private GameManager GM => GameManager.Instance;
	private static readonly string[] TaskNames = { "修炼", "训练", "采集", "炼丹", "炼器", "授课", "守卫", "探索", "休息" };
	private static readonly string[] TabLabels = { "总览", "弟子", "营造", "灵筑", "道缘", "宗门令", "记事", "卷宗" };
	private int _activeTab;

	// Top bar
	private Label _timeLabel = null!, _sectLabel = null!;
	private readonly Dictionary<ResourceType, Label> _resourceLabels = new();

	// Bottom bar
	private Button _nextDayBtn = null!;

	// Sidebar
	private readonly Button[] _sidebarBtns = new Button[8];

	// Content
	private VBoxContainer _contentStack = null!;
	private ScrollContainer _contentScroll = null!;
	private readonly VBoxContainer[] _tabContents = new VBoxContainer[8];

	// Popups
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
	private SettingsWindow _settingsPopup = null!;
	private OptionButton _matchMaleDrop = null!, _matchFemaleDrop = null!;

	// Recruitment
	private Window _recruitPopup = null!;
	private HBoxContainer _candidateRow = null!;
	private Label _tournamentLabel = null!;

	// Smart/Batch assign
	private Window _smartPopup = null!;
	private OptionButton _batchDrop = null!;
	private readonly Dictionary<int, CheckBox> _batchChecks = new();

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

	public override void _ExitTree()
	{
		EventBus.DayPassed -= OnDayPassed;
		EventBus.ResourceChanged -= OnResourceChanged;
		EventBus.EventChoiceRequired -= OnEventTriggered;
		EventBus.GameNotification -= OnGameNotification;
		EventBus.DiscipleRecruited -= OnDiscipleChanged;
		EventBus.DiscipleDeparted -= OnDiscipleChanged;
		EventBus.RecruitSelectionReady -= ShowRecruitSelection;
	}

	// ===================== BUILD =====================

	private void BuildUI()
	{
		// Background
		var bgPanel = new Panel(); bgPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		string gameBgPath = "res://Resources/Textures/BG/main_menu.png";
		if (ResourceLoader.Exists(gameBgPath))
		{
			var gameBgTex = ResourceLoader.Load<Texture2D>(gameBgPath);
			bgPanel.AddThemeStyleboxOverride("panel", new StyleBoxTexture { Texture = gameBgTex, ModulateColor = new Color(0.45f, 0.42f, 0.40f) });
		}
		else bgPanel.AddThemeStyleboxOverride("panel", UITheme.BgStyle());
		AddChild(bgPanel);

		var rootHBox = new HBoxContainer();
		rootHBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(rootHBox);

		// === LEFT SIDEBAR ===
		var sidebar = new PanelContainer { CustomMinimumSize = new Vector2I(100, 0) };
		sidebar.AddThemeStyleboxOverride("panel", UITheme.SidebarPanelStyle());
		rootHBox.AddChild(sidebar);
		var sidebarVBox = new VBoxContainer(); sidebar.AddChild(sidebarVBox);
		var iconLabel = new Label { Text = "⚔", HorizontalAlignment = HorizontalAlignment.Center };
		iconLabel.AddThemeFontSizeOverride("font_size", 24); iconLabel.AddThemeColorOverride("font_color", UITheme.Gold);
		sidebarVBox.AddChild(iconLabel); sidebarVBox.AddChild(SP(4));
		var sTitle = new Label { Text = "剑缘", HorizontalAlignment = HorizontalAlignment.Center };
		sTitle.AddThemeFontSizeOverride("font_size", 11); sTitle.AddThemeColorOverride("font_color", UITheme.TextDim);
		sidebarVBox.AddChild(sTitle); sidebarVBox.AddChild(SP(12));

		for (int i = 0; i < TabLabels.Length; i++)
		{
			int tabIdx = i;
			var btn = new Button { Text = $"  {TabLabels[i]}", Alignment = HorizontalAlignment.Left, CustomMinimumSize = new Vector2I(90, 38) };
			btn.AddThemeFontSizeOverride("font_size", 13);
			btn.AddThemeColorOverride("font_color", UITheme.TextPrimary);
			btn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
			btn.AddThemeStyleboxOverride("normal", UITheme.SidebarBtnNormal());
			btn.AddThemeStyleboxOverride("hover", UITheme.SidebarBtnHover());
			btn.Flat = true;
			btn.Pressed += () => { AudioManager.PlayClick(); SwitchToTab(tabIdx); };
			_sidebarBtns[i] = btn; sidebarVBox.AddChild(btn); sidebarVBox.AddChild(SP(1));
		}
		sidebarVBox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
		var settingsBtn = new Button { Text = "  ⚙ 设置", Alignment = HorizontalAlignment.Left, CustomMinimumSize = new Vector2I(90, 36) };
		settingsBtn.AddThemeFontSizeOverride("font_size", 12); settingsBtn.AddThemeColorOverride("font_color", UITheme.TextDim);
		settingsBtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
		settingsBtn.AddThemeStyleboxOverride("normal", UITheme.SidebarBtnNormal());
		settingsBtn.AddThemeStyleboxOverride("hover", UITheme.SidebarBtnHover());
		settingsBtn.Flat = true;
		settingsBtn.Pressed += () => { AudioManager.PlayClick(); _settingsPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_settingsPopup.GetChild(0)); };
		sidebarVBox.AddChild(settingsBtn); sidebarVBox.AddChild(SP(8));

		// === RIGHT MAIN AREA ===
		var mainArea = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		rootHBox.AddChild(mainArea);

		// --- Top Bar ---
		var topBar = new PanelContainer { CustomMinimumSize = new Vector2I(0, 52) };
		topBar.AddThemeStyleboxOverride("panel", UITheme.TopBarStyle());
		mainArea.AddChild(topBar);
		var topHBox = new HBoxContainer(); topBar.AddChild(topHBox);
		topHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
		var topLeft = new VBoxContainer(); topHBox.AddChild(topLeft);
		_sectLabel = new Label(); _sectLabel.AddThemeFontSizeOverride("font_size", 15); _sectLabel.AddThemeColorOverride("font_color", UITheme.Gold); topLeft.AddChild(_sectLabel);
		var topInfoRow = new HBoxContainer(); topLeft.AddChild(topInfoRow);
		_timeLabel = new Label(); _timeLabel.AddThemeFontSizeOverride("font_size", 12); _timeLabel.AddThemeColorOverride("font_color", UITheme.TextDim); topInfoRow.AddChild(_timeLabel);
		_tournamentLabel = new Label { Visible = false }; _tournamentLabel.AddThemeFontSizeOverride("font_size", 12); _tournamentLabel.AddThemeColorOverride("font_color", UITheme.TextOrange); topInfoRow.AddChild(_tournamentLabel);
		topHBox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
		var resRow = new HBoxContainer(); topHBox.AddChild(resRow);
		foreach (ResourceType rt in Enum.GetValues<ResourceType>())
		{
			var resIcon = SpriteSheetManager.GetResourceIcon(rt);
			if (resIcon != null) { var icon = new TextureRect { Texture = resIcon, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(20, 20) }; resRow.AddChild(icon); resRow.AddChild(new Control { CustomMinimumSize = new Vector2I(2, 0) }); }
			var lb = new Label(); lb.AddThemeFontSizeOverride("font_size", 12); _resourceLabels[rt] = lb; resRow.AddChild(lb); resRow.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });
		}
		topHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

		// --- Content Area ---
		var contentPanel = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
		contentPanel.AddThemeStyleboxOverride("panel", UITheme.ContentAreaStyle());
		mainArea.AddChild(contentPanel);
		_contentStack = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; contentPanel.AddChild(_contentStack);
		_contentStack.AddChild(SP(14));
		_contentScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
		_contentStack.AddChild(_contentScroll);

		// --- Bottom Bar ---
		var bottomBar = new PanelContainer { CustomMinimumSize = new Vector2I(0, 52) };
		bottomBar.AddThemeStyleboxOverride("panel", UITheme.BottomBarStyle()); mainArea.AddChild(bottomBar);
		var bottomHBox = new HBoxContainer(); bottomBar.AddChild(bottomHBox);
		bottomHBox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
		var timeLabel = new Label { Text = "推演天时", VerticalAlignment = VerticalAlignment.Center };
		timeLabel.AddThemeFontSizeOverride("font_size", 12); timeLabel.AddThemeColorOverride("font_color", UITheme.TextDim);
		bottomHBox.AddChild(timeLabel); bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
		_nextDayBtn = Btn("下一日"); _nextDayBtn.CustomMinimumSize = new Vector2I(110, 44); _nextDayBtn.AddThemeFontSizeOverride("font_size", 15); _nextDayBtn.AddThemeColorOverride("font_color", UITheme.Gold);
		bottomHBox.AddChild(_nextDayBtn); _nextDayBtn.Pressed += () => { GM.NextDay(); RefreshAll(); };
		bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });
		var recruitBtn = Btn("入门大比"); recruitBtn.CustomMinimumSize = new Vector2I(110, 38);
		recruitBtn.Pressed += () => { UIAnimator.ButtonPress(recruitBtn); GM.ScheduleRecruitTournament(); };
		bottomHBox.AddChild(recruitBtn); bottomHBox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
		_bgmIndicator = new Button { Text = "♪", Alignment = HorizontalAlignment.Center };
		_bgmIndicator.AddThemeFontSizeOverride("font_size", 10); _bgmIndicator.AddThemeColorOverride("font_color", new Color(0.4f, 0.6f, 0.4f));
		_bgmIndicator.AddThemeColorOverride("font_hover_color", new Color(0.6f, 0.9f, 0.6f));
		_bgmIndicator.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.05f, 0.04f, 0.08f, 0.7f), CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3, CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3 });
		_bgmIndicator.Flat = true;
		_bgmIndicator.Pressed += () => { RefreshBgmPopup(); _bgmSelectPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_bgmSelectPopup.GetChild(0)); };
		bottomHBox.AddChild(_bgmIndicator); bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });

		BuildPopups();
		BuildRecruitPopup();
		BuildSmartAssignPopup();

		for (int i = 0; i < 8; i++) _tabContents[i] = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		SwitchToTab(0);
	}

	// ===================== POPUPS =====================

	void BuildPopups()
	{
		_eventPopup = new Window { Title = "机缘", Size = new Vector2I(600, 520), Visible = false, Exclusive = true };
		_eventPopup.CloseRequested += () => { _eventPopup.Hide(); GM.DismissEvent(); RefreshAll(); };
		AddChild(_eventPopup); var ev = new VBoxContainer(); ev.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _eventPopup.AddChild(ev);
		_eventTitle = new Label { HorizontalAlignment = HorizontalAlignment.Center }; _eventTitle.AddThemeFontSizeOverride("font_size", 22); _eventTitle.AddThemeColorOverride("font_color", UITheme.Gold); ev.AddChild(_eventTitle);
		_eventDesc = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center }; _eventDesc.AddThemeFontSizeOverride("font_size", 14); _eventDesc.AddThemeColorOverride("font_color", UITheme.TextPrimary); ev.AddChild(_eventDesc);
		ev.AddChild(SP(8));
		_eventChoice1 = Btn(""); ev.AddChild(_eventChoice1); _eventEffect1 = EL(); ev.AddChild(_eventEffect1);
		_eventChoice2 = Btn(""); ev.AddChild(_eventChoice2); _eventEffect2 = EL(); ev.AddChild(_eventEffect2);
		_eventChoice3 = Btn(""); ev.AddChild(_eventChoice3); _eventEffect3 = EL(); ev.AddChild(_eventEffect3);
		_dismissedBtn = Btn("合上"); ev.AddChild(_dismissedBtn); _dismissedBtn.Hide(); _dismissedBtn.Pressed += DismissEventPopup;
		_eventChoice1.Pressed += () => ResolveChoice(0); _eventChoice2.Pressed += () => ResolveChoice(1); _eventChoice3.Pressed += () => ResolveChoice(2);

		_savePopup = new Window { Title = "存录宗门", Size = new Vector2I(540, 420), Visible = false, Exclusive = true };
		_savePopup.CloseRequested += () => _savePopup.Hide(); AddChild(_savePopup);
		var sv = new VBoxContainer(); sv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _savePopup.AddChild(sv);
		_slotContainer = new VBoxContainer(); var sc = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; sc.AddChild(_slotContainer); sv.AddChild(sc);
		_saveErrorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; _saveErrorLabel.AddThemeFontSizeOverride("font_size", 13); _saveErrorLabel.AddThemeColorOverride("font_color", UITheme.Crimson); sv.AddChild(_saveErrorLabel);
		var closeSv = Btn("归去"); sv.AddChild(closeSv); closeSv.Pressed += () => _savePopup.Hide();

		_gameOverPopup = new Window { Title = "宗门倾覆", Size = new Vector2I(400, 250), Visible = false, Exclusive = true };
		_gameOverPopup.CloseRequested += () => _gameOverPopup.Hide(); AddChild(_gameOverPopup);
		var gv = new VBoxContainer(); gv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _gameOverPopup.AddChild(gv);
		var gt2 = new Label { Text = "宗门倾覆", HorizontalAlignment = HorizontalAlignment.Center }; gt2.AddThemeFontSizeOverride("font_size", 24); gt2.AddThemeColorOverride("font_color", UITheme.Crimson); gv.AddChild(gt2);
		gv.AddChild(SP(8)); var gd = new Label { Text = "所有弟子都已离去，宗门名存实亡...\n一个修仙门派就此陨落。", HorizontalAlignment = HorizontalAlignment.Center }; gd.AddThemeFontSizeOverride("font_size", 14); gd.AddThemeColorOverride("font_color", UITheme.TextPrimary); gv.AddChild(gd);
		gv.AddChild(SP(16));
		var gr = Btn("重头来过"); var grc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; grc.AddChild(gr); gv.AddChild(grc);
		gr.Pressed += () => { _gameOverPopup.Hide(); GM.InitializeNewGame(); RefreshAll(); };
		var gm2 = Btn("归返山门"); var gm2c = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; gm2c.AddChild(gm2); gv.AddChild(gm2c);
		gm2.Pressed += () => { GetTree().ChangeSceneToFile("res://Scenes/StartMenu.tscn"); };

		_loadPopup = new Window { Title = "开卷", Size = new Vector2I(540, 420), Visible = false, Exclusive = true };
		_loadPopup.CloseRequested += () => _loadPopup.Hide(); AddChild(_loadPopup);
		var lvWin = new VBoxContainer(); lvWin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _loadPopup.AddChild(lvWin);
		_loadSlotContainer = new VBoxContainer(); var lsc2 = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; lsc2.AddChild(_loadSlotContainer); lvWin.AddChild(lsc2);
		_loadErrorLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; _loadErrorLabel.AddThemeFontSizeOverride("font_size", 13); _loadErrorLabel.AddThemeColorOverride("font_color", UITheme.Crimson); lvWin.AddChild(_loadErrorLabel);
		var closeLv = Btn("归去"); lvWin.AddChild(closeLv); closeLv.Pressed += () => _loadPopup.Hide();

		_hintPopup = new Window { Title = "启禀", Size = new Vector2I(360, 160), Visible = false, Exclusive = true, Unresizable = true };
		_hintPopup.CloseRequested += () => _hintPopup.Hide(); AddChild(_hintPopup);
		var hv = new VBoxContainer(); hv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _hintPopup.AddChild(hv);
		_hintLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, SizeFlagsVertical = SizeFlags.ExpandFill };
		_hintLabel.AddThemeFontSizeOverride("font_size", 14); _hintLabel.AddThemeColorOverride("font_color", UITheme.TextPrimary); hv.AddChild(_hintLabel);
		var hb = Btn("确定"); var hc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; hc.AddChild(hb); hv.AddChild(hc); hb.Pressed += () => _hintPopup.Hide();

		_detailPopup = new Window { Title = "弟子行状", Size = new Vector2I(480, 420), Visible = false, Exclusive = true };
		_detailPopup.CloseRequested += () => _detailPopup.Hide(); AddChild(_detailPopup);
		var dv = new VBoxContainer(); dv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _detailPopup.AddChild(dv);
		_detailContent = new VBoxContainer(); var sc3 = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; sc3.AddChild(_detailContent); dv.AddChild(sc3);
		_detailAvatarPanel = new PanelContainer { CustomMinimumSize = new Vector2I(88, 88) };
		var daBg = new StyleBoxFlat { BgColor = new Color(0.14f, 0.11f, 0.22f), CornerRadiusBottomLeft = 44, CornerRadiusBottomRight = 44, CornerRadiusTopLeft = 44, CornerRadiusTopRight = 44, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.GoldDark };
		_detailAvatarPanel.AddThemeStyleboxOverride("panel", daBg);
		_detailAvatarLabel = new Label { Text = "?", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; _detailAvatarLabel.AddThemeFontSizeOverride("font_size", 28); _detailAvatarLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f)); _detailAvatarPanel.AddChild(_detailAvatarLabel);
		var daCenter = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; daCenter.AddChild(_detailAvatarPanel); _detailContent.AddChild(daCenter); _detailContent.AddChild(SP(4));
		_detailName = new Label(); _detailName.AddThemeFontSizeOverride("font_size", 20); _detailName.AddThemeColorOverride("font_color", UITheme.Gold); _detailContent.AddChild(_detailName);
		_detailRealm = new Label(); _detailRealm.AddThemeFontSizeOverride("font_size", 14); _detailRealm.AddThemeColorOverride("font_color", UITheme.TextPrimary); _detailContent.AddChild(_detailRealm);
		_detailAge = new Label(); _detailAge.AddThemeFontSizeOverride("font_size", 12); _detailAge.AddThemeColorOverride("font_color", UITheme.TextDim); _detailContent.AddChild(_detailAge); _detailContent.AddChild(SP(4));
		_detailStats = new Label(); _detailStats.AddThemeFontSizeOverride("font_size", 13); _detailStats.AddThemeColorOverride("font_color", UITheme.TextPrimary); _detailContent.AddChild(_detailStats);
		_detailCombat = new Label(); _detailCombat.AddThemeFontSizeOverride("font_size", 13); _detailCombat.AddThemeColorOverride("font_color", UITheme.TextOrange); _detailContent.AddChild(_detailCombat);
		_detailTask = new Label(); _detailTask.AddThemeFontSizeOverride("font_size", 13); _detailTask.AddThemeColorOverride("font_color", UITheme.TextPrimary); _detailContent.AddChild(_detailTask);
		_detailProgress = new Label(); _detailProgress.AddThemeFontSizeOverride("font_size", 13); _detailProgress.AddThemeColorOverride("font_color", UITheme.TextGreen); _detailContent.AddChild(_detailProgress);
		_detailMood = new Label(); _detailMood.AddThemeFontSizeOverride("font_size", 13); _detailMood.AddThemeColorOverride("font_color", UITheme.TextPrimary); _detailContent.AddChild(_detailMood);
		_detailStamina = new Label(); _detailStamina.AddThemeFontSizeOverride("font_size", 13); _detailStamina.AddThemeColorOverride("font_color", UITheme.TextPrimary); _detailContent.AddChild(_detailStamina);
		_detailSkills = new Label(); _detailSkills.AddThemeFontSizeOverride("font_size", 12); _detailSkills.AddThemeColorOverride("font_color", UITheme.TextDim); _detailContent.AddChild(_detailSkills);
		_detailEquip = new Label(); _detailEquip.AddThemeFontSizeOverride("font_size", 12); _detailEquip.AddThemeColorOverride("font_color", UITheme.TextDim); _detailContent.AddChild(_detailEquip);
		_equipSection = new VBoxContainer(); _detailContent.AddChild(_equipSection); dv.AddChild(SP(8));
		var closeDetail = Btn("合上"); dv.AddChild(closeDetail); closeDetail.Pressed += () => _detailPopup.Hide();

		_settingsPopup = new SettingsWindow(
			onSave: () => { RefreshSaveSlots(); _savePopup.PopupCentered(); UIAnimator.WindowOpen((Control)_savePopup.GetChild(0)); },
			onLoad: () => { RefreshLoadSlots(); _loadPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_loadPopup.GetChild(0)); },
			onMainMenu: () => GetTree().ChangeSceneToFile("res://Scenes/StartMenu.tscn"));
		AddChild(_settingsPopup);

		_bgmSelectPopup = new Window { Title = "择丝竹之音", Size = new Vector2I(280, 260), Visible = false, Exclusive = true, Unresizable = true };
		_bgmSelectPopup.CloseRequested += () => _bgmSelectPopup.Hide(); AddChild(_bgmSelectPopup);
		var bgmV = new VBoxContainer(); bgmV.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _bgmSelectPopup.AddChild(bgmV);
	}

	void BuildRecruitPopup()
	{
		_recruitPopup = new Window { Title = "入门大比 — 选拔贤才", Size = new Vector2I(860, 480), Visible = false, Exclusive = true, Unresizable = true };
		_recruitPopup.CloseRequested += () => { _recruitPopup.Hide(); GM.CancelRecruit(); };
		AddChild(_recruitPopup); var rv = new VBoxContainer(); rv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _recruitPopup.AddChild(rv);
		rv.AddChild(SP(10));
		var rTitle = new Label { Text = "— 入门大比 —", HorizontalAlignment = HorizontalAlignment.Center }; rTitle.AddThemeFontSizeOverride("font_size", 22); rTitle.AddThemeColorOverride("font_color", UITheme.Gold); rv.AddChild(rTitle);
		var rSub = new Label { Text = "七日大比落幕，求道者云集，请选择一位收入门下", HorizontalAlignment = HorizontalAlignment.Center }; rSub.AddThemeFontSizeOverride("font_size", 13); rSub.AddThemeColorOverride("font_color", UITheme.TextDim); rv.AddChild(rSub); rv.AddChild(SP(12));
		var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; rv.AddChild(scroll);
		_candidateRow = new HBoxContainer(); scroll.AddChild(_candidateRow);
		rv.AddChild(SP(8));
		var cancelBtn = new Button { Text = "尽舍之（不费天时）", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(200, 40) };
		cancelBtn.AddThemeFontSizeOverride("font_size", 14); cancelBtn.AddThemeColorOverride("font_color", UITheme.TextDim); cancelBtn.AddThemeColorOverride("font_hover_color", UITheme.Crimson);
		cancelBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); cancelBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		cancelBtn.Pressed += () => { _recruitPopup.Hide(); GM.CancelRecruit(); };
		var bc2 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc2.AddChild(cancelBtn); rv.AddChild(bc2); rv.AddChild(SP(8));
	}

	void BuildSmartAssignPopup()
	{
		_smartPopup = new Window { Title = "智能安排弟子", Size = new Vector2I(500, 400), Visible = false, Exclusive = true, Unresizable = true };
		_smartPopup.CloseRequested += () => _smartPopup.Hide(); AddChild(_smartPopup);
		var sv = new VBoxContainer(); sv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _smartPopup.AddChild(sv);
		sv.AddChild(SP(10)); sv.AddChild(HL("智能安排策略", 18, UITheme.Gold)); sv.AddChild(SP(10));
		var presetGrid = new GridContainer { Columns = 2 }; var pc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; pc.AddChild(presetGrid); sv.AddChild(pc);

		void addPreset(string label, string desc, Action action)
		{
			var btn = new Button { Text = $"{label}\n{desc}", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(200, 50) };
			btn.AddThemeFontSizeOverride("font_size", 12); btn.AddThemeColorOverride("font_color", UITheme.TextPrimary); btn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
			btn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); btn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			btn.Pressed += () => { action(); _smartPopup.Hide(); RefreshDisciples(); };
			presetGrid.AddChild(btn);
		}

		addPreset("专精优先", "按弟子最高专长分配", () => ApplySmart("proficiency"));
		addPreset("修炼为重", "全体弟子潜心修炼", () => ApplySmart("cultivate"));
		addPreset("资源均衡", "采集/炼丹/炼器均匀分布", () => ApplySmart("balanced"));
		addPreset("轮换休整", "低体力/低心情者休息", () => ApplySmart("rest"));

		sv.AddChild(SP(8));
		var closeBtn = SmallBtn("合上"); closeBtn.Pressed += () => _smartPopup.Hide();
		var cc22 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc22.AddChild(closeBtn); sv.AddChild(cc22); sv.AddChild(SP(8));
	}

	// ===================== TAB SWITCHING =====================

	void SwitchToTab(int idx)
	{
		_activeTab = idx;
		for (int i = 0; i < _sidebarBtns.Length; i++)
		{
			bool active = i == idx;
			_sidebarBtns[i].AddThemeColorOverride("font_color", active ? UITheme.Gold : UITheme.TextPrimary);
			_sidebarBtns[i].AddThemeStyleboxOverride("normal", active ? UITheme.SidebarBtnActive() : UITheme.SidebarBtnNormal());
		}
		var content = _tabContents[idx]; content.FreeChildren();
		if (_contentScroll.GetChildCount() > 0) _contentScroll.RemoveChild(_contentScroll.GetChild(0));
		_contentScroll.AddChild(content);
		RefreshTabContent(idx); UIAnimator.TabSwitch(content);
	}

	void RefreshTabContent(int idx)
	{
		switch (idx) { case 0: RefreshOverview(); break; case 1: RefreshDisciples(); break; case 2: RefreshBuild(); break; case 3: RefreshFacilities(); break; case 4: RefreshCompanions(); break; case 5: RefreshQuests(); break; case 6: RefreshLog(); break; case 7: RefreshStats(); break; }
	}

	// ===================== SIGNALS =====================

	void ConnectSignals()
	{
		EventBus.DayPassed += OnDayPassed; EventBus.ResourceChanged += OnResourceChanged;
		EventBus.EventChoiceRequired += OnEventTriggered; EventBus.GameNotification += OnGameNotification;
		EventBus.DiscipleRecruited += OnDiscipleChanged; EventBus.DiscipleDeparted += OnDiscipleChanged;
		EventBus.RecruitSelectionReady += ShowRecruitSelection;
	}
	void OnDayPassed(int _, int __, int ___) => RefreshAll();
	void OnResourceChanged(ResourceType _, int __, int ___) => RefreshResources();
	void OnGameNotification(string t, string m)
	{
		if (t == "启禀") { _hintLabel.Text = m; _hintPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_hintPopup.GetChild(0)); }
		if (t == "宗门倾覆") { _gameOverPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_gameOverPopup.GetChild(0)); }
	}
	void OnDiscipleChanged(DiscipleData _) { RefreshDisciples(); RefreshOverview(); }

	// ===================== REFRESH =====================

	void RefreshAll() { if (!IsInsideTree()) return; RefreshTime(); RefreshResources(); RefreshSectInfo(); RefreshTournament(); RefreshTabContent(_activeTab); RefreshBgmIndicator(); RefreshButtons(); }
	void RefreshTime() => _timeLabel.Text = GM.Time.GetDateString();
	void RefreshSectInfo() => _sectLabel.Text = $"{GM.FullSectName} Lv.{GM.SectLevel}";
	void RefreshButtons() { _nextDayBtn.Disabled = GM.PendingEvent != null || GM.PendingRecruitCandidates != null; }
	void RefreshTournament()
	{
		if (GM.RecruitTournamentDays > 0) { _tournamentLabel.Visible = true; _tournamentLabel.Text = $" | 大比之期尚余 {GM.RecruitTournamentDays} 日"; }
		else _tournamentLabel.Visible = false;
	}
	void RefreshResources()
	{
		var m = new Dictionary<ResourceType, (string, Color)>
		{
			[ResourceType.SpiritStone] = ("灵石", UITheme.Gold), [ResourceType.Herb] = ("灵草", new Color(0.3f, 0.8f, 0.3f)),
			[ResourceType.Ore] = ("矿石", new Color(0.7f, 0.6f, 0.4f)), [ResourceType.Pill] = ("丹药", new Color(0.9f, 0.4f, 0.6f)),
			[ResourceType.Equipment] = ("法器", new Color(0.5f, 0.5f, 1.0f)), [ResourceType.Contribution] = ("贡献", new Color(0.8f, 0.8f, 0.3f)),
			[ResourceType.SpiritEssence] = ("灵气", new Color(0.4f, 0.7f, 1.0f)),
		};
		foreach (var kv in _resourceLabels) { int a = GM.Resources.Get(kv.Key), ic = GM.Resources.GetIncome(kv.Key); if (m.TryGetValue(kv.Key, out var nc)) { kv.Value.Text = $"{nc.Item1}:{a}{(ic != 0 ? $"({ic:+0;-0})" : "")}"; kv.Value.AddThemeColorOverride("font_color", nc.Item2); } }
	}

	// ===================== TAB: OVERVIEW (0) =====================

	void RefreshOverview()
	{
		var c = _tabContents[0]; c.FreeChildren();
		c.AddChild(HL($"{GM.FullSectName}", 24, UITheme.Gold));
		c.AddChild(TB($"Lv.{GM.SectLevel} · {SectTitleDesc(GM.SectReputation)} · 声望{GM.SectReputation} · 战力{GM.SectPower}", UITheme.TextDim, 13));
		c.AddChild(SP(14));

		var statGrid = new GridContainer { Columns = 4 }; c.AddChild(CenteredGrid(statGrid));
		StatCard(statGrid, "宗门等级", $"Lv.{GM.SectLevel}", UITheme.Gold); StatCard(statGrid, "声望", GM.SectReputation.ToString(), UITheme.TextBlue);
		StatCard(statGrid, "战力", GM.SectPower.ToString(), UITheme.TextOrange); StatCard(statGrid, "弟子", $"{GM.Disciples.Count}/{GM.MaxDisciples}", UITheme.TextGreen);
		StatCard(statGrid, "灵筑", $"{GM.Facilities.AllFacilities.Count(f => f.IsBuilt)}座", UITheme.TextGreen);
		StatCard(statGrid, "营造中", $"{GM.Facilities.AllFacilities.Count(f => f.IsUnderConstruction)}座", UITheme.TextOrange);
		StatCard(statGrid, "道缘", $"{GM.Companions.AllCompanions.Count(c => c.IsMarried)}对", new Color(1.0f, 0.45f, 0.65f));
		StatCard(statGrid, "法器库存", $"{GM.AllEquipment.Count(e => e.EquippedById < 0)}件", UITheme.TextBlue);
		c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));

		c.AddChild(HL("资源储备", 16, UITheme.Gold)); c.AddChild(SP(6));
		var resGrid = new GridContainer { Columns = 4 }; c.AddChild(CenteredGrid(resGrid));
		foreach (ResourceType rt in Enum.GetValues<ResourceType>()) { int val = GM.Resources.Get(rt); int inc = GM.Resources.GetIncome(rt); string text = inc > 0 ? $"{ResName(rt)}: {val} (+{inc}/d)" : $"{ResName(rt)}: {val}"; var lb = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center }; lb.AddThemeFontSizeOverride("font_size", 13); lb.AddThemeColorOverride("font_color", UITheme.TextPrimary); resGrid.AddChild(lb); }
		c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));

		c.AddChild(HL("弟子分工", 16, UITheme.Gold)); c.AddChild(SP(6));
		var taskCounts = new int[9]; foreach (var d in GM.Disciples.AllDisciples) taskCounts[(int)d.CurrentTask]++;
		var taskLine = ""; for (int i = 0; i < TaskNames.Length; i++) if (taskCounts[i] > 0) taskLine += $"{TaskNames[i]}×{taskCounts[i]}  ";
		if (taskLine == "") taskLine = "暂无分工";
		var tl = new Label { Text = taskLine, HorizontalAlignment = HorizontalAlignment.Center }; tl.AddThemeFontSizeOverride("font_size", 13); tl.AddThemeColorOverride("font_color", UITheme.TextPrimary); c.AddChild(tl); c.AddChild(SP(4));
		var incLine = "每日产出: "; foreach (ResourceType rt in Enum.GetValues<ResourceType>()) { int inc2 = GM.Resources.GetIncome(rt); if (inc2 > 0) incLine += $"【{ResName(rt)} +{inc2}】 "; }
		if (incLine == "每日产出: ") incLine += "暂无";
		c.AddChild(new Label { Text = incLine, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextGreen));
	}

	// ===================== TAB: DISCIPLES (1) =====================

	void RefreshDisciples()
	{
		var c = _tabContents[1]; c.FreeChildren();
		c.AddChild(HL($"弟子名录（{GM.Disciples.Count}/{GM.MaxDisciples}人）", 18, UITheme.Gold)); c.AddChild(SP(10));
		if (GM.Disciples.Count == 0) { c.AddChild(TB("尚无弟子。点击底部「入门大比」举办选拔大会（七日后举行）。", UITheme.TextDim, 13)); return; }

		// Batch controls
		_batchChecks.Clear();
		var batchBar = new HBoxContainer(); var bc1 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; batchBar.AddChild(bc1);
		var selAll = SmallBtn("全选"); selAll.Pressed += () => { foreach (var kv in _batchChecks) kv.Value.ButtonPressed = true; }; bc1.AddChild(selAll);
		bc1.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
		var deselAll = SmallBtn("全不选"); deselAll.Pressed += () => { foreach (var kv in _batchChecks) kv.Value.ButtonPressed = false; }; bc1.AddChild(deselAll);
		bc1.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
		bc1.AddChild(new Label { Text = "批量设为:", VerticalAlignment = VerticalAlignment.Center }.WithFont(11, UITheme.TextDim));
		_batchDrop = new OptionButton(); _batchDrop.AddThemeFontSizeOverride("font_size", 11); foreach (var tn in TaskNames) _batchDrop.AddItem(tn); bc1.AddChild(_batchDrop);
		bc1.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
		var applyBtn = SmallBtn("执行"); applyBtn.Pressed += ApplyBatchAssign; bc1.AddChild(applyBtn);
		c.AddChild(batchBar); c.AddChild(SP(4));

		// Auto-assign row (separate line)
		var autoRow = new HBoxContainer();
		var ac = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; autoRow.AddChild(ac);
		if (GM.CanAutoAssign)
		{
			var autoToggle = new CheckBox { ButtonPressed = GM.AutoAssignEnabled };
			autoToggle.AddThemeFontSizeOverride("font_size", 11);
			autoToggle.Toggled += (on) => { GM.AutoAssignEnabled = on; };
			ac.AddChild(autoToggle);
			ac.AddChild(new Control { CustomMinimumSize = new Vector2I(3, 0) });
			ac.AddChild(new Label { Text = "自动安排", VerticalAlignment = VerticalAlignment.Center }.WithFont(11, UITheme.TextGreen));
			ac.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
			var stratBtn = SmallBtn("策略"); stratBtn.AddThemeColorOverride("font_color", UITheme.Gold);
			stratBtn.Pressed += () => { _smartPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_smartPopup.GetChild(0)); };
			ac.AddChild(stratBtn);
		}
		else
		{
			ac.AddChild(new Label { Text = "🔒 修建藏经阁并达到Lv.2可解锁自动安排" }.WithFont(10, UITheme.TextDim));
		}
		c.AddChild(autoRow);
		c.AddChild(SP(8));

		var cardGrid = new GridContainer { Columns = 3 }; c.AddChild(cardGrid);
		foreach (var d in GM.Disciples.AllDisciples)
		{
			int did = d.Id; var card = MakeCard(220); var cv = (VBoxContainer)card.GetChild(0);
			// Checkbox
			var cb = new CheckBox { SizeFlagsHorizontal = SizeFlags.ExpandFill }; _batchChecks[did] = cb;
			var checkRow = new HBoxContainer(); checkRow.AddChild(cb); cv.AddChild(checkRow);
			// Avatar
			var avatar = MakeAvatarCircle(d.IsMale, 56); var avCenter = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; avCenter.AddChild(avatar); cv.AddChild(avCenter); cv.AddChild(SP(3));
			// Name
			string gi = d.IsMale ? "♂" : "♀"; bool isChild = d.Skills.ContainsKey(10);
			string rootTag = d.SpiritRoot != SpiritualRoot.None ? $" [{d.SpiritRootName}]" : "";
			string title2 = isChild ? $"{gi}{d.Name} 🔯{rootTag}" : $"{gi}{d.Name}{rootTag}";
			var nl = new Button { Text = title2, Alignment = HorizontalAlignment.Center }; nl.AddThemeFontSizeOverride("font_size", 15); nl.AddThemeColorOverride("font_color", UITheme.Gold); nl.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1)); nl.AddThemeStyleboxOverride("normal", new StyleBoxEmpty()); nl.AddThemeStyleboxOverride("hover", new StyleBoxEmpty()); nl.Flat = true; var capD = d; nl.Pressed += () => ShowDiscipleDetail(capD); cv.AddChild(nl);
			cv.AddChild(new Label { Text = d.FullRealmName, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, RealmColor(d.Realm)));
			// Flavor
			var fp = new List<string>(); if (!string.IsNullOrEmpty(d.Background)) fp.Add(d.Background); if (!string.IsNullOrEmpty(d.Personality)) fp.Add(d.Personality); if (!string.IsNullOrEmpty(d.Trait) && d.Trait != "无") fp.Add(d.Trait);
			if (fp.Count > 0) cv.AddChild(new Label { Text = string.Join(" · ", fp), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, new Color(0.65f, 0.55f, 0.75f)));
			// Companion/breakthrough
			if (d.CompanionId >= 0) { var comp2 = GM.Companions.Get(d.CompanionId); if (comp2 != null) { var cl = new Label { Text = comp2.IsMarried ? "💍 道缘" : "❤ 结缘中", HorizontalAlignment = HorizontalAlignment.Center }; cl.AddThemeFontSizeOverride("font_size", 10); cl.AddThemeColorOverride("font_color", comp2.IsMarried ? new Color(1, 0.5f, 0.7f) : new Color(1, 0.7f, 0.4f)); cv.AddChild(cl); } }
			if (d.IsInBreakthrough) { var bl = new Label { Text = "⚠ 突破中", HorizontalAlignment = HorizontalAlignment.Center }; bl.AddThemeFontSizeOverride("font_size", 11); bl.AddThemeColorOverride("font_color", UITheme.TextOrange); cv.AddChild(bl); }
			cv.AddChild(SP(3));
			// Stats
			cv.AddChild(new Label { Text = $"天赋{d.Talent} 悟性{d.Comprehension} 体质{d.Constitution} 神识{d.Spirit}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
			cv.AddChild(new Label { Text = $"战力{d.CombatPower}  修为{d.CultivationProgress:F0}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextPrimary));
			string mb = BarsString(d.Mood, 100, 10), sb = BarsString(d.CurrentStamina, d.MaxStamina, 10);
			var cond = new Label { Text = $"心情{mb} {d.Mood:F0}  体力{sb} {d.CurrentStamina}/{d.MaxStamina}", HorizontalAlignment = HorizontalAlignment.Center }; cond.AddThemeFontSizeOverride("font_size", 10); cond.AddThemeColorOverride("font_color", d.Mood < 20 || d.Loyalty < 20 ? UITheme.Crimson : d.CurrentStamina <= 10 ? UITheme.TextOrange : UITheme.TextDim); cv.AddChild(cond);
			if (d.Skills.Count > 0) cv.AddChild(new Label { Text = string.Join(" ", d.Skills.Select(s => $"{SkillName(s.Key)}Lv.{s.Value}")), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextDim));
			cv.AddChild(SP(3));
			// Task
			var taskRow = new HBoxContainer(); taskRow.AddChild(new Label { Text = "任务:" }.WithFont(11, UITheme.TextDim));
			var drop = new OptionButton(); drop.AddThemeFontSizeOverride("font_size", 10); drop.SizeFlagsHorizontal = SizeFlags.ExpandFill; foreach (var tn in TaskNames) drop.AddItem(tn); drop.Select((int)d.CurrentTask);
			drop.ItemSelected += (idx) => { Callable.From(() => { GM.AssignTask(did, (DiscipleTaskType)(int)idx); }).CallDeferred(); }; taskRow.AddChild(drop); cv.AddChild(taskRow);
			cv.AddChild(SP(3));
			var disBtn = SmallBtn("遣散"); disBtn.Pressed += () => GM.DismissDisciple(did); var dc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; dc.AddChild(disBtn); cv.AddChild(dc);
			cardGrid.AddChild(card);
		}
	}

	// ===================== TAB: BUILD (2) =====================

	void RefreshBuild()
	{
		var c = _tabContents[2]; c.FreeChildren();
		c.AddChild(HL($"营造灵筑（Lv.{GM.SectLevel} 上限{GM.SectLevel * 2}座）", 18, UITheme.Gold)); c.AddChild(SP(10));
		var facilities = Enum.GetValues<FacilityType>().Select(ft => (type: ft, info: FacilityTable.GetInfo(ft))).OrderBy(f => f.info.MinSectLevel > GM.SectLevel ? 1 : 0).ToList();
		var cardGrid = new GridContainer { Columns = 2 }; var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(cardGrid); c.AddChild(cc);
		foreach (var (ft, info) in facilities)
		{
			bool locked = GM.SectLevel < info.MinSectLevel; var card = MakeCard(380); var cv = (VBoxContainer)card.GetChild(0);
			var facTex = SpriteSheetManager.GetFacilityIcon(ft); var ir = new TextureRect { Texture = facTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(72, 72) }; var ic = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ic.AddChild(ir); cv.AddChild(ic); cv.AddChild(SP(6));
			cv.AddChild(new Label { Text = info.Name, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(18, locked ? UITheme.TextDim : UITheme.Gold));
			if (locked) { cv.AddChild(SP(4)); cv.AddChild(new Label { Text = $"🔒 需宗门Lv.{info.MinSectLevel}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.Crimson)); cv.AddChild(SP(4)); cv.AddChild(new Label { Text = info.Description, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim)); }
			else {
			cv.AddChild(SP(4));
			cv.AddChild(new Label { Text = info.Description, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
			cv.AddChild(SP(4));
			cv.AddChild(new Label { Text = $"{info.BaseBuildCost}灵石  ·  {info.BuildDays}日竣工", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
			cv.AddChild(SP(6));
			var btn = new Button { Text = "营 造", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(160, 40) };
			btn.AddThemeFontSizeOverride("font_size", 15); btn.AddThemeColorOverride("font_color", UITheme.TextPrimary); btn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
			btn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); btn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			var f2 = ft; btn.Pressed += () => GM.StartBuild(f2);
			var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(btn); cv.AddChild(bc);
		}
			cardGrid.AddChild(card);
		}
	}

	// ===================== TAB: FACILITIES (3) =====================

	void RefreshFacilities()
	{
		var c = _tabContents[3]; c.FreeChildren();
		c.AddChild(HL($"灵筑管理（{GM.Facilities.Count}座）", 18, UITheme.Gold)); c.AddChild(SP(10));
		if (GM.Facilities.Count == 0) { c.AddChild(TB("尚未营造灵筑。切换到「营造」页开始建设。", UITheme.TextDim, 13)); return; }
		var cardGrid = new GridContainer { Columns = 3 }; c.AddChild(cardGrid);
		foreach (var f in GM.Facilities.AllFacilities.OrderByDescending(f => f.Level))
		{
			var card = MakeCard(220); var cv = (VBoxContainer)card.GetChild(0);
			var facTex = SpriteSheetManager.GetFacilityIcon(f.Type); var ir = new TextureRect { Texture = facTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(44, 44) }; var ic = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ic.AddChild(ir); cv.AddChild(ic); cv.AddChild(SP(4));
			cv.AddChild(new Label { Text = $"{f.TypeName} Lv.{f.Level}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(15, UITheme.Gold));
			cv.AddChild(new Label { Text = f.StatusText, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, f.IsBuilt ? UITheme.TextGreen : f.IsUnderConstruction ? UITheme.TextOrange : UITheme.TextDim));
			cv.AddChild(SP(4));
			if (f.IsBuilt) { cv.AddChild(new Label { Text = $"容纳{f.MaxDisciples}人  产{FacilityTable.GetOutput(f.Type, f.Level)}/d", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextPrimary)); if (f.Level < f.MaxLevel) { int cost = FacilityTable.GetUpgradeCost(f.Type, f.Level); var ub = SmallBtn($"晋升 {cost}灵石"); int fid = f.Id; ub.Pressed += () => GM.UpgradeFacility(fid); var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(ub); cv.AddChild(bc); } else cv.AddChild(new Label { Text = "已满级", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim)); }
			else if (f.IsUnderConstruction) { string sl = f.IsUpgrading ? "晋升中" : "营造中"; cv.AddChild(new Label { Text = $"{sl} {f.ConstructionProgress}/{FacilityTable.GetInfo(f.Type).BuildDays}日", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextOrange)); }
			cardGrid.AddChild(card);
		}
	}

	// ===================== TAB: COMPANIONS (4) =====================

	void RefreshCompanions()
	{
		var c = _tabContents[4]; c.FreeChildren(); var allComps = GM.Companions.AllCompanions;
		c.AddChild(HL($"道缘谱（{allComps.Count}对）", 18, UITheme.Gold)); c.AddChild(SP(10));
		if (allComps.Count > 0) { var cg = new GridContainer { Columns = 2 }; c.AddChild(cg);
			foreach (var comp in allComps) { var d1 = GM.Disciples.Get(comp.DiscipleId1); var d2 = GM.Disciples.Get(comp.DiscipleId2); if (d1 == null || d2 == null) continue; var card = MakeCard(300); var cv = (VBoxContainer)card.GetChild(0);
				var avRow = new HBoxContainer(); var ca1 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; avRow.AddChild(ca1); ca1.AddChild(MakeAvatarCircle(d1.IsMale, 44)); var hl2 = new Label { Text = " ❤ ", VerticalAlignment = VerticalAlignment.Center }; hl2.AddThemeFontSizeOverride("font_size", 18); hl2.AddThemeColorOverride("font_color", new Color(1, 0.4f, 0.5f)); avRow.AddChild(hl2); var ca2 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; avRow.AddChild(ca2); ca2.AddChild(MakeAvatarCircle(d2.IsMale, 44)); cv.AddChild(avRow); cv.AddChild(SP(4));
				string g1 = d1.IsMale ? "♂" : "♀", g2 = d2.IsMale ? "♂" : "♀"; cv.AddChild(new Label { Text = $"{g1}{d1.Name}  &  {g2}{d2.Name}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.Gold));
				cv.AddChild(new Label { Text = comp.IsMarried ? "💍 已结道缘" : "未结道缘", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, comp.IsMarried ? new Color(1, 0.6f, 0.8f) : UITheme.TextDim));
				cv.AddChild(new Label { Text = $"好感度: {comp.Affection:F0}/100" }.WithFont(11, UITheme.TextPrimary));
				var bar = new ColorRect { CustomMinimumSize = new Vector2I((int)(comp.Affection * 2), 5) }; bar.Color = comp.Affection >= 80 ? new Color(1, 0.3f, 0.6f) : comp.Affection >= 60 ? new Color(1, 0.6f, 0.2f) : new Color(0.5f, 0.5f, 0.8f); cv.AddChild(bar);
				bool bc2 = d1.CurrentTask == DiscipleTaskType.Cultivate && d2.CurrentTask == DiscipleTaskType.Cultivate; cv.AddChild(new Label { Text = bc2 ? $"双修加成: +{comp.DualCultivationBonus * 100:F0}% (生效中)" : $"双修加成: +{comp.DualCultivationBonus * 100:F0}%", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, bc2 ? UITheme.TextGreen : UITheme.TextDim));
				cv.AddChild(SP(4));
				var br = new HBoxContainer(); var bcr = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; br.AddChild(bcr); int cId = comp.Id;
				var g1b = SmallBtn("赠丹药"); g1b.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.Pill, 1); bcr.AddChild(g1b); var g2b = SmallBtn("赠灵石"); g2b.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.SpiritStone, 50); bcr.AddChild(g2b);
				if (!comp.IsMarried) { var mb = SmallBtn("结道缘"); mb.Disabled = comp.Affection < 60; mb.Pressed += () => GM.ProposeMarriage(cId); bcr.AddChild(mb); }
				var bb = SmallBtn("和离"); bb.Pressed += () => GM.BreakUpCompanion(cId); bcr.AddChild(bb); cv.AddChild(br); cg.AddChild(card); }
			c.AddChild(SP(14)); } else { c.AddChild(TB("尚无道缘。使用下方功能为弟子牵线搭桥。", UITheme.TextDim, 13)); c.AddChild(SP(10)); }

		c.AddChild(HL("牵线搭桥", 16, UITheme.Gold)); c.AddChild(SP(6));
		var singles = GM.Disciples.AllDisciples.Where(d => d.CompanionId < 0).ToList(); var males = singles.Where(d => d.IsMale).ToList(); var females = singles.Where(d => !d.IsMale).ToList();
		if (males.Count == 0 || females.Count == 0) c.AddChild(TB(males.Count + females.Count == 0 ? "暂无单身弟子可牵线。" : "性别比例失衡，需更多异性弟子。", UITheme.TextDim, 12));
		else { var mr = new HBoxContainer(); mr.AddChild(new Label { Text = "选择两位单身弟子:" }.WithFont(12, UITheme.TextPrimary)); _matchMaleDrop = new OptionButton(); foreach (var m in males) _matchMaleDrop.AddItem($"♂{m.Name} ({m.FullRealmName})"); mr.AddChild(_matchMaleDrop); mr.AddChild(new Label { Text = " + " }.WithFont(13, UITheme.Gold)); _matchFemaleDrop = new OptionButton(); foreach (var f in females) _matchFemaleDrop.AddItem($"♀{f.Name} ({f.FullRealmName})"); mr.AddChild(_matchFemaleDrop); var ib = SmallBtn("牵线"); ib.Pressed += () => { if (_matchMaleDrop.Selected < 0 || _matchFemaleDrop.Selected < 0) return; GM.IntroduceCompanions(males[_matchMaleDrop.Selected].Id, females[_matchFemaleDrop.Selected].Id); }; mr.AddChild(ib); c.AddChild(mr); c.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 4) }); c.AddChild(TB("成功率受境界相近度、属性互补、年龄相仿、忠诚度影响。", UITheme.TextDim, 11)); }
	}

	// ===================== TAB: QUESTS (5) =====================

	void RefreshQuests()
	{
		var c = _tabContents[5]; c.FreeChildren(); var completed = GM.Quests.AllQuests.Count(q => q.Completed);
		c.AddChild(HL($"宗门令（{completed}/{GM.Quests.AllQuests.Count}完成）", 18, UITheme.Gold)); c.AddChild(SP(10));
		if (GM.Quests.AllQuests.Count == 0) { c.AddChild(TB("暂无宗门令。", UITheme.TextDim, 13)); return; }
		var cg = new GridContainer { Columns = 2 }; c.AddChild(cg);
		foreach (var q in GM.Quests.AllQuests) { var card = MakeCard(300); var cv = (VBoxContainer)card.GetChild(0); cv.AddChild(new Label { Text = q.Completed ? $"✓ {q.Title}" : q.Title, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, q.Completed ? UITheme.TextGreen : UITheme.Gold)); cv.AddChild(new Label { Text = q.Description, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextPrimary)); cv.AddChild(new Label { Text = q.ProgressText, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextBlue)); cv.AddChild(new Label { Text = $"奖励: {q.RewardText}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextGreen)); cg.AddChild(card); }
	}

	// ===================== TAB: LOG (6) =====================

	void RefreshLog()
	{
		var c = _tabContents[6]; c.FreeChildren();
		c.AddChild(HL("宗门记事", 18, UITheme.Gold));
		c.AddChild(SP(8));

		var entries = GM.EventLogEntries;
		var sb = new System.Text.StringBuilder();
		if (entries != null && entries.Count > 0)
		{
			int lastDay = -1;
			foreach (var e in entries)
			{
				if (e.Day != lastDay)
				{
					sb.AppendLine("[color=#e8b83a]━━ 第" + e.Day + "日 ━━[/color]");
					lastDay = e.Day;
				}
				string color = GetLogColor(e.Message);
				sb.AppendLine("  " + color + e.Message + "[/color]");
			}
		}
		else
		{
			sb.AppendLine("[color=#888888]宗门初立，尚无记事。推演天时以记录宗门变迁。[/color]");
		}

		var logLabel = new RichTextLabel
		{
			BbcodeEnabled = true,
			Text = sb.ToString(),
			FitContent = true,
			ScrollActive = false,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
		};
		logLabel.AddThemeFontSizeOverride("normal_font_size", 12);
		logLabel.AddThemeColorOverride("default_color", UITheme.TextPrimary);
		c.AddChild(logLabel);
	}

	static string GetLogColor(string msg)
	{
		if (msg.Contains("加入") || msg.Contains("慕名而来")) return "[color=#55cc55]";
		if (msg.Contains("离开") || msg.Contains("离去") || msg.Contains("遣散")) return "[color=#cc5555]";
		if (msg.Contains("突破")) return "[color=#e8b83a]";
		if (msg.Contains("营造") || msg.Contains("升级") || msg.Contains("竣工") || msg.Contains("晋升")) return "[color=#55aacc]";
		if (msg.Contains("创立") || msg.Contains("宗门晋升")) return "[color=#e8b83a]";
		if (msg.Contains("入门大比") || msg.Contains("大比")) return "[color=#e8903a]";
		if (msg.Contains("是日") || msg.Contains("灵石")) return "[color=#888888]";
		if (msg.Contains("道缘") || msg.Contains("结缘") || msg.Contains("牵线")) return "[color=#cc88cc]";
		return "[color=#cccccc]";
	}

	// ===================== TAB: STATS (7) =====================
	// ===================== TAB: STATS (7) =====================
	// ===================== TAB: STATS (7) =====================

	void RefreshStats()
	{
		var c = _tabContents[7]; c.FreeChildren(); c.AddChild(HL("宗门卷宗", 18, UITheme.Gold)); c.AddChild(SP(10));
		var grid = new GridContainer { Columns = 3 }; c.AddChild(CenteredGrid(grid));
		StatCard(grid, "宗门等级", $"Lv.{GM.SectLevel}", UITheme.Gold); StatCard(grid, "宗门称号", GM.SectTitle, UITheme.Gold); StatCard(grid, "声望", GM.SectReputation.ToString(), UITheme.TextBlue); StatCard(grid, "战力", GM.SectPower.ToString(), UITheme.TextOrange); StatCard(grid, "弟子数", $"{GM.Disciples.Count}/{GM.MaxDisciples}", UITheme.TextGreen); StatCard(grid, "灵筑数", GM.Facilities.Count.ToString(), UITheme.TextGreen); StatCard(grid, "道缘对数", GM.Companions.AllCompanions.Count(c => c.IsMarried).ToString(), new Color(1, 0.5f, 0.7f)); StatCard(grid, "法器库存", GM.AllEquipment.Count.ToString(), UITheme.TextBlue); StatCard(grid, "历经时日", GM.Time.GetTotalDays().ToString(), UITheme.TextDim);
		c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));
		c.AddChild(HL("境界分布", 16, UITheme.Gold)); c.AddChild(SP(6));
		var rc = new Dictionary<CultivationRealm, int>(); foreach (var d in GM.Disciples.AllDisciples) { rc.TryGetValue(d.Realm, out int cnt); rc[d.Realm] = cnt + 1; }
		foreach (var kv in rc.OrderByDescending(kv => (int)kv.Key)) c.AddChild(new Label { Text = $"{kv.Key}: {kv.Value}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, RealmColor(kv.Key)));
		c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));
		c.AddChild(HL("宗门令分布", 16, UITheme.Gold)); c.AddChild(SP(6));
		var tc = new int[9]; foreach (var d in GM.Disciples.AllDisciples) tc[(int)d.CurrentTask]++; for (int i = 0; i < TaskNames.Length; i++) if (tc[i] > 0) c.AddChild(new Label { Text = $"{TaskNames[i]}: {tc[i]}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextPrimary));
	}

	// ===================== RECRUIT / SMART / BATCH =====================

	void ShowRecruitSelection(List<DiscipleData> candidates)
	{
		_candidateRow.FreeChildren();
		foreach (var d in candidates)
		{
			var card = MakeCard(155); var cv = (VBoxContainer)card.GetChild(0);
			var av = MakeAvatarCircle(d.IsMale, 48); var ac = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ac.AddChild(av); cv.AddChild(ac); cv.AddChild(SP(3));
			string gi = d.IsMale ? "♂" : "♀"; cv.AddChild(new Label { Text = $"{gi} {d.Name}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(15, UITheme.Gold)); cv.AddChild(new Label { Text = $"{d.Age}岁", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim)); cv.AddChild(SP(3));
			cv.AddChild(new Label { Text = $"身世: {d.Background}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextPrimary)); cv.AddChild(new Label { Text = $"性格: {d.Personality}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextPrimary));
			if (d.Trait != "无") cv.AddChild(new Label { Text = $"天赋: {d.Trait}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextOrange));
			cv.AddChild(SP(3));
			StatLine(cv, "天赋", d.Talent, UITheme.Gold); StatLine(cv, "悟性", d.Comprehension, UITheme.TextBlue); StatLine(cv, "体质", d.Constitution, UITheme.TextGreen); StatLine(cv, "神识", d.Spirit, new Color(0.7f, 0.3f, 1.0f));
			cv.AddChild(SP(2));
			Color rc2 = d.SpiritRoot switch { SpiritualRoot.Heavenly => UITheme.Gold, SpiritualRoot.SingleElement => new Color(1.0f, 0.5f, 0.3f), SpiritualRoot.DualElement => new Color(0.7f, 0.3f, 1.0f), SpiritualRoot.Special => new Color(0.3f, 0.8f, 1.0f), _ => UITheme.TextDim };
			cv.AddChild(new Label { Text = $"灵根: {d.SpiritRootName}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, rc2)); cv.AddChild(SP(4));
			var selBtn = new Button { Text = "选 择", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(120, 32) }; selBtn.AddThemeFontSizeOverride("font_size", 13); selBtn.AddThemeColorOverride("font_color", UITheme.TextPrimary); selBtn.AddThemeColorOverride("font_hover_color", UITheme.Gold); selBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); selBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); var cp = d; selBtn.Pressed += () => { _recruitPopup.Hide(); GM.ConfirmRecruit(cp); RefreshAll(); }; var sbc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; sbc.AddChild(selBtn); cv.AddChild(sbc);
			_candidateRow.AddChild(card); _candidateRow.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
		}
		_recruitPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_recruitPopup.GetChild(0));
	}

	void ApplySmart(string strategy)
	{
		var list = GM.Disciples.AllDisciples.ToList();
		switch (strategy)
		{
			case "proficiency": foreach (var d in list) { var best = d.TaskProficiency.Count > 0 ? d.TaskProficiency.OrderByDescending(kv => kv.Value).First().Key : (d.EffTalent >= d.EffConstitution ? DiscipleTaskType.Cultivate : DiscipleTaskType.Train); GM.AssignTask(d.Id, best); } break;
			case "cultivate": foreach (var d in list) GM.AssignTask(d.Id, DiscipleTaskType.Cultivate); break;
			case "balanced": int n = list.Count; for (int i = 0; i < n; i++) { var t = (i % 4) switch { 0 => DiscipleTaskType.Gather, 1 => DiscipleTaskType.Alchemy, 2 => DiscipleTaskType.Craft, _ => DiscipleTaskType.Cultivate }; GM.AssignTask(list[i].Id, t); } break;
			case "rest": foreach (var d in list) { if (d.CurrentStamina < d.MaxStamina / 2 || d.Mood < 30 || d.Health < d.MaxHealth / 2) GM.AssignTask(d.Id, DiscipleTaskType.Rest); } break;
		}
	}

	void ApplyBatchAssign()
	{
		if (_batchDrop == null) return; int sel = _batchDrop.Selected; if (sel < 0) return; var task = (DiscipleTaskType)sel; int count = 0;
		foreach (var d in GM.Disciples.AllDisciples) { if (_batchChecks.TryGetValue(d.Id, out var cb) && cb.ButtonPressed) { GM.AssignTask(d.Id, task); count++; } }
		RefreshDisciples();
	}

	// ===================== EVENT / DETAIL / SAVE / LOAD =====================

	void OnEventTriggered(EventData e) { _eventPopup.Title = e.Title; _eventTitle.Text = e.Title; _eventDesc.Text = e.Description; _eventChoice1.Text = e.Choice1Text; _eventChoice1.Show(); _eventEffect1.Text = FormatOutcome(e.Choice1Outcome); _eventEffect1.Show(); _eventChoice2.Text = e.Choice2Text; _eventChoice2.Show(); _eventEffect2.Text = FormatOutcome(e.Choice2Outcome); _eventEffect2.Show(); _eventChoice3.Text = string.IsNullOrEmpty(e.Choice3Text) ? "无视" : e.Choice3Text; _eventChoice3.Show(); _eventEffect3.Text = e.Choice3Outcome != null ? FormatOutcome(e.Choice3Outcome) : ""; _eventEffect3.Show(); _dismissedBtn.Hide(); _eventPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_eventPopup.GetChild(0)); }
	void ResolveChoice(int i) { _eventChoice1.Hide(); _eventChoice2.Hide(); _eventChoice3.Hide(); _eventEffect1.Hide(); _eventEffect2.Hide(); _eventEffect3.Hide(); _dismissedBtn.Show(); GM.ResolveEvent(i); _dismissedBtn.Text = "合上"; }
	void DismissEventPopup() { _eventPopup.Hide(); GM.DismissEvent(); RefreshAll(); }
	public override void _Input(InputEvent e) { if (!IsInsideTree()) return; if (e.IsActionPressed("ui_cancel") && _eventPopup.Visible) DismissEventPopup(); }

	void ShowDiscipleDetail(DiscipleData d)
	{
		_detailPopup.Title = $"{d.Name} · 行状";
		var davTex = SpriteSheetManager.GetAvatar(d.IsMale);
		if (davTex != null) { _detailAvatarPanel.FreeChildren(); _detailAvatarPanel.AddChild(new TextureRect { Texture = davTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, CustomMinimumSize = new Vector2I(84, 84) }); }
		else { _detailAvatarLabel.Text = d.IsMale ? "♂" : "♀"; _detailAvatarLabel.AddThemeColorOverride("font_color", d.IsMale ? new Color(0.5f, 0.7f, 1.0f) : new Color(1.0f, 0.5f, 0.7f)); }
		_detailName.Text = d.Name; _detailRealm.Text = $"境界: {d.FullRealmName}  (第{d.Age}岁 入门{d.YearsInSect}年)";
		var fp2 = new List<string>(); if (!string.IsNullOrEmpty(d.Background)) fp2.Add($"身世: {d.Background}"); if (!string.IsNullOrEmpty(d.Personality)) fp2.Add($"性格: {d.Personality}"); if (!string.IsNullOrEmpty(d.Trait) && d.Trait != "无") fp2.Add($"天赋: {d.Trait}");
		_detailAge.Text = $"灵根: {d.SpiritRootName}  天赋{d.Talent} 悟性{d.Comprehension} 体质{d.Constitution} 神识{d.Spirit}"; if (fp2.Count > 0) _detailAge.Text += $"\n{string.Join("  |  ", fp2)}";
		_detailStats.Text = $"心情 {d.Mood:F0}/100  忠诚 {d.Loyalty}/100"; _detailCombat.Text = $"战力 {d.CombatPower}  贡献 {d.TotalContribution}";
		_detailTask.Text = $"当前任务: {TaskNames[(int)d.CurrentTask]}  修为进度: {d.CultivationProgress:F0}";
		double dbonus = FacilityTable.GetTotalTaskBonus(d.CurrentTask, GM.Facilities.AllFacilities); if (dbonus > 0) _detailTask.Text += $"  设施加成: +{dbonus * 100:F0}%";
		var ri = CultivationTable.GetInfo(d.Realm); _detailProgress.Text = d.IsInBreakthrough ? "⚠ 正在突破中..." : $"突破需求: {ri.BreakthroughRequiredProgress}  当前进度: {d.CultivationProgress:F0}  (突破概率 {ri.BreakthroughChance * 100:F0}%)";
		_detailMood.Text = $"体力 {d.CurrentStamina}/{d.MaxStamina}  气血 {d.Health}/{d.MaxHealth}"; _detailStamina.Text = $"心情值影响修炼效率 {(d.Mood / 200.0 + 1.0):F2}倍";
		if (d.CompanionId >= 0) { var cp2 = GM.Companions.Get(d.CompanionId); if (cp2 != null) { var pid = cp2.DiscipleId1 == d.Id ? cp2.DiscipleId2 : cp2.DiscipleId1; var p = GM.Disciples.Get(pid); string pn = p?.Name ?? "?"; _detailStamina.Text += $"  道缘: {pn} (好感{cp2.Affection:F0})"; if (cp2.IsMarried) _detailStamina.Text += " [已结道缘]"; } }
		_detailSkills.Text = (d.Skills.Count > 0 ? $"技能: {string.Join(", ", d.Skills.Select(s => $"{SkillName(s.Key)}Lv.{s.Value}"))}" : "技能: 无") + (d.TaskProficiency.Count > 0 ? $" | 专精: {string.Join(" ", d.TaskProficiency.Where(kv => kv.Value > 0).Select(kv => $"{TaskNames[(int)kv.Key]}Lv.{kv.Value}"))}" : "");
		_equipSection.FreeChildren(); _detailEquip.Text = "";
		var eqItems = GM.AllEquipment.Where(e => d.EquipmentIds.Contains(e.Id)).ToList();
		if (eqItems.Count > 0) { _equipSection.AddChild(new Label { Text = "已装备:" }.WithFont(13, UITheme.Gold)); foreach (var eq in eqItems) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, UITheme.TextGreen)); var ub = SmallBtn("卸下"); int eqId = eq.Id; ub.Pressed += () => { GM.UnequipItem(eqId); ShowDiscipleDetail(d); RefreshDisciples(); }; row.AddChild(ub); _equipSection.AddChild(row); } }
		else _equipSection.AddChild(new Label { Text = "未装备法器" }.WithFont(11, UITheme.TextDim));
		var avail = GM.AllEquipment.Where(e => e.EquippedById < 0).ToList(); if (avail.Count > 0 && d.EquipmentIds.Count < 2) { _equipSection.AddChild(new Label { Text = "可装备:" }.WithFont(13, UITheme.Gold)); foreach (var eq in avail.Take(5)) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, UITheme.TextPrimary)); var eb = SmallBtn("装备"); int eqId2 = eq.Id; eb.Pressed += () => { GM.EquipItem(eqId2, d.Id); ShowDiscipleDetail(d); RefreshDisciples(); }; row.AddChild(eb); _equipSection.AddChild(row); } }
		_detailStats.AddThemeColorOverride("font_color", d.Mood >= 70 ? UITheme.TextGreen : d.Mood >= 40 ? UITheme.TextOrange : UITheme.Crimson);
		_detailPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_detailPopup.GetChild(0));
	}

	void RefreshSaveSlots()
	{
		_slotContainer.FreeChildren(); _saveErrorLabel.Text = ""; var slots = GM.SaveLoad.GetOccupiedSlots().ToHashSet();
		for (int i = 0; i < SaveLoadManager.MaxSlots; i++) { int si = i; var frame = new HBoxContainer(); var info = new Label { SizeFlagsHorizontal = SizeFlags.ExpandFill, HorizontalAlignment = HorizontalAlignment.Center }; info.AddThemeFontSizeOverride("font_size", 13); if (slots.Contains(i)) { var data = GM.SaveLoad.LoadFromSlot(i); info.Text = data != null ? $"卷位 {i + 1}  |  第{data.CurrentYear}年  |  {data.SectName}  |  弟子{data.Disciples.Count}人" : $"卷位 {i + 1}  |  空"; } else info.Text = $"卷位 {i + 1}  |  空"; frame.AddChild(info); var sbBtn = SmallBtn("存于此位"); sbBtn.Pressed += () => OnSaveSlot(si); frame.AddChild(sbBtn); if (slots.Contains(i)) { var db = SmallBtn("销毁"); db.Pressed += () => OnDeleteSlot(si); frame.AddChild(db); } _slotContainer.AddChild(frame); }
	}
	void OnDeleteSlot(int slot) { GM.SaveLoad.DeleteSlot(slot); _saveErrorLabel.Text = $"卷位 {slot + 1} 已销毁。"; _saveErrorLabel.AddThemeColorOverride("font_color", UITheme.TextOrange); RefreshSaveSlots(); }
	void OnSaveSlot(int slot) { GM.SaveGame(slot); _saveErrorLabel.Text = $"已存录至卷位 {slot + 1}。"; _saveErrorLabel.AddThemeColorOverride("font_color", UITheme.TextGreen); RefreshSaveSlots(); }

	void RefreshLoadSlots()
	{
		_loadSlotContainer.FreeChildren(); _loadErrorLabel.Text = ""; var slots = GM.SaveLoad.GetOccupiedSlots().ToHashSet();
		if (slots.Count == 0) { _loadSlotContainer.AddChild(new Label { Text = "  未有存录。", SizeFlagsHorizontal = SizeFlags.ExpandFill }.WithFont(13, UITheme.TextDim)); return; }
		for (int i = 0; i < SaveLoadManager.MaxSlots; i++) { if (!slots.Contains(i)) continue; int si = i; var data = GM.SaveLoad.LoadFromSlot(i); if (data == null) continue; var frame = new HBoxContainer(); var info = new Label { SizeFlagsHorizontal = SizeFlags.ExpandFill, HorizontalAlignment = HorizontalAlignment.Center }; info.AddThemeFontSizeOverride("font_size", 13); info.Text = $"卷位 {i + 1}  |  第{data.CurrentYear}年  |  {data.SectName}  |  弟子{data.Disciples.Count}人"; frame.AddChild(info); var lb = SmallBtn("开卷"); lb.Pressed += () => OnLoadSlot(si); frame.AddChild(lb); var db = SmallBtn("销毁"); db.Pressed += () => { GM.SaveLoad.DeleteSlot(si); RefreshLoadSlots(); }; frame.AddChild(db); _loadSlotContainer.AddChild(frame); }
	}
	void OnLoadSlot(int slot) { if (!GM.LoadGame(slot)) { _loadErrorLabel.Text = "开卷无果，存录可能已损坏。"; return; } _loadPopup.Hide(); RefreshAll(); }

	// ===================== BGM =====================

	void RefreshBgmIndicator() { if (AudioManager.BgmNames.Count > 0) { int idx = AudioManager.CurrentBgmIndex % AudioManager.BgmNames.Count; _bgmIndicator.Text = $"♪ {AudioManager.BgmNames[idx]}"; } }
	void RefreshBgmPopup()
	{
		var vbox = (VBoxContainer)_bgmSelectPopup.GetChild(0); vbox.FreeChildren(); vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 6) });
		vbox.AddChild(new Label { Text = "择丝竹之音", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.Gold)); vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 8) });
		for (int i = 0; i < AudioManager.BgmNames.Count; i++) { int idx = i; var isCur = i == AudioManager.CurrentBgmIndex; var btn = new Button { Text = isCur ? $"▶ {AudioManager.BgmNames[i]}" : AudioManager.BgmNames[i], Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(0, 30) }; btn.AddThemeFontSizeOverride("font_size", 12); btn.AddThemeColorOverride("font_color", isCur ? UITheme.TextGreen : UITheme.TextPrimary); btn.AddThemeColorOverride("font_hover_color", UITheme.Gold); var sn = new StyleBoxFlat { BgColor = isCur ? new Color(0.10f, 0.18f, 0.10f) : new Color(0.13f, 0.10f, 0.18f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 }; var sh = new StyleBoxFlat { BgColor = new Color(0.22f, 0.18f, 0.32f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 }; btn.AddThemeStyleboxOverride("normal", sn); btn.AddThemeStyleboxOverride("hover", sh); btn.Pressed += () => { AudioManager.SetBgm(idx); _bgmSelectPopup.Hide(); RefreshBgmIndicator(); }; vbox.AddChild(btn); vbox.AddChild(new Control { CustomMinimumSize = new Vector2I(0, 2) }); }
	}

	// ===================== HELPERS =====================

	static Control SP(int h) => new Control { CustomMinimumSize = new Vector2I(0, h) };
	static Label HL(string t, int fs, Color c) { var lb = new Label { Text = t, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.TitleFont != null) lb.AddThemeFontOverride("font", UITheme.TitleFont); lb.AddThemeFontSizeOverride("font_size", fs); lb.AddThemeColorOverride("font_color", c); return lb; }
	static Label TB(string t, Color c, int fs) { var lb = new Label { Text = t, AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", fs); lb.AddThemeColorOverride("font_color", c); return lb; }
	static Label EL() { var lb = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", 11); lb.AddThemeColorOverride("font_color", UITheme.TextGreen); return lb; }
	static Control HR() { var r = new ColorRect { CustomMinimumSize = new Vector2I(0, 1), Color = UITheme.GoldDark, SizeFlagsHorizontal = SizeFlags.ExpandFill }; var mc = new MarginContainer(); mc.AddThemeConstantOverride("margin_left", 40); mc.AddThemeConstantOverride("margin_right", 40); mc.AddChild(r); return mc; }
	static void StatCard(GridContainer grid, string label, string value, Color valColor) { var card = new PanelContainer { CustomMinimumSize = new Vector2I(140, 60) }; var style = UITheme.CardStyle(); style.ContentMarginLeft = 10; style.ContentMarginRight = 10; style.ContentMarginTop = 8; style.ContentMarginBottom = 8; card.AddThemeStyleboxOverride("panel", style); var cv = new VBoxContainer(); card.AddChild(cv); var lb = new Label { Text = label, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", 11); lb.AddThemeColorOverride("font_color", UITheme.TextDim); cv.AddChild(lb); var vl = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.TitleFont != null) vl.AddThemeFontOverride("font", UITheme.TitleFont); vl.AddThemeFontSizeOverride("font_size", 22); vl.AddThemeColorOverride("font_color", valColor); cv.AddChild(vl); grid.AddChild(card); }
	static CenterContainer CenteredGrid(GridContainer grid) { var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(grid); return cc; }
	static PanelContainer MakeCard(int minWidth) { var card = new PanelContainer { CustomMinimumSize = new Vector2I(minWidth, 0) }; var s = UITheme.CardStyle(); s.ContentMarginLeft = 10; s.ContentMarginRight = 10; s.ContentMarginTop = 10; s.ContentMarginBottom = 10; card.AddThemeStyleboxOverride("panel", s); var content = new VBoxContainer(); card.AddChild(content); card.MouseEntered += () => UIAnimator.CardHoverEnter(card); card.MouseExited += () => UIAnimator.CardHoverExit(card); return card; }
	static PanelContainer MakeAvatarCircle(bool isMale, int size) { var avatar = new PanelContainer { CustomMinimumSize = new Vector2I(size, size) }; var avatarBg = new StyleBoxFlat { BgColor = isMale ? new Color(0.14f, 0.18f, 0.28f) : new Color(0.26f, 0.12f, 0.20f), CornerRadiusBottomLeft = size / 2, CornerRadiusBottomRight = size / 2, CornerRadiusTopLeft = size / 2, CornerRadiusTopRight = size / 2, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.GoldDark }; avatar.AddThemeStyleboxOverride("panel", avatarBg); var avatarTex = SpriteSheetManager.GetAvatar(isMale); if (avatarTex != null) avatar.AddChild(new TextureRect { Texture = avatarTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, CustomMinimumSize = new Vector2I(size - 4, size - 4) }); else { var label = new Label { Text = isMale ? "♂" : "♀", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; label.AddThemeFontSizeOverride("font_size", size / 3 + 6); label.AddThemeColorOverride("font_color", isMale ? new Color(0.5f, 0.7f, 1.0f) : new Color(1.0f, 0.5f, 0.7f)); avatar.AddChild(label); } return avatar; }
	static void StatLine(VBoxContainer parent, string label, int value, Color color) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"{label}:", CustomMinimumSize = new Vector2I(30, 0) }.WithFont(10, UITheme.TextDim)); var bar = new ColorRect { CustomMinimumSize = new Vector2I((int)(value * 0.8f), 6) }; bar.Color = color; row.AddChild(bar); row.AddChild(new Label { Text = value.ToString() }.WithFont(10, color)); parent.AddChild(row); }
	static string BarsString(double current, double max, int segments) { double ratio = Math.Clamp(current / max, 0, 1); int filled = (int)(ratio * segments); return "[" + new string('█', filled) + new string('░', segments - filled) + "]"; }
	static Color RealmColor(CultivationRealm realm) => realm switch { CultivationRealm.Mortal => UITheme.TextDim, CultivationRealm.QiRefining => new Color(0.7f, 0.7f, 0.7f), CultivationRealm.Foundation => UITheme.TextGreen, CultivationRealm.CoreFormation => UITheme.Gold, CultivationRealm.NascentSoul => new Color(0.7f, 0.3f, 1.0f), CultivationRealm.SpiritTransformation => UITheme.TextOrange, CultivationRealm.Tribulation => new Color(0.3f, 0.8f, 1.0f), CultivationRealm.GreatAscension => new Color(1.0f, 0.3f, 0.3f), _ => UITheme.TextPrimary };
	static string SectTitleDesc(int rep) => rep switch { < 50 => "无名小宗", < 150 => "初露锋芒", < 400 => "小有名气", < 800 => "一方豪强", < 1500 => "名门大派", < 3000 => "威震四方", < 6000 => "仙道圣地", _ => "万宗至尊" };
	static string FormatOutcome(EventOutcome o) { var parts = new List<string>(); if (o.ResourceChanges != null) foreach (var kv in o.ResourceChanges) { string sign = kv.Value >= 0 ? "+" : ""; parts.Add($"{ResName(kv.Key)}{sign}{kv.Value}"); } if (o.ReputationChange != 0) parts.Add($"声望{(o.ReputationChange >= 0 ? "+" : "")}{o.ReputationChange}"); if (o.PowerChange != 0) parts.Add($"战力{(o.PowerChange >= 0 ? "+" : "")}{o.PowerChange}"); if (o.DiscipleStatEffects != null && o.DiscipleStatEffects.Length >= 3) { if (o.DiscipleStatEffects[0] != 0) parts.Add($"全体忠诚{(o.DiscipleStatEffects[0] >= 0 ? "+" : "")}{o.DiscipleStatEffects[0]}"); if (o.DiscipleStatEffects[1] != 0) parts.Add($"全体心情{(o.DiscipleStatEffects[1] >= 0 ? "+" : "")}{o.DiscipleStatEffects[1]}"); if (o.DiscipleStatEffects[2] != 0) parts.Add($"全体气血{(o.DiscipleStatEffects[2] >= 0 ? "+" : "")}{o.DiscipleStatEffects[2]}"); } if (o.DiscipleCultivationBonus > 0) parts.Add($"全体修为+{o.DiscipleCultivationBonus:F0}"); if (o.DiscipleCountChange != 0) parts.Add($"弟子{(o.DiscipleCountChange >= 0 ? "+" : "")}{o.DiscipleCountChange}人"); return parts.Count > 0 ? $"[ {string.Join("  |  ", parts)} ]" : ""; }
	static string SkillName(int id) => id switch { 1 => "修炼加速", 2 => "战斗技巧", 3 => "炼丹精通", 4 => "炼器精通", 5 => "采集专精", 6 => "教导有方", 7 => "探索老手", 10 => "道脉传承", _ => $"技能{id}" };
	static string ResName(ResourceType rt) => rt switch { ResourceType.SpiritStone => "灵石", ResourceType.Herb => "灵草", ResourceType.Ore => "矿石", ResourceType.Pill => "丹药", ResourceType.Equipment => "法器", ResourceType.Contribution => "贡献", ResourceType.SpiritEssence => "灵气", _ => rt.ToString() };
	static Button Btn(string text) { var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) }; if (UITheme.BodyFont != null) b.AddThemeFontOverride("font", UITheme.BodyFont); b.AddThemeFontSizeOverride("font_size", 13); b.AddThemeColorOverride("font_color", UITheme.TextPrimary); b.AddThemeColorOverride("font_hover_color", UITheme.Gold); b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed()); return b; }
	static Button SmallBtn(string text) { var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(60, 24) }; if (UITheme.BodyFont != null) b.AddThemeFontOverride("font", UITheme.BodyFont); b.AddThemeFontSizeOverride("font_size", 11); b.AddThemeColorOverride("font_color", UITheme.TextPrimary); b.AddThemeColorOverride("font_hover_color", UITheme.Gold); b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed()); return b; }
}

internal static class UIColumns
{
	public static Color DarkBg => new(0.05f, 0.04f, 0.10f); public static Color PanelBg => new(0.10f, 0.08f, 0.15f); public static Color Gold => new(0.91f, 0.72f, 0.29f); public static Color TextDim => new(0.55f, 0.55f, 0.65f);
	public static Button MakeButton(string text, string tip) { var b = new Button { Text = text, TooltipText = tip, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) }; b.AddThemeFontSizeOverride("font_size", 13); b.AddThemeColorOverride("font_color", new(0.88f, 0.88f, 0.92f)); b.AddThemeColorOverride("font_hover_color", Gold); b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed()); return b; }
	public static Button MakeSmallButton(string text) { var b = new Button { Text = text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(60, 24) }; b.AddThemeFontSizeOverride("font_size", 11); b.AddThemeColorOverride("font_color", new(0.88f, 0.88f, 0.92f)); b.AddThemeColorOverride("font_hover_color", Gold); b.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); b.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); b.AddThemeStyleboxOverride("pressed", UITheme.BtnStylePressed()); return b; }
}

internal static class NodeExtensions
{
	public static void FreeChildren(this Node node) { foreach (var c in node.GetChildren().ToList()) { c.Free(); } }
	public static Label WithFont(this Label label, int size, Color color) { label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", color); return label; }
}
