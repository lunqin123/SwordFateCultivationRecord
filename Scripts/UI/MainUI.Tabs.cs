namespace SwordFateCultivationRecord;

public partial class MainUI : Control
{
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
        switch (idx) { case 0: RefreshOverview(); break; case 1: RefreshDisciples(); break; case 2: RefreshBuild(); break; case 3: RefreshFacilities(); break; case 4: RefreshCompanions(); break; case 5: RefreshQuests(); break; case 6: RefreshLog(); break; case 7: RefreshStats(); break; case 8: RefreshPlot(); break; }
    }

    // ===================== SIGNALS =====================

    void ConnectSignals()
    {
        EventBus.DayPassed += OnDayPassed; EventBus.ResourceChanged += OnResourceChanged;
        EventBus.EventChoiceRequired += OnEventTriggered; EventBus.GameNotification += OnGameNotification;
        EventBus.DiscipleRecruited += OnDiscipleChanged; EventBus.DiscipleDeparted += OnDiscipleChanged;
        EventBus.RecruitSelectionReady += ShowRecruitSelection;
        EventBus.PlotStageCompleted += OnPlotStageCompleted;
        EventBus.PlotStageActivated += OnPlotStageActivated;
        EventBus.GameEnding += ShowEnding;
    }
    void OnDayPassed(int _, int __, int ___)
    {
        RefreshAll();
        if (_dayFlash == null) return;
        _dayFlash.Color = new Color(0.95f, 0.78f, 0.35f, 0.18f);
        var t = _dayFlash.CreateTween();
        t.TweenProperty(_dayFlash, "color", new Color(1, 1, 1, 0), 0.5f).SetEase(Tween.EaseType.Out);
    }
    void OnResourceChanged(ResourceType type, int oldVal, int newVal) { RefreshResources(); if (newVal > oldVal) ShowResourceGain(type, newVal - oldVal); }

    void ShowResourceGain(ResourceType type, int delta)
    {
        if (!_resourceAnchors.TryGetValue(type, out var anchor)) return;
        var popup = new Label { Text = "+" + delta, HorizontalAlignment = HorizontalAlignment.Center };
        popup.AddThemeFontSizeOverride("font_size", 14);
        popup.AddThemeColorOverride("font_color", UITheme.Gold);
        popup.AddThemeColorOverride("font_outline_color", new Color(0,0,0,0.6f));
        popup.AddThemeConstantOverride("outline_size", 2);
        var globalPos = anchor.GetGlobalRect().Position + new Vector2(anchor.Size.X / 2, 0);
        popup.Position = globalPos - _floatOverlay.GetGlobalRect().Position + new Vector2(0, -10);
        _floatOverlay.AddChild(popup);
        var t = popup.CreateTween().SetParallel();
        t.TweenProperty(popup, "position", popup.Position + new Vector2(0, -30), 0.8f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        t.TweenProperty(popup, "modulate", new Color(1,1,1,0), 0.6f).SetEase(Tween.EaseType.In).SetDelay(0.2f);
        t.Finished += () => popup.QueueFree();
    }
    void OnGameNotification(string t, string m)
    {
        if (t is "启禀" or "灵筑解锁" or "功能解锁") { _hintLabel.Text = m; _hintPopup.Title = t; _hintPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_hintPopup.GetChild(0)); }
        if (t == "宗门倾覆") { _gameOverPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_gameOverPopup.GetChild(0)); }
    }
    void OnDiscipleChanged(DiscipleData _) { RefreshDisciples(); RefreshOverview(); }

    // ===================== REFRESH =====================

    void RefreshAll() { if (!IsInsideTree()) return; RefreshTime(); RefreshResources(); RefreshSectInfo(); RefreshTournament(); RefreshTabContent(_activeTab); RefreshBgmIndicator(); RefreshButtons(); UpdatePlotIndicator(); }
    void RefreshTime() => _timeLabel.Text = GM.Time.GetDateString();
    void RefreshSectInfo() { _sectLabel.Text = $"{GM.FullSectName} Lv.{GM.SectLevel}  内门{GM.Disciples.Count}/{GM.MaxDisciples}  外门{GM.OuterDiscipleCount}/{GM.MaxOuterDisciples}"; _realmBtn.Visible = GM.SectLevel >= 3;
        _combatBtn.Visible = GM.SectLevel >= 2; }
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
        StatCard(statGrid, "战力", GM.SectPower.ToString(), UITheme.TextOrange); StatCard(statGrid, "内门", $"{GM.Disciples.Count}/{GM.MaxDisciples}", UITheme.TextGreen);
        StatCard(statGrid, "灵筑", $"{GM.Facilities.AllFacilities.Count(f => f.IsBuilt)}座", UITheme.TextGreen);
        StatCard(statGrid, "营造中", $"{GM.Facilities.AllFacilities.Count(f => f.IsUnderConstruction)}座", UITheme.TextOrange);
        StatCard(statGrid, "道缘", $"{GM.Companions.AllCompanions.Count(c => c.IsMarried)}对", new Color(1.0f, 0.45f, 0.65f));
        StatCard(statGrid, "子嗣", $"{GM.Children.Count}人", new Color(0.5f, 0.8f, 0.9f));
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
        c.AddChild(HL($"内门弟子（{GM.Disciples.Count}/{GM.MaxDisciples}人）", 18, UITheme.Gold)); c.AddChild(SP(4));

        // 外门管理区
        {
            int gCnt2 = GM.OuterGatherCount, tCnt2 = GM.OuterTradeCount, iCnt2 = GM.OuterIdleCount;
            double eff3 = GM.OuterEfficiency;
            string effTag2 = eff3 >= 1.0 ? "" : $" (超限{eff3*100:F0}%)";
            c.AddChild(new Label { Text = $"外门 {GM.OuterDiscipleCount}/{GM.MaxOuterDisciples}人: 采集{gCnt2} {tCnt2}经商 {iCnt2}待命{effTag2}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));

            // +/- buttons for role adjustment (no sliders — ScrollContainer eats drag)
            var roleRow = new HBoxContainer(); var roleCenter = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; roleRow.AddChild(roleCenter);
            var roleInner = new HBoxContainer(); roleCenter.AddChild(roleInner);

            Action buildRole = () => {
                roleInner.FreeChildren();
                // Gather
                roleInner.AddChild(new Label { Text = "采集" }.WithFont(12, UITheme.TextGreen));
                var gMinus = SmallBtn("−"); gMinus.CustomMinimumSize = new Vector2I(26, 26);
                gMinus.Pressed += () => { GM.SetOuterRoles(GM.OuterGatherRatio - 5, GM.OuterTradeRatio); RefreshDisciples(); };
                roleInner.AddChild(gMinus);
                roleInner.AddChild(new Label { Text = GM.OuterGatherRatio + "%", CustomMinimumSize = new Vector2I(38, 0), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
                var gPlus = SmallBtn("+"); gPlus.CustomMinimumSize = new Vector2I(26, 26);
                gPlus.Pressed += () => { GM.SetOuterRoles(GM.OuterGatherRatio + 5, GM.OuterTradeRatio); RefreshDisciples(); };
                roleInner.AddChild(gPlus);
                roleInner.AddChild(new Control { CustomMinimumSize = new Vector2I(20, 0) });
                // Trade
                roleInner.AddChild(new Label { Text = "经商" }.WithFont(12, UITheme.Gold));
                var tMinus = SmallBtn("−"); tMinus.CustomMinimumSize = new Vector2I(26, 26);
                tMinus.Pressed += () => { GM.SetOuterRoles(GM.OuterGatherRatio, GM.OuterTradeRatio - 5); RefreshDisciples(); };
                roleInner.AddChild(tMinus);
                roleInner.AddChild(new Label { Text = GM.OuterTradeRatio + "%", CustomMinimumSize = new Vector2I(38, 0), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextPrimary));
                var tPlus = SmallBtn("+"); tPlus.CustomMinimumSize = new Vector2I(26, 26);
                tPlus.Pressed += () => { GM.SetOuterRoles(GM.OuterGatherRatio, GM.OuterTradeRatio + 5); RefreshDisciples(); };
                roleInner.AddChild(tPlus);
            };
            buildRole();
            c.AddChild(roleRow);

            // Recruit button
            var obr = new HBoxContainer(); obr.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
            int rcost = 50 + GM.OuterDiscipleCount / 5;
            var rob = new Button { Text = $"招募外门 ({rcost}灵石)", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(170, 34) };
            rob.AddThemeFontSizeOverride("font_size", 12); rob.AddThemeColorOverride("font_color", UITheme.TextPrimary); rob.AddThemeColorOverride("font_hover_color", UITheme.Gold);
            rob.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); rob.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
            rob.Pressed += () => { AudioManager.PlayClick(); GM.RecruitOuterDisciples(); RefreshDisciples(); };
            obr.AddChild(rob); obr.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
            c.AddChild(obr);
        }
        c.AddChild(SP(8)); c.AddChild(HR()); c.AddChild(SP(8));

        if (GM.Disciples.Count == 0) { c.AddChild(TB("尚无内门弟子。点击底部「内门选拔」举办选拔大会（七日后举行）。", UITheme.TextDim, 13)); return; }

        // Batch controls
        _batchChecks.Clear();
        var batchBar = new HBoxContainer(); var bc1 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; batchBar.AddChild(bc1);
        var batchInner = new HBoxContainer(); bc1.AddChild(batchInner);
        var selAll = SmallBtn("全选"); selAll.Pressed += () => { foreach (var kv in _batchChecks) kv.Value.ButtonPressed = true; }; batchInner.AddChild(selAll);
        batchInner.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
        var deselAll = SmallBtn("全不选"); deselAll.Pressed += () => { foreach (var kv in _batchChecks) kv.Value.ButtonPressed = false; }; batchInner.AddChild(deselAll);
        batchInner.AddChild(new Control { CustomMinimumSize = new Vector2I(8, 0) });
        batchInner.AddChild(new Label { Text = "批量设为:", VerticalAlignment = VerticalAlignment.Center }.WithFont(11, UITheme.TextDim));
        _batchDrop = new OptionButton(); _batchDrop.AddThemeFontSizeOverride("font_size", 11); foreach (var tn in TaskNames) _batchDrop.AddItem(tn); batchInner.AddChild(_batchDrop);
        batchInner.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
        var applyBtn = SmallBtn("执行"); applyBtn.Pressed += ApplyBatchAssign; batchInner.AddChild(applyBtn);
        c.AddChild(batchBar); c.AddChild(SP(4));

        // Smart arrange
        if (GM.CanAutoAssign)
        {
            var smartRow = new HBoxContainer(); var sc2 = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; smartRow.AddChild(sc2);
            var smartInner = new HBoxContainer(); sc2.AddChild(smartInner);
            var smartBtn = new Button { Text = "智能安排", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(140, 34) };
            smartBtn.AddThemeFontSizeOverride("font_size", 14); smartBtn.AddThemeColorOverride("font_color", UITheme.Gold); smartBtn.AddThemeColorOverride("font_hover_color", new Color(1,1,1));
            smartBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); smartBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
            smartBtn.Pressed += () => { AudioManager.PlayClick(); ApplySmart("proficiency"); RefreshDisciples(); };
            smartInner.AddChild(smartBtn);
            smartInner.AddChild(new Control { CustomMinimumSize = new Vector2I(12, 0) });
            var autoToggle = new CheckBox { ButtonPressed = GM.AutoAssignEnabled };
            autoToggle.Toggled += (on) => { GM.AutoAssignEnabled = on; };
            smartInner.AddChild(autoToggle);
            smartInner.AddChild(new Label { Text = "每日自动", VerticalAlignment = VerticalAlignment.Center }.WithFont(11, UITheme.TextDim));
            smartInner.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });
            var stratBtn = SmallBtn("策略"); stratBtn.AddThemeColorOverride("font_color", UITheme.TextBlue);
            stratBtn.Pressed += () => { _smartPopup.PopupCentered(); UIAnimator.WindowOpen((Control)_smartPopup.GetChild(0)); };
            smartInner.AddChild(stratBtn);
            c.AddChild(smartRow);
        }
        else
        {
            c.AddChild(new Label { Text = "🔒 修建藏经阁并达到宗门Lv.2解锁「智能安排」", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
        }
        c.AddChild(SP(8));

        int cols = Math.Min(3, Math.Max(1, GM.Disciples.Count));
        var cardGrid = new GridContainer { Columns = cols }; var gridWrap = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; gridWrap.AddChild(cardGrid); c.AddChild(gridWrap);
        foreach (var d in GM.Disciples.AllDisciples)
        {
            int did = d.Id; var card = MakeCard(220, 320); var cv = (VBoxContainer)card.GetChild(0);
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
        c.AddChild(HL($"灵筑与炼制", 18, UITheme.Gold)); c.AddChild(SP(8));

        // Crafting section
        {
            var craftGrid = new GridContainer { Columns = 2 }; c.AddChild(CenteredGrid(craftGrid));

            // Equipment crafting card
            var eqCard = MakeCard(300); var ecv = (VBoxContainer)eqCard.GetChild(0);
            ecv.AddChild(HL("⚒ 炼制法器", 16, UITheme.Gold)); ecv.AddChild(SP(6));
            ecv.AddChild(new Label { Text = "消耗矿石与灵石，炼制一件随机法器\n品质越高消耗越大，属性越强", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
            ecv.AddChild(SP(8));
            var eqRow = new HBoxContainer(); var eqc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; eqRow.AddChild(eqc);
            var eqInner = new HBoxContainer(); eqc.AddChild(eqInner);
            foreach (var qual in new[] { (EquipmentQuality.Common, "凡品", 10, 30), (EquipmentQuality.Uncommon, "灵品", 30, 100), (EquipmentQuality.Rare, "宝品", 80, 300) })
            {
                var qbtn = new Button { Text = $"{qual.Item2}\n矿{qual.Item3} 灵石{qual.Item4}", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(80, 44) };
                qbtn.AddThemeFontSizeOverride("font_size", 10); qbtn.AddThemeColorOverride("font_color", UITheme.TextPrimary); qbtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
                qbtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); qbtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
                var q = qual.Item1; int oc = qual.Item3; int sc = qual.Item4;
                qbtn.Pressed += () => { if (GM.Resources.Spend(ResourceType.Ore, oc) && GM.Resources.Spend(ResourceType.SpiritStone, sc)) { var ne = EquipmentTable.CraftRandom(GM.SectLevel); if (ne.Quality < q) { while (ne.Quality < q) ne.UpgradeQuality(); } GM.AllEquipment.Add(ne); AudioManager.PlayBuild(); RefreshFacilities(); } };
                eqInner.AddChild(qbtn); eqInner.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
            }
            ecv.AddChild(eqRow);
            craftGrid.AddChild(eqCard);

            // Pill crafting card
            var pillCard = MakeCard(300); var pcv = (VBoxContainer)pillCard.GetChild(0);
            pcv.AddChild(HL("🧪 炼制丹药", 16, UITheme.TextGreen)); pcv.AddChild(SP(6));
            pcv.AddChild(new Label { Text = "消耗灵草与灵石，炼制丹药\n可用于赠礼、突破消耗、弟子修炼", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
            pcv.AddChild(SP(8));
            var pillRow = new HBoxContainer(); var pillc = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; pillRow.AddChild(pillc);
            var pillInner = new HBoxContainer(); pillc.AddChild(pillInner);
            foreach (var pt in new[] { ("小还丹", 3, 5, 20), ("培元丹", 5, 12, 50), ("凝神丹", 8, 30, 120) })
            {
                var pbtn = new Button { Text = $"{pt.Item1}×{pt.Item2}\n草{pt.Item3} 灵石{pt.Item4}", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(85, 44) };
                pbtn.AddThemeFontSizeOverride("font_size", 10); pbtn.AddThemeColorOverride("font_color", UITheme.TextPrimary); pbtn.AddThemeColorOverride("font_hover_color", UITheme.Gold);
                pbtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); pbtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
                int hc = pt.Item3; int psc = pt.Item4; int pcount = pt.Item2;
                pbtn.Pressed += () => { if (GM.Resources.Spend(ResourceType.Herb, hc) && GM.Resources.Spend(ResourceType.SpiritStone, psc)) { GM.Resources.Add(ResourceType.Pill, pcount); AudioManager.PlayBuild(); RefreshFacilities(); } };
                pillInner.AddChild(pbtn); pillInner.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
            }
            pcv.AddChild(pillRow);
            craftGrid.AddChild(pillCard);
        }

        c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(8));
        c.AddChild(HL($"已建灵筑（{GM.Facilities.AllFacilities.Count(f => f.IsBuilt)}座）", 16, UITheme.Gold)); c.AddChild(SP(8));
        if (GM.Facilities.Count == 0) { c.AddChild(TB("尚未营造灵筑。切换到「营造」页开始建设。", UITheme.TextDim, 13)); return; }

        foreach (var f in GM.Facilities.AllFacilities.OrderByDescending(f => f.Level))
        {
            var card = MakeCard(600); card.CustomMinimumSize = new Vector2I(550, 0); var cv = (VBoxContainer)card.GetChild(0);

            // Top row: icon + name/status + buttons
            var topRow = new HBoxContainer();
            var facTex = SpriteSheetManager.GetFacilityIcon(f.Type);
            if (facTex != null) { var ir = new TextureRect { Texture = facTex, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2I(48, 48) }; topRow.AddChild(ir); topRow.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) }); }

            var mid = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            mid.AddChild(new Label { Text = $"{f.TypeName} Lv.{f.Level}" }.WithFont(16, UITheme.Gold));
            mid.AddChild(new Label { Text = f.StatusText }.WithFont(11, f.IsBuilt ? UITheme.TextGreen : f.IsUnderConstruction ? UITheme.TextOrange : UITheme.TextDim));
            topRow.AddChild(mid);

            // Buttons
            if (f.IsBuilt)
            {
                var mgrBtn = SmallBtn("管理"); mgrBtn.AddThemeColorOverride("font_color", UITheme.Gold);
                var fid2 = f.Id; mgrBtn.Pressed += () => { var ff = GM.Facilities.Get(fid2); if (ff != null) ShowFacilityDetail(ff); };
                topRow.AddChild(mgrBtn);
                topRow.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
                if (f.Level < f.MaxLevel)
                {
                    int cost = FacilityTable.GetUpgradeCost(f.Type, f.Level);
                    var upBtn = SmallBtn($"晋升{cost}灵石");
                    int fid3 = f.Id; upBtn.Pressed += () => GM.UpgradeFacility(fid3);
                    topRow.AddChild(upBtn);
                }
            }
            cv.AddChild(topRow);

            // Detail row
            if (f.IsBuilt)
            {
                cv.AddChild(SP(6));
                var detailRow = new HBoxContainer();
                int outVal = FacilityTable.GetOutput(f.Type, f.Level);
                string outName = ResName(FacilityTable.GetInfo(f.Type).OutputType);
                detailRow.AddChild(new Label { Text = $"产出: {outName}+{outVal}/日" }.WithFont(12, UITheme.TextPrimary));
                detailRow.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
                detailRow.AddChild(new Label { Text = $"容纳: {f.MaxDisciples}人" }.WithFont(12, UITheme.TextDim));
                detailRow.AddChild(new Control { CustomMinimumSize = new Vector2I(12, 0) });
                string bonusText = "";
                foreach (DiscipleTaskType tt in Enum.GetValues<DiscipleTaskType>())
                {
                    double tb = FacilityTable.GetTaskBonus(f.Type, f.Level, tt);
                    if (tb > 0) { bonusText = $"加成: {TaskNames[(int)tt]}+{tb*100:F0}%"; break; }
                }
                if (!string.IsNullOrEmpty(bonusText))
                    detailRow.AddChild(new Label { Text = bonusText }.WithFont(12, UITheme.TextGreen));
                cv.AddChild(detailRow);
            }
            else if (f.IsUnderConstruction)
            {
                cv.AddChild(SP(4));
                string sl = f.IsUpgrading ? "晋升中" : "营造中";
                cv.AddChild(new Label { Text = $"{sl} {f.ConstructionProgress}/{FacilityTable.GetInfo(f.Type).BuildDays}日" }.WithFont(12, UITheme.TextOrange));
            }

            var wrapper = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; wrapper.AddChild(card);
            c.AddChild(wrapper);
            c.AddChild(SP(6));
        }
    }

    // ===================== TAB: COMPANIONS (4) =====================
    // ===================== TAB: COMPANIONS (4) =====================

    void RefreshCompanions()
    {
        var c = _tabContents[4]; c.FreeChildren(); var allComps = GM.Companions.AllCompanions;
        c.AddChild(HL($"道缘谱（{allComps.Count}对）", 18, UITheme.Gold)); c.AddChild(SP(10));

        if (allComps.Count > 0)
        {
            foreach (var comp in allComps)
            {
                var d1 = GM.Disciples.Get(comp.DiscipleId1); var d2 = GM.Disciples.Get(comp.DiscipleId2);
                if (d1 == null || d2 == null) continue;
                var card = MakeCard(500); card.SizeFlagsHorizontal = SizeFlags.ExpandFill; var cv = (VBoxContainer)card.GetChild(0);

                // Header: affection + status
                var hdr = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                hdr.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
                Color affC = comp.Affection >= 80 ? new Color(1,0.4f,0.6f) : comp.Affection >= 60 ? new Color(1,0.6f,0.3f) : UITheme.TextDim;
                hdr.AddChild(new Label { Text = $"好感 {comp.Affection:F0}/100" }.WithFont(12, affC));
                hdr.AddChild(new Control { CustomMinimumSize = new Vector2I(10, 0) });
                Color stC = comp.IsMarried ? new Color(1,0.5f,0.7f) : new Color(1,0.6f,0.3f);
                hdr.AddChild(new Label { Text = comp.IsMarried ? "[ 已结道缘 ]" : "[ 结缘中 ]" }.WithFont(12, stC));
                cv.AddChild(hdr); cv.AddChild(SP(4));

                // Affection bar
                var affBar = new ColorRect { CustomMinimumSize = new Vector2I((int)(comp.Affection * 5), 4) };
                affBar.Color = affC;
                cv.AddChild(affBar); cv.AddChild(SP(8));

                // Two disciple panels
                var bothRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };

                // -- Left --
                var lp = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                var lac = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; lac.AddChild(MakeAvatarCircle(d1.IsMale, 44)); lp.AddChild(lac); lp.AddChild(SP(3));
                lp.AddChild(new Label { Text = (d1.IsMale?"♂":"♀") + " " + d1.Name, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.Gold));
                lp.AddChild(new Label { Text = d1.FullRealmName, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, RealmColor(d1.Realm)));
                lp.AddChild(new Label { Text = "天赋"+d1.Talent+" 悟性"+d1.Comprehension, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextDim));
                lp.AddChild(new Label { Text = "体质"+d1.Constitution+" 神识"+d1.Spirit, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextDim));
                lp.AddChild(new Label { Text = "战力"+d1.CombatPower+" 修为"+d1.CultivationProgress.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextPrimary));
                lp.AddChild(new Label { Text = "心情"+BarsString(d1.Mood,100,6)+" "+d1.Mood.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, d1.Mood<20?UITheme.Crimson:UITheme.TextDim));
                lp.AddChild(new Label { Text = "体力"+BarsString(d1.CurrentStamina,d1.MaxStamina,6)+" "+d1.CurrentStamina+"/"+d1.MaxStamina, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, d1.CurrentStamina<=10?UITheme.TextOrange:UITheme.TextDim));
                lp.AddChild(new Label { Text = "任务: "+TaskNames[(int)d1.CurrentTask], HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextBlue));
                if (!string.IsNullOrEmpty(d1.Background)) lp.AddChild(new Label { Text = d1.Background, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, new Color(0.65f,0.55f,0.75f)));
                bothRow.AddChild(lp);

                // Heart
                var heartCol = new VBoxContainer { CustomMinimumSize = new Vector2I(40, 0) };
                var heartFill = new CenterContainer { SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
                var heart = new Label { Text = "❤", HorizontalAlignment = HorizontalAlignment.Center };
                heart.AddThemeFontSizeOverride("font_size", 22); heart.AddThemeColorOverride("font_color", new Color(1,0.35f,0.45f));
                heartFill.AddChild(heart); heartCol.AddChild(heartFill);
                bothRow.AddChild(heartCol);

                // -- Right --
                var rp = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
                var rac = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; rac.AddChild(MakeAvatarCircle(d2.IsMale, 44)); rp.AddChild(rac); rp.AddChild(SP(3));
                rp.AddChild(new Label { Text = (d2.IsMale?"♂":"♀") + " " + d2.Name, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.Gold));
                rp.AddChild(new Label { Text = d2.FullRealmName, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, RealmColor(d2.Realm)));
                rp.AddChild(new Label { Text = "天赋"+d2.Talent+" 悟性"+d2.Comprehension, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextDim));
                rp.AddChild(new Label { Text = "体质"+d2.Constitution+" 神识"+d2.Spirit, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextDim));
                rp.AddChild(new Label { Text = "战力"+d2.CombatPower+" 修为"+d2.CultivationProgress.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextPrimary));
                rp.AddChild(new Label { Text = "心情"+BarsString(d2.Mood,100,6)+" "+d2.Mood.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, d2.Mood<20?UITheme.Crimson:UITheme.TextDim));
                rp.AddChild(new Label { Text = "体力"+BarsString(d2.CurrentStamina,d2.MaxStamina,6)+" "+d2.CurrentStamina+"/"+d2.MaxStamina, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, d2.CurrentStamina<=10?UITheme.TextOrange:UITheme.TextDim));
                rp.AddChild(new Label { Text = "任务: "+TaskNames[(int)d2.CurrentTask], HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, UITheme.TextBlue));
                if (!string.IsNullOrEmpty(d2.Background)) rp.AddChild(new Label { Text = d2.Background, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, new Color(0.65f,0.55f,0.75f)));
                bothRow.AddChild(rp);
                cv.AddChild(bothRow); cv.AddChild(SP(6));

                // Dual cultivation info
                bool bc = d1.CurrentTask == DiscipleTaskType.Cultivate && d2.CurrentTask == DiscipleTaskType.Cultivate;
                var dl = new Label { Text = bc ? $"双修加成: +{comp.DualCultivationBonus*100:F0}% (修炼中)" : $"双修加成: +{comp.DualCultivationBonus*100:F0}% (需同修)", HorizontalAlignment = HorizontalAlignment.Center };
                dl.AddThemeFontSizeOverride("font_size", 11); dl.AddThemeColorOverride("font_color", bc ? UITheme.TextGreen : UITheme.TextDim);
                cv.AddChild(dl); cv.AddChild(SP(6));

                // Action buttons
                var br = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
                int cId = comp.Id;
                var g1b = SmallBtn("赠丹药"); g1b.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.Pill, 1); br.AddChild(g1b);
                br.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
                var g2b = SmallBtn("赠灵石"); g2b.Pressed += () => GM.GiveGiftToCompanion(cId, ResourceType.SpiritStone, 50); br.AddChild(g2b);
                if (!comp.IsMarried) { br.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) }); var mb = SmallBtn("结道缘"); mb.Disabled = comp.Affection < 60; mb.Pressed += () => GM.ProposeMarriage(cId); br.AddChild(mb); }
                br.AddChild(new Control { CustomMinimumSize = new Vector2I(4, 0) });
                var bb = SmallBtn("和离"); bb.Pressed += () => GM.BreakUpCompanion(cId); br.AddChild(bb); cv.AddChild(br);

                c.AddChild(card); c.AddChild(SP(8));
            }
            c.AddChild(HR()); c.AddChild(SP(4));
        }
        else { c.AddChild(TB("尚无道缘。使用下方功能为弟子牵线搭桥。", UITheme.TextDim, 13)); c.AddChild(SP(10)); }

        // Matchmaking
        c.AddChild(HL("牵线搭桥", 16, UITheme.Gold)); c.AddChild(SP(8));
        var singles = GM.Disciples.AllDisciples.Where(d => d.CompanionId < 0).ToList(); var males = singles.Where(d => d.IsMale).ToList(); var females = singles.Where(d => !d.IsMale).ToList();
        if (males.Count == 0 || females.Count == 0) { c.AddChild(TB(males.Count+females.Count == 0 ? "暂无单身弟子。" : "性别比例失衡。", UITheme.TextDim, 12)); return; }

        // Selection row
        var selRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
        selRow.AddChild(new Label { Text = "男弟子:" }.WithFont(14, UITheme.TextPrimary));
        selRow.AddChild(new Control { CustomMinimumSize = new Vector2I(6, 0) });
        _matchMaleDrop = new OptionButton(); foreach (var m in males) _matchMaleDrop.AddItem(m.Name+" ("+m.FullRealmName+")"); selRow.AddChild(_matchMaleDrop);
        selRow.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });
        selRow.AddChild(new Label { Text = "❤" }.WithFont(22, new Color(1,0.4f,0.5f)));
        selRow.AddChild(new Control { CustomMinimumSize = new Vector2I(16, 0) });
        selRow.AddChild(new Label { Text = "女弟子:" }.WithFont(14, UITheme.TextPrimary));
        selRow.AddChild(new Control { CustomMinimumSize = new Vector2I(6, 0) });
        _matchFemaleDrop = new OptionButton(); foreach (var f in females) _matchFemaleDrop.AddItem(f.Name+" ("+f.FullRealmName+")"); selRow.AddChild(_matchFemaleDrop);
        selRow.AddChild(new Control { CustomMinimumSize = new Vector2I(12, 0) });
        var ib = new Button { Text = "牵 线", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(100, 38) }; ib.AddThemeFontSizeOverride("font_size", 15); ib.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); ib.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover()); ib.AddThemeColorOverride("font_color", UITheme.Gold);
        ib.Pressed += () => { if (_matchMaleDrop.Selected < 0 || _matchFemaleDrop.Selected < 0) return; GM.IntroduceCompanions(males[_matchMaleDrop.Selected].Id, females[_matchFemaleDrop.Selected].Id); };
        selRow.AddChild(ib);
        c.AddChild(selRow); c.AddChild(SP(8));

        // Detail panels for selected disciples
        var detailRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        var md = _matchMaleDrop.Selected >= 0 ? males[_matchMaleDrop.Selected] : null;
        var fd = _matchFemaleDrop.Selected >= 0 ? females[_matchFemaleDrop.Selected] : null;

        // Build a detail panel for a disciple
        VBoxContainer BuildDiscPanel(DiscipleData? d)
        {
            var p = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            if (d == null) { p.AddChild(new Label { Text = "请选择弟子", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(14, UITheme.TextDim)); return p; }
            var avC = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; avC.AddChild(MakeAvatarCircle(d.IsMale, 68)); p.AddChild(avC); p.AddChild(SP(4));
            string gi = d.IsMale ? "♂" : "♀";
            p.AddChild(new Label { Text = gi+" "+d.Name, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(17, UITheme.Gold));
            p.AddChild(new Label { Text = d.FullRealmName, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, RealmColor(d.Realm)));
            p.AddChild(SP(2));
            p.AddChild(new Label { Text = "天赋"+d.Talent+"  悟性"+d.Comprehension, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
            p.AddChild(new Label { Text = "体质"+d.Constitution+"  神识"+d.Spirit, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
            p.AddChild(SP(2));
            p.AddChild(new Label { Text = "战力"+d.CombatPower+"  修为"+d.CultivationProgress.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextPrimary));
            p.AddChild(new Label { Text = "心情"+BarsString(d.Mood,100,8)+" "+d.Mood.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, d.Mood<20?UITheme.Crimson:UITheme.TextDim));
            p.AddChild(new Label { Text = "体力"+BarsString(d.CurrentStamina,d.MaxStamina,8)+" "+d.CurrentStamina+"/"+d.MaxStamina, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, d.CurrentStamina<=10?UITheme.TextOrange:UITheme.TextDim));
            p.AddChild(SP(2));
            p.AddChild(new Label { Text = "任务: "+TaskNames[(int)d.CurrentTask], HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextBlue));
            if (!string.IsNullOrEmpty(d.Background)) p.AddChild(new Label { Text = d.Background, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, new Color(0.65f,0.55f,0.75f)));
            if (d.Trait != "无" && !string.IsNullOrEmpty(d.Trait)) p.AddChild(new Label { Text = d.Trait, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextOrange));
            return p;
        }

        var mlp = BuildDiscPanel(md); var mrp = BuildDiscPanel(fd);
        detailRow.AddChild(mlp);
        detailRow.AddChild(new Control { CustomMinimumSize = new Vector2I(12, 0) });
        detailRow.AddChild(mrp);
        c.AddChild(detailRow);
        c.AddChild(SP(4));

        // Update panels when dropdown changes
        _matchMaleDrop.ItemSelected += (_) => { if (_matchMaleDrop.Selected >= 0) { mlp.FreeChildren(); var np = BuildDiscPanel(males[_matchMaleDrop.Selected]); foreach (var ch in np.GetChildren().ToList()) { np.RemoveChild(ch); mlp.AddChild(ch); } np.QueueFree(); } };
        _matchFemaleDrop.ItemSelected += (_) => { if (_matchFemaleDrop.Selected >= 0) { mrp.FreeChildren(); var np = BuildDiscPanel(females[_matchFemaleDrop.Selected]); foreach (var ch in np.GetChildren().ToList()) { np.RemoveChild(ch); mrp.AddChild(ch); } np.QueueFree(); } };

        c.AddChild(SP(4));
        c.AddChild(TB("成功率受境界相近度、属性互补、年龄相仿、忠诚度影响。", UITheme.TextDim, 11));
    }

    // ===================== TAB: QUESTS (5) =====================

    void RefreshQuests()
    {
        var c = _tabContents[5]; c.FreeChildren();
        int completed = GM.Quests.AllQuests.Count(q => q.Completed);
        int maxTier = SectQuestSystem.GetMaxTier(GM);
        string tierDesc = maxTier switch { 1 => "凡品", 2 => "凡品·良品", 3 => "凡品~优品", _ => "凡品~极品" };
        c.AddChild(HL($"门令（{completed}/{GM.Quests.AllQuests.Count}完成）", 18, UITheme.Gold));
        c.AddChild(TB($"可接品级: {tierDesc}", UITheme.TextDim, 11));
        c.AddChild(SP(10));
        if (GM.Quests.AllQuests.Count == 0) { c.AddChild(TB("暂无门令。", UITheme.TextDim, 13)); return; }

        foreach (var q in GM.Quests.AllQuests)
        {
            var card = MakeCard(500); card.SizeFlagsHorizontal = SizeFlags.ExpandFill; var cv = (VBoxContainer)card.GetChild(0);

            // Top row: title + tier badge + status
            var topRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var tierBadge = new Label { Text = $" [{q.TierLabel}] " };
            tierBadge.AddThemeFontSizeOverride("font_size", 11); tierBadge.AddThemeColorOverride("font_color", q.TierColor);
            topRow.AddChild(tierBadge);
            topRow.AddChild(new Label { Text = q.Title }.WithFont(15, q.Completed ? UITheme.TextGreen : UITheme.Gold));
            topRow.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
            topRow.AddChild(new Label { Text = q.Completed ? "✓ 已完成" : "进行中" }.WithFont(10, q.Completed ? UITheme.TextGreen : UITheme.TextOrange));
            cv.AddChild(topRow);

            cv.AddChild(new Label { Text = q.Description }.WithFont(12, UITheme.TextPrimary));
            cv.AddChild(SP(4));

            // Progress bar
            var progRow = new HBoxContainer();
            progRow.AddChild(new Label { Text = $"进度: " }.WithFont(11, UITheme.TextDim));
            double ratio = q.TargetCount > 0 ? Math.Clamp((double)q.CurrentCount / q.TargetCount, 0, 1) : 0;
            var bar = new ColorRect { CustomMinimumSize = new Vector2I((int)(ratio * 300), 8), Color = q.Completed ? UITheme.TextGreen : UITheme.TextBlue };
            progRow.AddChild(bar);
            progRow.AddChild(new Label { Text = $" {q.CurrentCount}/{q.TargetCount}" }.WithFont(11, UITheme.TextDim));
            cv.AddChild(progRow);

            cv.AddChild(SP(4));
            cv.AddChild(new Label { Text = $"赏赐: {q.RewardText}" }.WithFont(11, UITheme.TextGreen));
            c.AddChild(card); c.AddChild(SP(6));
        }
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
        foreach (var kv in rc.OrderByDescending(kv => (int)kv.Key)) c.AddChild(new Label { Text = $"{RealmNameCN(kv.Key)}: {kv.Value}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, RealmColor(kv.Key)));
        c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));
        c.AddChild(HL("门令分布", 16, UITheme.Gold)); c.AddChild(SP(6));
        var tc = new int[9]; foreach (var d in GM.Disciples.AllDisciples) tc[(int)d.CurrentTask]++; for (int i = 0; i < TaskNames.Length; i++) if (tc[i] > 0) c.AddChild(new Label { Text = $"{TaskNames[i]}: {tc[i]}人", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextPrimary));

        // Achievements
        c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(10));
        c.AddChild(HL($"成就 ({GM.Achievements.Progress.TotalUnlocked}/{GM.Achievements.TotalCount})", 16, UITheme.Gold)); c.AddChild(SP(6));
        var catNames = new Dictionary<AchievementCategory, string> {
            [AchievementCategory.SectGrowth] = "宗门发展", [AchievementCategory.DiscipleTraining] = "弟子培养",
            [AchievementCategory.ResourceWealth] = "资源积累", [AchievementCategory.PlotProgress] = "剧情推进",
            [AchievementCategory.Companionship] = "道侣情缘", [AchievementCategory.Exploration] = "秘境探索",
            [AchievementCategory.Challenge] = "挑战成就"
        };
        foreach (var cat in catNames)
        {
            var achievements = AchievementTable.All.Where(a => a.Category == cat.Key).ToList();
            int unlocked = achievements.Count(a => GM.Achievements.Progress.UnlockedIds.Contains(a.Id));
            c.AddChild(new Label { Text = $"{cat.Value}: {unlocked}/{achievements.Count}", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
            foreach (var a in achievements)
            {
                bool done = GM.Achievements.Progress.UnlockedIds.Contains(a.Id);
                string line = done ? $"✓ {a.Title}" : (a.IsHidden ? "??? 未解锁" : $"○ {a.Title} — {a.Description}");
                c.AddChild(new Label { Text = line, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(10, done ? UITheme.TextGreen : UITheme.TextDim));
            }
        }
    }

        // ===================== TAB: PLOT (8) =====================

    void RefreshPlot()
    {
        var c = _tabContents[8]; c.FreeChildren();
        c.AddChild(HL("仙途纪事", 18, UITheme.Gold)); c.AddChild(SP(10));
        var stage = GM.Plot.ActiveStage;
        var last = GM.Plot.LastCompletedStage;
        if (stage != null)
        {
            c.AddChild(HL(stage.ChapterTitle, 14, UITheme.TextOrange)); c.AddChild(SP(6));
            c.AddChild(HL(stage.Title, 20, UITheme.Gold)); c.AddChild(SP(12));
            var narrative = new RichTextLabel { BbcodeEnabled = true, FitContent = true, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            narrative.AddThemeFontSizeOverride("normal_font_size", 14); narrative.AddThemeColorOverride("default_color", UITheme.TextPrimary);
            narrative.Text = stage.Narrative.Replace("\n", "\n\n");
            c.AddChild(narrative); c.AddChild(SP(16));
            c.AddChild(HR()); c.AddChild(SP(8));
            c.AddChild(HL("当前目标", 16, UITheme.Gold)); c.AddChild(SP(6));
            c.AddChild(new Label { Text = stage.Objective, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(15, UITheme.TextBlue));
            c.AddChild(SP(4));
            c.AddChild(new Label { Text = stage.CompletionHint, HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }.WithFont(12, UITheme.TextDim));
            if (!stage.IsManualAcknowledge)
            {
                c.AddChild(SP(8));
                foreach (var cond in stage.CompletionConditions)
                {
                    string prog = GetConditionProgress(cond);
                    var pl = new Label { Text = prog, HorizontalAlignment = HorizontalAlignment.Center };
                    pl.AddThemeFontSizeOverride("font_size", 12);
                    pl.AddThemeColorOverride("font_color", IsConditionMet(cond) ? UITheme.TextGreen : UITheme.TextDim);
                    c.AddChild(pl);
                }
            }
            c.AddChild(SP(12));
            if (stage.IsManualAcknowledge)
            {
                var ackBtn = new Button { Text = "确认 · 踏上仙途", Alignment = HorizontalAlignment.Center, CustomMinimumSize = new Vector2I(240, 48) };
                ackBtn.AddThemeFontSizeOverride("font_size", 18); ackBtn.AddThemeColorOverride("font_color", UITheme.Gold);
                ackBtn.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
                ackBtn.AddThemeStyleboxOverride("normal", UITheme.BtnStyleNormal()); ackBtn.AddThemeStyleboxOverride("hover", UITheme.BtnStyleHover());
                ackBtn.Pressed += () => { AudioManager.PlayClick(); GM.Plot.AcknowledgeStage(GM); RefreshPlot(); RefreshOverview(); };
                var ackC = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; ackC.AddChild(ackBtn); c.AddChild(ackC);
            }
            else c.AddChild(new Label { Text = "（目标达成后将自动推进剧情）", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(11, UITheme.TextDim));
        }
        else if (last != null && GM.Plot.Progress.CompletedStageCount >= PlotTable.AllStages.Count)
        {
            var allChapters = PlotTable.AllStages.Select(s => s.ChapterTitle).Distinct().ToList();
            c.AddChild(HL(string.Join(" · ", allChapters) + " —— 全部完成", 18, UITheme.Gold)); c.AddChild(SP(10));
            c.AddChild(new Label { Text = "宗门在你的带领下从荒山小派一路成长为名动一方的修仙势力。\n\n修真界浩瀚无垠前方还有无尽的机遇与挑战。\n\n后续剧情卷册正在编纂中，敬请期待...", HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart }.WithFont(14, UITheme.TextPrimary));
        }
        else c.AddChild(new Label { Text = "暂无进行中的剧情。", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextDim));

        c.AddChild(SP(10)); c.AddChild(HR()); c.AddChild(SP(8));
        c.AddChild(HL("已完成章节", 16, UITheme.Gold)); c.AddChild(SP(6));
        if (GM.Plot.Progress.CompletedStageIds.Count > 0)
        {
            foreach (int sid in GM.Plot.Progress.CompletedStageIds.OrderBy(x => x))
            {
                var sd = PlotTable.Get(sid);
                if (sd != null) c.AddChild(new Label { Text = "\u2713 " + sd.Title, HorizontalAlignment = HorizontalAlignment.Center }.WithFont(13, UITheme.TextGreen));
            }
        }
        else c.AddChild(new Label { Text = "尚未完成任何章节", HorizontalAlignment = HorizontalAlignment.Center }.WithFont(12, UITheme.TextDim));
    }

    string GetConditionProgress(PlotCondition cond)
    {
        switch (cond.Type)
        {
            case PlotConditionType.DiscipleCount: return $"招募弟子: {GM.Disciples.Count}/{cond.TargetValue}";
            case PlotConditionType.FacilityCount: int built = GM.Facilities.AllFacilities.Count(f => f.IsBuilt); return $"已建灵筑: {built}/{cond.TargetValue}";
            case PlotConditionType.DiscipleRealm: var realm = (CultivationRealm)cond.TargetValue; string rn2 = realm switch { CultivationRealm.QiRefining => "练气期", CultivationRealm.Foundation => "筑基期", CultivationRealm.CoreFormation => "金丹期", _ => realm.ToString() }; bool has = GM.Disciples.AllDisciples.Any(d => (int)d.Realm >= cond.TargetValue); return $"拥有{rn2}弟子: {(has ? "已达成" : "未达成")}";
            case PlotConditionType.Reputation: return $"声望: {GM.SectReputation}/{cond.TargetValue}";
            case PlotConditionType.ResourceAmount: int val = GM.Resources.Get(cond.ResourceType); string rn3 = cond.ResourceType switch { ResourceType.SpiritStone => "灵石", ResourceType.Herb => "灵草", ResourceType.Ore => "矿石", ResourceType.Pill => "丹药", _ => cond.ResourceType.ToString() }; return $"{rn3}: {val}/{cond.TargetValue}";
            case PlotConditionType.SectLevel: return $"宗门等级: {GM.SectLevel}/{cond.TargetValue}";
            default: return "";
        }
    }

    bool IsConditionMet(PlotCondition cond)
    {
        switch (cond.Type)
        {
            case PlotConditionType.DiscipleCount: return GM.Disciples.Count >= cond.TargetValue;
            case PlotConditionType.FacilityCount: return GM.Facilities.AllFacilities.Count(f => f.IsBuilt) >= cond.TargetValue;
            case PlotConditionType.DiscipleRealm: return GM.Disciples.AllDisciples.Any(d => (int)d.Realm >= cond.TargetValue);
            case PlotConditionType.Reputation: return GM.SectReputation >= cond.TargetValue;
            case PlotConditionType.ResourceAmount: return GM.Resources.Get(cond.ResourceType) >= cond.TargetValue;
            case PlotConditionType.SectLevel: return GM.SectLevel >= cond.TargetValue;
            default: return false;
        }
    }

}
