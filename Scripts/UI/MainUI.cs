namespace SwordFateCultivationRecord;

public partial class MainUI : Control
{
	private GameManager GM => GameManager.Instance;
	private static readonly string[] TaskNames = { "修炼", "训练", "采集", "炼丹", "炼器", "授课", "守卫", "探索", "休息" };
	private static readonly string[] TabLabels = { "总览", "弟子", "营造", "灵筑", "道缘", "门令", "记事", "卷宗", "剧情" };
	private int _activeTab;

	// Top bar
	private Label _timeLabel = null!, _sectLabel = null!;
	private readonly Dictionary<ResourceType, Label> _resourceLabels = new();
	private readonly Dictionary<ResourceType, HBoxContainer> _resourceAnchors = new(); // for float animations
	private Control _floatOverlay = null!; // overlay for floating resource gain labels

	// Bottom bar
	private Button _nextDayBtn = null!;

	// Sidebar
	private readonly Button[] _sidebarBtns = new Button[9];

	// Content
	private VBoxContainer _contentStack = null!;
	private ScrollContainer _contentScroll = null!;
	private readonly VBoxContainer[] _tabContents = new VBoxContainer[9];

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
	private Window _tournamentConfirmPopup = null!;
	private HBoxContainer _candidateRow = null!;
	private Label _tournamentLabel = null!;
	private VBoxContainer _recruitCardContainer = null!;
	private ColorRect _dayFlash = null!;
	private Window _realmPopup = null!;
	private Window _plotPopup = null!;
	private Window _endingPopup = null!;
	private bool _endingShown;
	private Button _realmBtn = null!;

	// Smart/Batch assign
	private Window _smartPopup = null!;
	private Window _facDetailPopup = null!;
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
		if (GM.IsInitialized) { RefreshAll(); if (GM.Plot.ActiveStage != null) { SwitchToTab(8); RefreshPlot(); } }
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
		EventBus.PlotStageCompleted -= OnPlotStageCompleted;
		EventBus.PlotStageActivated -= OnPlotStageActivated;
		EventBus.GameEnding -= ShowEnding;
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

