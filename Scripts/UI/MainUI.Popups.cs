namespace SwordFateCultivationRecord;

public partial class MainUI : Control
{
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
		_recruitPopup = new Window { Title = "内门选拔", Size = new Vector2I(860, 520), Visible = false, Exclusive = true, Unresizable = true };
		_recruitPopup.CloseRequested += () => { _recruitPopup.Hide(); GM.CancelRecruit(); };
		AddChild(_recruitPopup);
		var rv = new VBoxContainer(); rv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _recruitPopup.AddChild(rv);
		rv.AddChild(SP(10));
		rv.AddChild(new Label { Text = "— 内门选拔 —", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(22, UITheme.Gold));
		var subLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; subLabel.AddThemeFontSizeOverride("font_size", 13); subLabel.AddThemeColorOverride("font_color", UITheme.TextDim); subLabel.Name = "SubLabel"; rv.AddChild(subLabel);
		var pickLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center }; pickLabel.AddThemeFontSizeOverride("font_size", 12); pickLabel.AddThemeColorOverride("font_color", UITheme.TextBlue); pickLabel.Name = "PickLabel"; rv.AddChild(pickLabel);
		rv.AddChild(SP(10));
		var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; rv.AddChild(scroll);
		_recruitCardContainer = new VBoxContainer(); scroll.AddChild(_recruitCardContainer);
		rv.AddChild(SP(4));
		var cancelBtn = new Button { Text = "尽舍之（不费天时）", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(200, 40) };
		cancelBtn.AddThemeFontSizeOverride("font_size", 14); cancelBtn.AddThemeColorOverride("font_color", UITheme.TextDim); cancelBtn.AddThemeColorOverride("font_hover_color", UITheme.Crimson);
		cancelBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); cancelBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		cancelBtn.Pressed += () => { _recruitPopup.Hide(); GM.CancelRecruit(); };
		var bc2 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc2.AddChild(cancelBtn); rv.AddChild(bc2); rv.AddChild(SP(8));
	}
	void BuildTournamentConfirmPopup()
	{
		_tournamentConfirmPopup = new Window { Title = "入门大比", Size = new Vector2I(380, 200), Visible = false, Exclusive = true, Unresizable = true };
		_tournamentConfirmPopup.CloseRequested += () => _tournamentConfirmPopup.Hide();
		AddChild(_tournamentConfirmPopup);
		var cv = new VBoxContainer(); cv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _tournamentConfirmPopup.AddChild(cv);
		cv.AddChild(SP(14));
		cv.AddChild(new Label { Text = "是否举行入门大比？", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(16, UITheme.Gold));
		cv.AddChild(SP(6));
		cv.AddChild(new Label { Text = "广发英雄帖，七日后召开选拔大会（消耗1日）", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
		cv.AddChild(SP(14));
		var btnRow = new HBoxContainer(); var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; btnRow.AddChild(bc);
		var innerBtnRow = new HBoxContainer(); bc.AddChild(innerBtnRow);
		var okBtn = new Button { Text = "确定", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) };
		okBtn.AddThemeFontSizeOverride("font_size", 14); okBtn.AddThemeColorOverride("font_color", UITheme.Gold); okBtn.AddThemeColorOverride("font_hover_color", new Color(1,1,1));
		okBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); okBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		okBtn.Pressed += () => { _tournamentConfirmPopup.Hide(); AudioManager.PlayClick(); GM.ScheduleRecruitTournament(); };
		innerBtnRow.AddChild(okBtn); innerBtnRow.AddChild(new Control { CustomMinimumSize = new Vector2I(12, 0) });
		var cancelBtn = new Button { Text = "取消", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) };
		cancelBtn.AddThemeFontSizeOverride("font_size", 14); cancelBtn.AddThemeColorOverride("font_color", UITheme.TextDim);
		cancelBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); cancelBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		cancelBtn.Pressed += () => _tournamentConfirmPopup.Hide();
		innerBtnRow.AddChild(cancelBtn); cv.AddChild(btnRow); cv.AddChild(SP(10));
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


	// ===================== FACILITY DETAIL POPUP =====================

	void BuildFacilityDetailPopup()
	{
		_facDetailPopup = new Window { Title = "灵筑管理", Size = new Vector2I(480, 420), Visible = false, Exclusive = true, Unresizable = true };
		_facDetailPopup.CloseRequested += () => _facDetailPopup.Hide();
		AddChild(_facDetailPopup);
		var root = new VBoxContainer(); root.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_facDetailPopup.AddChild(root);
	}

	void ShowFacilityDetail(FacilityData f)
	{
		_facDetailPopup.Title = $"{f.TypeName} · 管理";
		var root = (VBoxContainer)_facDetailPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));

		// Header
		var facTex = SpriteSheetManager.GetFacilityIcon(f.Type);
		if (facTex != null) { var ir = new TextureRect { Texture = facTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(56, 56) }; var ic = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ic.AddChild(ir); root.AddChild(ic); root.AddChild(SP(6)); }
		root.AddChild(new Label { Text = $"{f.TypeName} Lv.{f.Level}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(20, UITheme.Gold));
		var info = FacilityTable.GetInfo(f.Type);
		root.AddChild(new Label { Text = info.Description, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
		root.AddChild(SP(10));
		root.AddChild(HR());
		root.AddChild(SP(8));

		// Current status
		root.AddChild(HL("当前产出", 15, UITheme.Gold));
		root.AddChild(SP(4));
		int output = FacilityTable.GetOutput(f.Type, f.Level);
		string outputName = ResName(info.OutputType);
		root.AddChild(new Label { Text = $"每日产出: {outputName} +{output}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
		DiscipleTaskType? bonusTask = null;
		{
			// Check all task types for synergy
			foreach (DiscipleTaskType tt in Enum.GetValues<DiscipleTaskType>())
			{
				double tb = FacilityTable.GetTaskBonus(f.Type, f.Level, tt);
				if (tb > 0) { bonusTask = tt; break; }
			}
		}
		double taskBonus = bonusTask != null ? FacilityTable.GetTaskBonus(f.Type, f.Level, bonusTask.Value) : 0;
		if (taskBonus > 0)
			root.AddChild(new Label { Text = $"任务加成: +{taskBonus * 100:F0}%", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextGreen));
		root.AddChild(new Label { Text = $"容纳人数: {f.MaxDisciples}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));

		root.AddChild(SP(8));
		root.AddChild(HR());
		root.AddChild(SP(8));

		// Level-up preview
		if (f.Level < f.MaxLevel)
		{
			root.AddChild(HL($"晋升至 Lv.{f.Level + 1} 预览", 15, UITheme.Gold));
			root.AddChild(SP(4));
			int nextOut = FacilityTable.GetOutput(f.Type, f.Level + 1);
			root.AddChild(new Label { Text = $"产出: {outputName} {output} → {nextOut}/日", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextBlue));
			double nextBonus = bonusTask != null ? FacilityTable.GetTaskBonus(f.Type, f.Level + 1, bonusTask.Value) : 0;
			root.AddChild(new Label { Text = $"任务加成: +{taskBonus * 100:F0}% → +{nextBonus * 100:F0}%", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextBlue));
			int cost = FacilityTable.GetUpgradeCost(f.Type, f.Level);
			root.AddChild(new Label { Text = $"晋升费用: {cost}灵石", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextOrange));
			root.AddChild(SP(6));
			var upBtn = new Button { Text = "晋升灵筑", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(160, 36) };
			upBtn.AddThemeFontSizeOverride("font_size", 14); upBtn.AddThemeColorOverride("font_color", UITheme.TextPrimary); upBtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
			upBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); upBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			int fid = f.Id; upBtn.Pressed += () => { GM.UpgradeFacility(fid); _facDetailPopup.Hide(); RefreshFacilities(); };
			var ubc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ubc.AddChild(upBtn); root.AddChild(ubc);
		}
		else
		{
			root.AddChild(new Label { Text = "已达最高等级", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.Gold));
		}

		root.AddChild(SP(8));
		var closeBtn = SmallBtn("合上"); closeBtn.Pressed += () => _facDetailPopup.Hide();
		var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(closeBtn); root.AddChild(cc);
		root.AddChild(SP(8));

		_facDetailPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_facDetailPopup.GetChild(0));
	}

// ===================== RECRUIT / SMART / BATCH =====================

		void ShowRecruitSelection(List<DiscipleData> candidates)
	{
		int remaining = GM.TournamentPicksRemaining;
		if (remaining <= 0) { _recruitPopup.Hide(); RefreshAll(); return; }

		var rv = (VBoxContainer)_recruitPopup.GetChild(0);
		var subLabel = rv.FindChild("SubLabel") as Label;
		var pickLabel = rv.FindChild("PickLabel") as Label;
		if (subLabel != null) subLabel.Text = "七日大比落幕，求道者云集";
		if (pickLabel != null) pickLabel.Text = $"可选 {remaining} 人收入内门 · 共 {candidates.Count} 人应试";

		// Fortune's Child banner
		var fortuneChild = candidates.FirstOrDefault(c => c.Trait == "气运之子");
		if (fortuneChild != null)
		{
			var banner = new PanelContainer { CustomMinimumSize = new Vector2I(0, 36) };
			var bs = new StyleBoxFlat { BgColor = new Color(0.2f, 0.15f, 0.02f), CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = new Color(1, 0.7f, 0.1f) };
			banner.AddThemeStyleboxOverride("panel", bs);
			var bannerText = $"✦ 天降异象！气运之子「{fortuneChild.Name}」出现在选拔中 —— 天赋异禀，万中无一 ✦";
			var bl2 = new Label { Text = bannerText, HorizontalAlignment = HorizontalAlignment.Center };
			bl2.AddThemeFontSizeOverride("font_size", 14); bl2.AddThemeColorOverride("font_color", new Color(1, 0.85f, 0.2f));
			banner.AddChild(bl2);
			rv.AddChild(banner);
			rv.MoveChild(banner, 4);
		}

		_recruitCardContainer.FreeChildren();

		foreach (var d in candidates)
		{
			bool isFortune = d.Trait == "气运之子";
			bool isTopTier = isFortune || d.SpiritRoot == SpiritualRoot.Heavenly || (d.SpiritRoot == SpiritualRoot.SingleElement && d.Talent >= 65);
			bool isGood = !isTopTier && (d.SpiritRoot == SpiritualRoot.SingleElement || d.SpiritRoot == SpiritualRoot.DualElement || d.Talent >= 55);

			Color cardTint = isFortune ? new Color(0.25f, 0.2f, 0.05f) : isTopTier ? new Color(0.18f, 0.14f, 0.05f) : isGood ? new Color(0.12f, 0.13f, 0.18f) : new Color(0.10f, 0.08f, 0.13f);
			Color borderC = isFortune ? new Color(1, 0.7f, 0.1f) : isTopTier ? UITheme.Gold : isGood ? new Color(0.3f, 0.5f, 0.8f) : new Color(0.2f, 0.2f, 0.25f);
			int bw = isFortune ? 2 : isTopTier ? 1 : 0;

			var card = new PanelContainer { CustomMinimumSize = new Vector2I(0, 66), SizeFlagsHorizontal = SizeFlags.ExpandFill };
			var cs = new StyleBoxFlat { BgColor = cardTint, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, BorderWidthBottom = bw, BorderWidthLeft = bw, BorderWidthRight = bw, BorderWidthTop = bw, BorderColor = borderC, ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 6, ContentMarginBottom = 6 };
			card.AddThemeStyleboxOverride("panel", cs);
			var mainRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; card.AddChild(mainRow);

			// Avatar
			var av = MakeAvatarCircle(d.IsMale, 42); mainRow.AddChild(av); mainRow.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

			// Name + badge
			var infoCol = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			var nameRow = new HBoxContainer();
			string gi = d.IsMale ? "♂" : "♀";
			Color nc = isFortune ? new Color(1, 0.85f, 0.2f) : isTopTier ? UITheme.Gold : UITheme.TextPrimary;
			nameRow.AddChild(new Label { Text = gi + " " + d.Name }.WithFont(15, nc));
			nameRow.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

			string badge = d.SpiritRoot switch { SpiritualRoot.Heavenly => "天灵根", SpiritualRoot.SingleElement => "单灵根", SpiritualRoot.DualElement => "双灵根", SpiritualRoot.Special => "异灵根", SpiritualRoot.ThreeElement => "三灵根", _ => "杂灵根" };
			Color badgeC = d.SpiritRoot switch { SpiritualRoot.Heavenly => new Color(1, 0.75f, 0.1f), SpiritualRoot.SingleElement => new Color(1, 0.5f, 0.2f), SpiritualRoot.DualElement => new Color(0.7f, 0.3f, 1.0f), SpiritualRoot.Special => new Color(0.3f, 0.8f, 1.0f), _ => UITheme.TextDim };
			var blb = new Label { Text = badge }; blb.AddThemeFontSizeOverride("font_size", 10); blb.AddThemeColorOverride("font_color", badgeC);
			var bp = new PanelContainer(); bp.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(badgeC.R, badgeC.G, badgeC.B, 0.15f), CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3, CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, ContentMarginLeft = 6, ContentMarginRight = 6, ContentMarginTop = 2, ContentMarginBottom = 2 }); bp.AddChild(blb);
			nameRow.AddChild(bp);
			infoCol.AddChild(nameRow);

			// Quality + detail
			string qt = isFortune ? "✦ 气运之子 " : isTopTier ? "▲ 上等 " : isGood ? "● 中等 " : "";
			Color qc = isFortune ? new Color(1, 0.85f, 0.2f) : isTopTier ? UITheme.Gold : isGood ? UITheme.TextGreen : UITheme.TextDim;
			string extra = (d.Trait != "无" && !string.IsNullOrEmpty(d.Trait) && !isFortune) ? (" · " + d.Trait) : "";
			infoCol.AddChild(new Label { Text = qt + d.Age + "岁 · " + d.Background + " · " + d.Personality + extra }.WithFont(11, qc));
			infoCol.AddChild(new Label { Text = "天赋" + d.Talent + "  悟性" + d.Comprehension + "  体质" + d.Constitution + "  神识" + d.Spirit }.WithFont(11, UITheme.TextDim));
			mainRow.AddChild(infoCol);

			// Mini stat bars
			var statCol = new VBoxContainer { CustomMinimumSize = new Vector2I(130, 0) };
			MiniBar(statCol, "天赋", d.Talent, UITheme.Gold);
			MiniBar(statCol, "悟性", d.Comprehension, UITheme.TextBlue);
			MiniBar(statCol, "体质", d.Constitution, UITheme.TextGreen);
			MiniBar(statCol, "神识", d.Spirit, new Color(0.7f, 0.3f, 1.0f));
			mainRow.AddChild(statCol); mainRow.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });

			// Select button
			var selBtn = new Button { Text = "选 择", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 40) };
			selBtn.AddThemeFontSizeOverride("font_size", 14);
			selBtn.AddThemeColorOverride("font_color", isFortune ? new Color(1, 0.85f, 0.2f) : UITheme.TextPrimary);
			selBtn.AddThemeColorOverride("font_hover_color", isFortune ? new Color(1, 1, 0.5f) : UITheme.Gold);
			if (isFortune)
			{
				var fs = new StyleBoxFlat { BgColor = new Color(0.3f, 0.2f, 0.02f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = new Color(1, 0.7f, 0.1f) };
				selBtn.AddThemeStyleboxOverride("normal", fs);
				selBtn.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = new Color(0.5f, 0.35f, 0.05f), CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4 });
			}
			else
			{
				selBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); selBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			}
			var cp = d; selBtn.Pressed += () => { GM.ConfirmRecruit(cp); if (GM.PendingRecruitCandidates == null) { _recruitPopup.Hide(); RefreshAll(); } };
			var btnCenter = new CenterContainer(); btnCenter.AddChild(selBtn);
			mainRow.AddChild(btnCenter);
			_recruitCardContainer.AddChild(card); _recruitCardContainer.AddChild(SP(4));
		}

		_recruitPopup.PopupCentered();
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

	void OnEventTriggered(EventData e)
	{
		_eventPopup.Title = e.Title; _eventTitle.Text = e.Title; _eventDesc.Text = e.Description;
		_eventChoice1.Text = e.Choice1Text; _eventChoice1.Show();
		_eventChoice2.Text = e.Choice2Text; _eventChoice2.Show();
		_eventChoice3.Text = string.IsNullOrEmpty(e.Choice3Text) ? "无视" : e.Choice3Text; _eventChoice3.Show();

		// Show/hide effects based on FormationHall level
		bool canSee = CanSeeEventEffects();
		_eventEffect1.Text = canSee ? FormatOutcome(e.Choice1Outcome) : "???（需阵法殿占卜吉凶）";
		_eventEffect2.Text = canSee ? FormatOutcome(e.Choice2Outcome) : "???（需阵法殿占卜吉凶）";
		_eventEffect3.Text = canSee ? (e.Choice3Outcome != null ? FormatOutcome(e.Choice3Outcome) : "") : "???";
		_eventEffect1.Show(); _eventEffect2.Show(); _eventEffect3.Show();
		_dismissedBtn.Hide(); _eventPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_eventPopup.GetChild(0));
	}

	bool CanSeeEventEffects()
	{
		var fh = GM.Facilities.AllFacilities.FirstOrDefault(f => f.IsBuilt && f.Type == FacilityType.FormationHall);
		return fh != null; // Lv.1+ reveals effects
	}
	void ResolveChoice(int i) { _eventChoice1.Hide(); _eventChoice2.Hide(); _eventChoice3.Hide(); _eventEffect1.Hide(); _eventEffect2.Hide(); _eventEffect3.Hide(); _dismissedBtn.Show(); GM.ResolveEvent(i); _dismissedBtn.Text = "合上"; }
	void DismissEventPopup() { _eventPopup.Hide(); GM.DismissEvent(); RefreshAll(); }
	public override void _Input(InputEvent e)
	{
		if (!IsInsideTree()) return;
		if (!e.IsActionPressed("ui_cancel")) return;
		if (_eventPopup.Visible) { DismissEventPopup(); return; }
		if (_recruitPopup.Visible) { _recruitPopup.Hide(); GM.CancelRecruit(); return; }
		if (_detailPopup.Visible) { _detailPopup.Hide(); return; }
		if (_savePopup.Visible) { _savePopup.Hide(); return; }
		if (_loadPopup.Visible) { _loadPopup.Hide(); return; }
		if (_hintPopup.Visible) { _hintPopup.Hide(); return; }
		if (_smartPopup.Visible) { _smartPopup.Hide(); return; }
		if (_facDetailPopup.Visible) { _facDetailPopup.Hide(); return; }
		if (_bgmSelectPopup.Visible) { _bgmSelectPopup.Hide(); return; }
		if (_combatPopup.Visible) { _combatPopup.Hide(); return; }
		if (_realmPopup.Visible) { _realmPopup.Hide(); return; }
		if (_plotPopup.Visible) { _plotPopup.Hide(); return; }
		if (_endingPopup.Visible) { _endingPopup.Hide(); return; }
		if (_gameOverPopup.Visible) return;
		_settingsPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_settingsPopup.GetChild(0));
	}

	void ShowDiscipleDetail(DiscipleData d)
	{
		_detailPopup.Title = $"{d.Name} · 行状";
		var davTex = SpriteSheetManager.GetAvatar(d.IsMale);
		// Remove texture children but keep _detailAvatarLabel alive
		foreach (var child in _detailAvatarPanel.GetChildren().ToArray())
			if (child != _detailAvatarLabel) { _detailAvatarPanel.RemoveChild(child); child.QueueFree(); }
		if (davTex != null)
		{
			_detailAvatarLabel.Visible = false;
			_detailAvatarPanel.AddChild(new TextureRect { Texture = davTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, CustomMinimumSize = new Vector2I(84, 84) });
		}
		else
		{
			_detailAvatarLabel.Visible = true;
			_detailAvatarLabel.Text = d.IsMale ? "♂" : "♀";
			_detailAvatarLabel.AddThemeColorOverride("font_color", d.IsMale ? new Color(0.5f, 0.7f, 1.0f) : new Color(1.0f, 0.5f, 0.7f));
		}
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
		if (eqItems.Count > 0) { _equipSection.AddChild(new Label { Text = "已装备:" }.WithFont(13, UITheme.Gold)); foreach (var eq in eqItems) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, UITheme.TextGreen)); var ub = SmallBtn("卸下"); int eqId = eq.Id; ub.Pressed += () => { GM.UnequipItem(eqId); ShowDiscipleDetail(d); RefreshDisciples(); }; row.AddChild(ub); var upb = SmallBtn("升级"); upb.Pressed += () => { GM.UpgradeEquipment(eqId); ShowDiscipleDetail(d); RefreshDisciples(); }; if (eq.Quality < EquipmentQuality.Epic) row.AddChild(upb); _equipSection.AddChild(row); } }
		else _equipSection.AddChild(new Label { Text = "未装备法器" }.WithFont(11, UITheme.TextDim));
		var avail = GM.AllEquipment.Where(e => e.EquippedById < 0).ToList(); if (avail.Count > 0 && d.EquipmentIds.Count < 2) { _equipSection.AddChild(new Label { Text = "可装备:" }.WithFont(13, UITheme.Gold)); foreach (var eq in avail.Take(5)) { var row = new HBoxContainer(); row.AddChild(new Label { Text = $"  {eq.FullName}  战力+{eq.CombatBonus} 天赋+{eq.TalentBonus} 悟性+{eq.ComprehensionBonus} 体质+{eq.ConstitutionBonus} 神识+{eq.SpiritBonus}" }.WithFont(11, UITheme.TextPrimary)); var eb = SmallBtn("装备"); int eqId2 = eq.Id; eb.Pressed += () => { GM.EquipItem(eqId2, d.Id); ShowDiscipleDetail(d); RefreshDisciples(); }; row.AddChild(eb); var upb2 = SmallBtn("升级"); upb2.Pressed += () => { GM.UpgradeEquipment(eqId2); ShowDiscipleDetail(d); RefreshDisciples(); }; if (eq.Quality < EquipmentQuality.Epic) row.AddChild(upb2); _equipSection.AddChild(row); } }
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

		// ===================== PLOT INTRO POPUP =====================

	void ShowPlotIntro(PlotStageDef stage)
	{
		var root = (VBoxContainer)_plotPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(16));
		root.AddChild(HL(stage.ChapterTitle, 16, UITheme.TextOrange));
		root.AddChild(SP(8));
		root.AddChild(HL(stage.Title, 26, UITheme.Gold));
		root.AddChild(SP(20));

		var narrLabel = new RichTextLabel { BbcodeEnabled = true, FitContent = true, SizeFlagsHorizontal = SizeFlags.ExpandFill };
		narrLabel.AddThemeFontSizeOverride("normal_font_size", 15);
		narrLabel.AddThemeColorOverride("default_color", UITheme.TextPrimary);
		string narrText = stage.Narrative.Replace("\n", "\n\n");
		narrLabel.Text = narrText;
		narrLabel.VisibleCharacters = 0;
		root.AddChild(narrLabel);
		root.AddChild(SP(24));

		// Acknowledge button — visible immediately, skip on click
		var btnContainer = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		root.AddChild(btnContainer);
		var ackBtn = new Button { Text = "踏上仙途", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(220, 50) };
		ackBtn.AddThemeFontSizeOverride("font_size", 20); ackBtn.AddThemeColorOverride("font_color", UITheme.Gold);
		ackBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
		ackBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); ackBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		btnContainer.AddChild(ackBtn);
		bool textDone = false;

		// Typewriter at ~80 chars/sec (much faster)
		int total = narrLabel.GetTotalCharacterCount();
		double duration = Math.Max(1.5, total * 0.012);
		var t = narrLabel.CreateTween();
		t.TweenProperty(narrLabel, "visible_characters", total, duration).SetEase(Tween.EaseType.Out);
		t.Finished += () => { textDone = true; };

		// Click anywhere to skip or confirm
		ackBtn.Pressed += () => {
			if (!textDone) { narrLabel.VisibleCharacters = total; t.Kill(); textDone = true; return; }
			AudioManager.PlayClick();
			_plotPopup.Hide();
			// Use CallDeferred to let popup fully close before stage processing
			var gm = GM;
			Callable.From(() => gm.Plot.AcknowledgeStage(gm)).CallDeferred();
		};
		// Click on text area also skips
		narrLabel.GuiInput += (e) => {
			if (e is InputEventMouseButton mb && mb.Pressed && !textDone)
				{ narrLabel.VisibleCharacters = total; t.Kill(); textDone = true; }
		};

		_plotPopup.PopupCentered();
		UIAnimator.WindowOpen((Control)_plotPopup.GetChild(0));
	}

	// ===================== PLOT POPUP =====================

	void BuildPlotPopup()
	{
		_plotPopup = new Window { Title = "仙途", Size = new Vector2I(680, 520), Visible = false, Exclusive = true };
		_plotPopup.CloseRequested += () => _plotPopup.Hide();
		var pv = new VBoxContainer(); pv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_plotPopup.AddChild(pv);
		AddChild(_plotPopup);
	}

	void OnPlotStageCompleted(PlotStageDef stage, string message)
	{
		if (_plotPopup == null || _plotPopup.GetChildCount() == 0) return;
		_plotPopup.Title = stage.Title + " · 完成";
		var root = (VBoxContainer)_plotPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));
		root.AddChild(HL(stage.ChapterTitle, 13, UITheme.TextOrange));
		root.AddChild(HL("— " + stage.Title + " —", 22, UITheme.Gold));
		root.AddChild(SP(8));
		var msgLabel = new RichTextLabel { BbcodeEnabled = true, FitContent = true, SizeFlagsHorizontal = SizeFlags.ExpandFill };
		msgLabel.AddThemeFontSizeOverride("normal_font_size", 14); msgLabel.AddThemeColorOverride("default_color", UITheme.TextPrimary);
		msgLabel.Text = message;
		msgLabel.VisibleCharacters = 0;
		int msgTotal = msgLabel.GetTotalCharacterCount();
		var mt = msgLabel.CreateTween();
		mt.TweenProperty(msgLabel, "visible_characters", msgTotal, Math.Max(1.0, msgTotal * 0.012)).SetEase(Tween.EaseType.Out);
		root.AddChild(msgLabel);
		root.AddChild(SP(16));
		var closeBtn = new Button { Text = "继续仙途", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(180, 44) };
		closeBtn.AddThemeFontSizeOverride("font_size", 16); closeBtn.AddThemeColorOverride("font_color", UITheme.Gold);
		closeBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
		closeBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); closeBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		closeBtn.Pressed += () => { _plotPopup.Hide(); RefreshAll(); };
		var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(closeBtn);
		root.AddChild(bc); root.AddChild(SP(10));
		_plotPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_plotPopup.GetChild(0));
	}

	void OnPlotStageActivated(PlotStageDef stage)
	{
		UpdatePlotIndicator();
		if (stage.IsManualAcknowledge)
		{
			SwitchToTab(8);
			RefreshPlot();
		}
		else if (_activeTab == 8)
			RefreshPlot();
	}

	// ===================== REALM POPUP =====================

	void BuildRealmPopup()
	{
		_realmPopup = new Window { Title = "秘境探索", Size = new Vector2I(680, 500), Visible = false, Exclusive = true };
		_realmPopup.CloseRequested += () => { _realmPopup.Hide(); GM.Realm.CancelRealm(); };
		var rv = new VBoxContainer(); rv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_realmPopup.AddChild(rv);
		AddChild(_realmPopup);
	}

	void BuildEndingPopup()
	{
		_endingPopup = new Window { Title = "剑缘终章", Size = new Vector2I(720, 600), Visible = false, Exclusive = true };
		_endingPopup.CloseRequested += () => _endingPopup.Hide();
		var ev = new VBoxContainer(); ev.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_endingPopup.AddChild(ev);
		AddChild(_endingPopup);
	}

	void ShowEnding()
	{
		if (_endingShown) return; _endingShown = true;
		var root = (VBoxContainer)_endingPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(20));
		root.AddChild(HL("—— 剑缘修仙录 ——", 28, UITheme.Gold));
		root.AddChild(SP(10));
		root.AddChild(HL("全篇终", 20, new Color(1, 0.7f, 0.3f)));
		root.AddChild(SP(20));

		// Stats summary
		root.AddChild(HL("宗门总结", 18, UITheme.Gold)); root.AddChild(SP(8));
		root.AddChild(new Label { Text = $"宗门: {GM.FullSectName}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(15, UITheme.Gold));
		root.AddChild(new Label { Text = $"最终称号: {GM.SectTitle}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, new Color(1, 0.7f, 0.3f)));
		root.AddChild(new Label { Text = $"声望: {GM.SectReputation}  战力: {GM.SectPower}  宗门Lv.{GM.SectLevel}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
		root.AddChild(new Label { Text = $"内门弟子: {GM.Disciples.Count}人  外门弟子: {GM.OuterDiscipleCount}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
		root.AddChild(new Label { Text = $"灵筑: {GM.Facilities.AllFacilities.Count(f => f.IsBuilt)}座  道侣: {GM.Companions.AllCompanions.Count(c => c.IsMarried)}对", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
		root.AddChild(new Label { Text = $"法器库存: {GM.AllEquipment.Count}件  灵石: {GM.Resources.Get(ResourceType.SpiritStone)}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
		root.AddChild(new Label { Text = $"历经: 第{GM.Time.Year}年{GM.Time.Month}月  共{GM.Time.GetTotalDays()}日", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextDim));
		root.AddChild(new Label { Text = $"成就解锁: {GM.Achievements.Progress.TotalUnlocked}/{GM.Achievements.TotalCount}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextGreen));

		// Realm stats
		root.AddChild(SP(20));
		root.AddChild(HL("剑尊遗言", 16, UITheme.Gold)); root.AddChild(SP(8));
		root.AddChild(new Label { Text = "「吾乃云霄剑尊，三千年前渡劫失败……\n今见你所创之宗门，剑道昌盛，弟子如云，\n吾心甚慰。剑道不绝，传承永续。」", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, new Color(0.7f, 0.8f, 1.0f)));

		root.AddChild(SP(24));
		var btnRow = new HBoxContainer(); var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; btnRow.AddChild(bc);
		var inner = new HBoxContainer(); bc.AddChild(inner);
		var contBtn = new Button { Text = "继续仙途", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(160, 44) };
		contBtn.AddThemeFontSizeOverride("font_size", 16); contBtn.AddThemeColorOverride("font_color", UITheme.Gold); contBtn.AddThemeColorOverride("font_hover_color", new Color(1,1,1));
		contBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); contBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		contBtn.Pressed += () => _endingPopup.Hide();
		inner.AddChild(contBtn);
		inner.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });
		var menuBtn = new Button { Text = "返回主菜单", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(160, 44) };
		menuBtn.AddThemeFontSizeOverride("font_size", 16); menuBtn.AddThemeColorOverride("font_color", UITheme.TextDim); menuBtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
		menuBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); menuBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		menuBtn.Pressed += () => { _endingPopup.Hide(); GetTree().ChangeSceneToFile("res://Scenes/StartMenu.tscn"); };
		inner.AddChild(menuBtn);
		root.AddChild(btnRow); root.AddChild(SP(20));

		_endingPopup.PopupCentered();
		UIAnimator.WindowOpen((Control)_endingPopup.GetChild(0));
	}

	void StartRealmExploration()
	{
		var realm = GM.Realm.StartRealm(GM.SectLevel);
		ShowRealmRoom();
	}

	void ShowRealmRoom()
	{
		var realm = GM.Realm.CurrentRealm;
		if (!realm.IsActive) return;

		var root = (VBoxContainer)_realmPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));
		root.AddChild(HL(realm.RealmName, 20, UITheme.Gold));
		root.AddChild(SP(4));

		// Progress indicator
		string progress = "探索进度: ";
		for (int i = 0; i < realm.TotalRooms; i++)
			progress += i < realm.CurrentRoom ? "●" : i == realm.CurrentRoom ? "▶" : "○";
		root.AddChild(new Label { Text = progress, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.TextBlue));
		root.AddChild(SP(10));

		// Danger + treasure indicators
		var statRow = new HBoxContainer();
		var sc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; statRow.AddChild(sc);
		var statInner = new HBoxContainer(); sc.AddChild(statInner);
		statInner.AddChild(new Label { Text = "⚔ 战力" + GM.SectPower }.WithFont(12, UITheme.TextOrange));
		statInner.AddChild(new Control { CustomMinimumSize = new Vector2I(20, 0) });
		statInner.AddChild(new Label { Text = "💎 收获" + realm.TreasureScore }.WithFont(12, UITheme.Gold));
		statInner.AddChild(new Control { CustomMinimumSize = new Vector2I(20, 0) });
		statInner.AddChild(new Label { Text = "❤ 损伤" + realm.DamageTaken }.WithFont(12, UITheme.Crimson));
		root.AddChild(statRow); root.AddChild(SP(10));

		var room = realm.Rooms[realm.CurrentRoom];
		// Room description
		string hint = string.IsNullOrEmpty(room.ImageHint) ? "" : room.ImageHint + " ";
		root.AddChild(new Label { Text = hint + "第" + (realm.CurrentRoom + 1) + "关 · 危险Lv." + room.DangerLevel, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.TextOrange));
		root.AddChild(SP(8));
		root.AddChild(new Label { Text = room.Description, HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }.WithFont(13, UITheme.TextPrimary));
		root.AddChild(SP(14));

		// Choice buttons
		for (int i = 0; i < room.Choices.Count; i++)
		{
			int idx = i;
			var choice = room.Choices[i];
			string riskIcon = choice.RiskLevel switch { 1 => "🟢", 2 => "🟡", _ => "🔴" };
			var btn = new Button { Text = riskIcon + " " + choice.Text, Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(500, 44) };
			btn.AddThemeFontSizeOverride("font_size", 14); btn.AddThemeColorOverride("font_color", UITheme.TextPrimary);
			btn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
			btn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); btn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			btn.Pressed += () => OnRealmChoice(idx);
			var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(btn);
			root.AddChild(bc); root.AddChild(SP(6));
		}

		root.AddChild(SP(8));
		var cancelBtn = SmallBtn("退出秘境");
		cancelBtn.Pressed += () => { _realmPopup.Hide(); GM.Realm.CancelRealm(); };
		var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(cancelBtn); root.AddChild(cc);
		root.AddChild(SP(8));

		_realmPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_realmPopup.GetChild(0));
	}

	void OnRealmChoice(int idx)
	{
		var result = GM.Realm.ProcessChoice(idx, GM);
		var realm = GM.Realm.CurrentRealm;

		// Show result popup
		var root = (VBoxContainer)_realmPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));
		root.AddChild(HL(result.success ? "✦ 成功 ✦" : "✗ 失败 ✗", 22, result.success ? UITheme.Gold : UITheme.Crimson));
		root.AddChild(SP(8));
		root.AddChild(new Label { Text = result.text, HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }.WithFont(14, UITheme.TextPrimary));
		root.AddChild(SP(16));

		if (realm.IsActive)
		{
			var nextBtn = new Button { Text = "继续探索 →", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(200, 44) };
			nextBtn.AddThemeFontSizeOverride("font_size", 16); nextBtn.AddThemeColorOverride("font_color", UITheme.Gold);
			nextBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
			nextBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); nextBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			nextBtn.Pressed += ShowRealmRoom;
			var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(nextBtn);
			root.AddChild(bc);
		}
		else
		{
			root.AddChild(HL("秘境探索结束", 18, UITheme.Gold));
			root.AddChild(SP(4));
			root.AddChild(new Label { Text = $"总收获评分: {realm.TreasureScore}  损伤: {realm.DamageTaken}次", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextBlue));
			root.AddChild(SP(10));
			var closeBtn = new Button { Text = "返回宗门", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(180, 44) };
			closeBtn.AddThemeFontSizeOverride("font_size", 16); closeBtn.AddThemeColorOverride("font_color", UITheme.Gold);
			closeBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
			closeBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); closeBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			closeBtn.Pressed += () => { _realmPopup.Hide(); RefreshAll(); };
			var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(closeBtn);
			root.AddChild(bc);
		}
		root.AddChild(SP(10));
	}


	// ===================== COMBAT POPUP =====================

	void BuildCombatPopup()
	{
		_combatPopup = new Window { Title = "宗门出征", Size = new Vector2I(700, 560), Visible = false, Exclusive = true };
		_combatPopup.CloseRequested += () => _combatPopup.Hide();
		var cv = new VBoxContainer(); cv.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_combatPopup.AddChild(cv);
		AddChild(_combatPopup);
	}

	void ShowCombatMissions()
	{
		var missions = GM.GetCombatMissions();
		var root = (VBoxContainer)_combatPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));
		root.AddChild(HL("— 宗门出征 —", 20, UITheme.Gold));
		root.AddChild(SP(4));
		root.AddChild(new Label { Text = "选一款战斗任务，派遣弟子出征", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
		root.AddChild(SP(8));

		if (missions.Count == 0)
		{
			root.AddChild(new Label { Text = "暂无可用战斗任务", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.TextDim));
			root.AddChild(SP(10));
			var closeBtn = SmallBtn("归去"); closeBtn.Pressed += () => _combatPopup.Hide();
			var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(closeBtn); root.AddChild(cc);
			_combatPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_combatPopup.GetChild(0));
			return;
		}

		var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; root.AddChild(scroll);
		var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; scroll.AddChild(list);

		foreach (var m in missions)
		{
			var card = new PanelContainer { CustomMinimumSize = new Vector2I(0, 80), SizeFlagsHorizontal = SizeFlags.ExpandFill };
			var cs = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.07f, 0.14f),
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1,
				BorderColor = UITheme.GoldDark, ContentMarginLeft = 12, ContentMarginRight = 12,
				ContentMarginTop = 8, ContentMarginBottom = 8,
			};
			card.AddThemeStyleboxOverride("panel", cs);
			var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; card.AddChild(row);

			string icon = m.EnemyPower switch { < 80 => "⚔", < 200 => "⚡", < 400 => "🔥", _ => "💀" };
			var iconLabel = new Label { Text = icon, CustomMinimumSize = new Vector2I(48, 48) };
			iconLabel.AddThemeFontSizeOverride("font_size", 28);
			iconLabel.AddThemeColorOverride("font_color", UITheme.Gold);
			row.AddChild(iconLabel); row.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });

			var infoCol = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			infoCol.AddChild(new Label { Text = m.Name }.WithFont(16, UITheme.Gold));
			infoCol.AddChild(new Label { Text = m.Description, AutowrapMode = TextServer.AutowrapMode.WordSmart }.WithFont(11, UITheme.TextDim));
			string rewards = string.Join(" ", m.VictoryRewards.Select(kv => ResName(kv.Key) + "+" + kv.Value));
			string detailStr = $"敌人: {m.EnemyName}  战力 {m.EnemyPower}  奖励: {rewards}  声望+{m.VictoryReputation}";
			infoCol.AddChild(new Label { Text = detailStr }.WithFont(11, UITheme.TextBlue));
			row.AddChild(infoCol);

			var selBtn = new Button { Text = "征讨", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(80, 36) };
			selBtn.AddThemeFontSizeOverride("font_size", 14); selBtn.AddThemeColorOverride("font_color", UITheme.Gold);
			selBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
			selBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
			selBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
			int mid = m.MissionId;
			selBtn.Pressed += () => ShowCombatDiscipleSelect(mid);
			var btnCenter = new CenterContainer(); btnCenter.AddChild(selBtn);
			row.AddChild(btnCenter);
			list.AddChild(card); list.AddChild(SP(4));
		}

		root.AddChild(SP(8));
		var closeButton = SmallBtn("归去"); closeButton.Pressed += () => _combatPopup.Hide();
		var cc2 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc2.AddChild(closeButton); root.AddChild(cc2);
		root.AddChild(SP(8));

		_combatPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_combatPopup.GetChild(0));
	}

	void ShowCombatDiscipleSelect(int missionId)
	{
		var mission = GM.GetCombatMissions().FirstOrDefault(m => m.MissionId == missionId);
		if (mission == null) return;

		var root = (VBoxContainer)_combatPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));
		root.AddChild(HL("— 选将出征 —", 20, UITheme.Gold));
		root.AddChild(SP(4));
		string titleStr = $"目标: {mission.Name}  (战力 {mission.EnemyPower}) 最多{mission.MaxDisciples}人";
		root.AddChild(new Label { Text = titleStr, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextOrange));
		root.AddChild(SP(8));

		var disciples = GM.Disciples.AllDisciples.Where(d => d.Health > 0).ToList();
		if (disciples.Count == 0)
		{
			root.AddChild(new Label { Text = "没有可出征的弟子", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.TextDim));
			root.AddChild(SP(10));
			var backBtn2 = SmallBtn("返回"); backBtn2.Pressed += ShowCombatMissions;
			var cc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; cc.AddChild(backBtn2); root.AddChild(cc);
			_combatPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_combatPopup.GetChild(0));
			return;
		}

		var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill }; root.AddChild(scroll);
		var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; scroll.AddChild(list);

		var checks = new Dictionary<int, CheckBox>();
		int maxSelect = mission.MaxDisciples;

		foreach (var d in disciples)
		{
			var card = new PanelContainer { CustomMinimumSize = new Vector2I(0, 56), SizeFlagsHorizontal = SizeFlags.ExpandFill };
			var cs = new StyleBoxFlat
			{
				BgColor = new Color(0.10f, 0.08f, 0.14f),
				CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
				CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
				BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1,
				BorderColor = UITheme.GoldDark, ContentMarginLeft = 8, ContentMarginRight = 8,
				ContentMarginTop = 6, ContentMarginBottom = 6,
			};
			card.AddThemeStyleboxOverride("panel", cs);
			var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; card.AddChild(row);

			var cb = new CheckBox { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			cb.AddThemeFontSizeOverride("font_size", 13);
			cb.AddThemeColorOverride("font_color", UITheme.TextPrimary);
			string healthStr = BarsString(d.Health, d.MaxHealth, 6);
			string cbText = $"{d.Name}  [{d.FullRealmName}]  战力{d.CombatPower}  气血{healthStr}  {d.Age}岁";
			cb.Text = cbText;
			checks[d.Id] = cb;
			cb.Pressed += () =>
			{
				int checkedCount = checks.Values.Count(c => c.ButtonPressed);
				if (checkedCount > maxSelect)
				{
					cb.ButtonPressed = false;
					EventBus.EmitNotification("启禀", $"最多选择{maxSelect}人出征");
				}
			};
			row.AddChild(cb);
			list.AddChild(card); list.AddChild(SP(3));
		}

		root.AddChild(SP(8));

		var btnRow = new HBoxContainer(); var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		btnRow.AddChild(bc); var inner = new HBoxContainer(); bc.AddChild(inner);

		var goBtn = new Button { Text = "出  征", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(140, 44) };
		goBtn.AddThemeFontSizeOverride("font_size", 16); goBtn.AddThemeColorOverride("font_color", UITheme.Gold);
		goBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
		goBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
		goBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		goBtn.Pressed += () =>
		{
			var selected = checks.Where(kv => kv.Value.ButtonPressed).Select(kv => kv.Key).ToList();
			if (selected.Count == 0) { EventBus.EmitNotification("启禀", "请至少选择一名弟子"); return; }
			if (selected.Count > maxSelect) { EventBus.EmitNotification("启禀", $"最多选择{maxSelect}人"); return; }
			ExecuteAndShowCombat(missionId, selected);
		};
		inner.AddChild(goBtn); inner.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });

		var backBtn = new Button { Text = "返回", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 36) };
		backBtn.AddThemeFontSizeOverride("font_size", 14); backBtn.AddThemeColorOverride("font_color", UITheme.TextDim);
		backBtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
		backBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
		backBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		backBtn.Pressed += ShowCombatMissions;
		inner.AddChild(backBtn);
		root.AddChild(btnRow); root.AddChild(SP(8));

		_combatPopup.Title = $"选将 \u00B7 {mission.Name}";
		_combatPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_combatPopup.GetChild(0));
	}

	void ExecuteAndShowCombat(int missionId, List<int> discipleIds)
	{
		var result = GM.ExecuteCombatMission(discipleIds, missionId);
		if (result == null) return;

		var root = (VBoxContainer)_combatPopup.GetChild(0);
		root.FreeChildren();
		root.AddChild(SP(10));

		string outcomeTitle = result.Victory ? "\u2726 大获全胜 \u2726" : "\u2717 铩羽而归 \u2717";
		Color outcomeColor = result.Victory ? UITheme.Gold : UITheme.Crimson;
		root.AddChild(HL(outcomeTitle, 22, outcomeColor));
		root.AddChild(SP(4));
		root.AddChild(new Label { Text = $"{result.MissionName}  ({result.RoundsFought}回合)", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextBlue));
		root.AddChild(SP(10));

		var logLabel = new Label { Text = result.BattleLog, AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
		logLabel.AddThemeFontSizeOverride("font_size", 13);
		logLabel.AddThemeColorOverride("font_color", UITheme.TextPrimary);
		root.AddChild(logLabel);
		root.AddChild(SP(10));

		if (result.Rewards.Count > 0 || result.ReputationGain != 0)
		{
			root.AddChild(HL("战果", 16, UITheme.Gold)); root.AddChild(SP(4));
			var rewardLine = new HBoxContainer(); var rc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			rewardLine.AddChild(rc); var ri = new HBoxContainer(); rc.AddChild(ri);
			foreach (var kv in result.Rewards)
			{
				ri.AddChild(new Label { Text = $"{ResName(kv.Key)}+{kv.Value}" }.WithFont(12, UITheme.TextGreen));
				ri.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });
			}
			if (result.ReputationGain != 0)
			{
				Color repColor = result.ReputationGain > 0 ? UITheme.TextOrange : UITheme.Crimson;
				string repStr = result.ReputationGain > 0 ? $"声望+{result.ReputationGain}" : $"声望{result.ReputationGain}";
				ri.AddChild(new Label { Text = repStr }.WithFont(12, repColor));
			}
			if (result.EquipmentGained > 0)
			{
				ri.AddChild(new Label { Text = "法器+1" }.WithFont(12, new Color(0.7f, 0.3f, 1.0f)));
			}
			root.AddChild(rewardLine); root.AddChild(SP(10));
		}

		if (result.DiscipleReports.Count > 0)
		{
			root.AddChild(HL("弟子状况", 15, UITheme.Gold)); root.AddChild(SP(4));
			foreach (var r in result.DiscipleReports)
			{
				string status = r.WasInjured ? "\u26A0 受伤" : "\u2713 轻伤";
				Color sc = r.WasInjured ? UITheme.TextOrange : UITheme.TextGreen;
				string reportStr = $"  {r.Name}  战力{r.StartingPower}  造成伤害{r.DamageDealt}  气血损失{r.HealthLost}  {status}";
				root.AddChild(new Label { Text = reportStr }.WithFont(12, sc));
			}
		}

		root.AddChild(SP(14));
		var closeBtn = new Button { Text = "返回宗门", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(180, 44) };
		closeBtn.AddThemeFontSizeOverride("font_size", 16); closeBtn.AddThemeColorOverride("font_color", UITheme.Gold);
		closeBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
		closeBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal());
		closeBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
		closeBtn.Pressed += () => { _combatPopup.Hide(); RefreshAll(); };
		var bc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; bc.AddChild(closeBtn);
		root.AddChild(bc); root.AddChild(SP(10));

		_combatPopup.Title = result.Victory ? "大获全胜" : "铩羽而归";
		_combatPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_combatPopup.GetChild(0));
	}
}