		var tabIcons = new[] { "总览.png","弟子.png","营造.png","灵筑.png","道缘.png","门令.png","记事.png","卷宗.png","剧情.png" };
		for (int i = 0; i < TabLabels.Length; i++)
		{
			int tabIdx = i;
			var btn = new Button { Text = "   " + TabLabels[i], Alignment = HorizontalAlignment.Left, CustomMinimumSize = new Vector2I(90, 38) };
			string ipath = "res://Resources/Textures/Icons/" + tabIcons[i];
			if (ResourceLoader.Exists(ipath))
			{
				btn.Icon = ResourceLoader.Load<Texture2D>(ipath);
				btn.ExpandIcon = true;
			}
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
			var innerRow = new HBoxContainer(); _resourceAnchors[rt] = innerRow;
			var resIcon = SpriteSheetManager.GetResourceIcon(rt);
			if (resIcon != null) { var icon = new TextureRect { Texture = resIcon, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(20, 20) }; innerRow.AddChild(icon); innerRow.AddChild(new Control { CustomMinimumSize = new Vector2I(2, 0) }); }
			var lb = new Label(); lb.AddThemeFontSizeOverride("font_size", 12); _resourceLabels[rt] = lb; innerRow.AddChild(lb);
			resRow.AddChild(innerRow); resRow.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });
		}
		topHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

		// --- Content Area ---
		var contentPanel = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
		contentPanel.AddThemeStyleboxOverride("panel", UITheme.ContentAreaStyle());
		mainArea.AddChild(contentPanel);
		_contentStack = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; contentPanel.AddChild(_contentStack);
		_contentStack.AddChild(SP(14));
		_contentScroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill, ClipContents = true };
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
		bottomHBox.AddChild(_nextDayBtn); _nextDayBtn.Pressed += () => { UIAnimator.ButtonPress(_nextDayBtn); GM.NextDay(); RefreshAll(); };
		bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });
		var recruitBtn = Btn("入门大比"); recruitBtn.CustomMinimumSize = new Vector2I(110, 38);
		recruitBtn.Pressed += () => { UIAnimator.ButtonPress(recruitBtn); _tournamentConfirmPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_tournamentConfirmPopup.GetChild(0)); };
		bottomHBox.AddChild(recruitBtn); bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
		_realmBtn = Btn("秘境探索"); _realmBtn.CustomMinimumSize = new Vector2I(110, 38);
		_realmBtn.Visible = false;
		_realmBtn.Pressed += () => { UIAnimator.ButtonPress(_realmBtn); StartRealmExploration(); };
		bottomHBox.AddChild(_realmBtn); bottomHBox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
		_bgmIndicator = new Button { Text = "♪", Alignment = HorizontalAlignment.Center };
		_bgmIndicator.AddThemeFontSizeOverride("font_size", 10); _bgmIndicator.AddThemeColorOverride("font_color", new Color(0.4f, 0.6f, 0.4f));
		_bgmIndicator.AddThemeColorOverride("font_hover_color", new Color(0.6f, 0.9f, 0.6f));
		_bgmIndicator.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.05f, 0.04f, 0.08f, 0.7f), CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3, CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3 });
		_bgmIndicator.Flat = true;
		_bgmIndicator.Pressed += () => { RefreshBgmPopup(); _bgmSelectPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_bgmSelectPopup.GetChild(0)); };
		bottomHBox.AddChild(_bgmIndicator); bottomHBox.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });

		BuildPopups();
		BuildRecruitPopup();
		BuildTournamentConfirmPopup();
		BuildSmartAssignPopup();
		BuildFacilityDetailPopup();
		BuildPlotPopup();
		BuildRealmPopup();
		BuildEndingPopup();

		for (int i = 0; i < 9; i++) _tabContents[i] = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		SwitchToTab(GM.Plot?.ActiveStage != null ? 8 : 0);

		_dayFlash = new ColorRect { Color = new Color(1, 1, 1, 0), MouseFilter = MouseFilterEnum.Ignore };
		_dayFlash.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(_dayFlash);
		_floatOverlay = new Control { MouseFilter = MouseFilterEnum.Ignore };
		_floatOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(_floatOverlay);
	}

	void UpdatePlotIndicator()
	{
		bool hasPlot = GM.Plot?.ActiveStage != null;
		_sidebarBtns[8].Text = hasPlot ? "  ● 剧情" : "  剧情";
		if (hasPlot)
			_sidebarBtns[8].AddThemeColorOverride("font_color", new Color(0.91f, 0.72f, 0.29f));
	}

	// ===================== HELPERS =====================

	static Control SP(int h) => new Control { CustomMinimumSize = new Vector2I(0, h) };
	static Label HL(string t, int fs, Color c) { var lb = new Label { Text = t, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.TitleFont != null) lb.AddThemeFontOverride("font", UITheme.TitleFont); lb.AddThemeFontSizeOverride("font_size", fs); lb.AddThemeColorOverride("font_color", c); return lb; }
	static Label TB(string t, Color c, int fs) { var lb = new Label { Text = t, AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", fs); lb.AddThemeColorOverride("font_color", c); return lb; }
	static Label EL() { var lb = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", 11); lb.AddThemeColorOverride("font_color", UITheme.TextGreen); return lb; }
	static Control HR() { var r = new ColorRect { CustomMinimumSize = new Vector2I(0, 1), Color = UITheme.GoldDark, SizeFlagsHorizontal = SizeFlags.ExpandFill }; var mc = new MarginContainer(); mc.AddThemeConstantOverride("margin_left", 40); mc.AddThemeConstantOverride("margin_right", 40); mc.AddChild(r); return mc; }
	static void StatCard(GridContainer grid, string label, string value, Color valColor) { var card = new PanelContainer { CustomMinimumSize = new Vector2I(140, 60) }; var style = UITheme.CardStyle(); style.ContentMarginLeft = 10; style.ContentMarginRight = 10; style.ContentMarginTop = 8; style.ContentMarginBottom = 8; card.AddThemeStyleboxOverride("panel", style); var cv = new VBoxContainer(); card.AddChild(cv); var lb = new Label { Text = label, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.BodyFont != null) lb.AddThemeFontOverride("font", UITheme.BodyFont); lb.AddThemeFontSizeOverride("font_size", 11); lb.AddThemeColorOverride("font_color", UITheme.TextDim); cv.AddChild(lb); var vl = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center }; if (UITheme.TitleFont != null) vl.AddThemeFontOverride("font", UITheme.TitleFont); vl.AddThemeFontSizeOverride("font_size", 22); vl.AddThemeColorOverride("font_color", valColor); cv.AddChild(vl); grid.AddChild(card); }
	static CenterContainer CenteredGrid(GridContainer grid) { var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(grid); return cc; }
	static PanelContainer MakeCard(int minWidth, int minHeight = 0) { var card = new PanelContainer { CustomMinimumSize = new Vector2I(minWidth, minHeight), ClipContents = true }; var s = UITheme.CardStyle(); s.ContentMarginLeft = 8; s.ContentMarginRight = 8; s.ContentMarginTop = 8; s.ContentMarginBottom = 8; card.AddThemeStyleboxOverride("panel", s); var content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; card.AddChild(content); return card; }
	static PanelContainer MakeAvatarCircle(bool isMale, int size) { var avatar = new PanelContainer { CustomMinimumSize = new Vector2I(size, size) }; var avatarBg = new StyleBoxFlat { BgColor = isMale ? new Color(0.14f, 0.18f, 0.28f) : new Color(0.26f, 0.12f, 0.20f), CornerRadiusBottomLeft = size / 2, CornerRadiusBottomRight = size / 2, CornerRadiusTopLeft = size / 2, CornerRadiusTopRight = size / 2, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = UITheme.GoldDark }; avatar.AddThemeStyleboxOverride("panel", avatarBg); var avatarTex = SpriteSheetManager.GetAvatar(isMale); if (avatarTex != null) avatar.AddChild(new TextureRect { Texture = avatarTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, CustomMinimumSize = new Vector2I(size - 4, size - 4) }); else { var label = new Label { Text = isMale ? "♂" : "♀", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; label.AddThemeFontSizeOverride("font_size", size / 3 + 6); label.AddThemeColorOverride("font_color", isMale ? new Color(0.5f, 0.7f, 1.0f) : new Color(1.0f, 0.5f, 0.7f)); avatar.AddChild(label); } return avatar; }
	static void MiniBar(VBoxContainer parent, string label, int value, Color color) { var row = new HBoxContainer(); row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2I(24, 0) }.WithFont(9, UITheme.TextDim)); var bg = new ColorRect { Color = new Color(0.15f, 0.12f, 0.18f), CustomMinimumSize = new Vector2I(70, 6) }; row.AddChild(bg); var fill = new ColorRect { Color = color, CustomMinimumSize = new Vector2I((int)(value * 0.7f), 6) }; row.AddChild(fill); row.AddChild(new Label { Text = value.ToString() }.WithFont(9, color)); parent.AddChild(row); }
	static void StatLine(VBoxContainer parent, string label, int value, Color color) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"{label}:", CustomMinimumSize = new Vector2I(30, 0) }.WithFont(10, UITheme.TextDim)); var bar = new ColorRect { CustomMinimumSize = new Vector2I((int)(value * 0.8f), 6) }; bar.Color = color; row.AddChild(bar); row.AddChild(new Label { Text = value.ToString() }.WithFont(10, color)); parent.AddChild(row); }
	static string BarsString(double current, double max, int segments) { double ratio = Math.Clamp(current / max, 0, 1); int filled = (int)(ratio * segments); return "[" + new string('█', filled) + new string('░', segments - filled) + "]"; }
	static string RealmNameCN(CultivationRealm r) => r switch { CultivationRealm.Mortal => "凡人", CultivationRealm.QiRefining => "练气", CultivationRealm.Foundation => "筑基", CultivationRealm.CoreFormation => "金丹", CultivationRealm.NascentSoul => "元婴", CultivationRealm.SpiritTransformation => "化神", CultivationRealm.Tribulation => "渡劫", CultivationRealm.GreatAscension => "大乘", _ => "未知" };
	static Color RealmColor(CultivationRealm realm) => realm switch { CultivationRealm.Mortal => UITheme.TextDim, CultivationRealm.QiRefining => new Color(0.7f, 0.7f, 0.7f), CultivationRealm.Foundation => UITheme.TextGreen, CultivationRealm.CoreFormation => UITheme.Gold, CultivationRealm.NascentSoul => new Color(0.7f, 0.3f, 1.0f), CultivationRealm.SpiritTransformation => UITheme.TextOrange, CultivationRealm.Tribulation => new Color(0.3f, 0.8f, 1.0f), CultivationRealm.GreatAscension => new Color(1.0f, 0.3f, 0.3f), _ => UITheme.TextPrimary };
	static string SectTitleDesc(int rep) => rep switch { < 50 => "无名小宗", < 150 => "初露锋芒", < 400 => "小有名气", < 800 => "一方豪强", < 1500 => "名门大派", < 3000 => "威震四方", < 6000 => "仙道圣地", _ => "万宗至尊" };
	static string FormatOutcome(EventOutcome o) { var parts = new List<string>(); if (o.ResourceChanges != null) foreach (var kv in o.ResourceChanges) { string sign = kv.Value >= 0 ? "+" : ""; parts.Add($"{ResName(kv.Key)}{sign}{kv.Value}"); } if (o.ReputationChange != 0) parts.Add($"声望{(o.ReputationChange >= 0 ? "+" : "")}{o.ReputationChange}"); if (o.PowerChange != 0) parts.Add($"战力{(o.PowerChange >= 0 ? "+" : "")}{o.PowerChange}"); if (o.DiscipleStatEffects != null) { var ds = o.DiscipleStatEffects; if (ds.LoyaltyChange != 0) parts.Add($"全忠{(ds.LoyaltyChange >= 0 ? "+":"")}{ds.LoyaltyChange}"); if (ds.MoodChange != 0) parts.Add($"全心情{(ds.MoodChange >= 0 ? "+":"")}{ds.MoodChange}"); if (ds.HealthChange != 0) parts.Add($"全气血{(ds.HealthChange >= 0 ? "+":"")}{ds.HealthChange}"); } if (o.DiscipleCultivationBonus > 0) parts.Add($"全体修为+{o.DiscipleCultivationBonus:F0}"); if (o.DiscipleCountChange != 0) parts.Add($"弟子{(o.DiscipleCountChange >= 0 ? "+" : "")}{o.DiscipleCountChange}人"); return parts.Count > 0 ? $"[ {string.Join("  |  ", parts)} ]" : ""; }
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
