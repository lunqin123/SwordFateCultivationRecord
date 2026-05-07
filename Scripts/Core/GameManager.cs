namespace SwordFateCultivationRecord;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; } = null!;

	// Subsystems
	public TimeSystem Time { get; private set; } = new();
	public ResourceSystem Resources { get; private set; } = new();
	public DiscipleSystem Disciples { get; private set; } = new();
	public FacilitySystem Facilities { get; private set; } = new();
	public EventSystem Events { get; private set; } = new();
	public SaveLoadManager SaveLoad { get; private set; } = new();
	public CompanionSystem Companions { get; private set; } = new();
	public SectQuestSystem Quests { get; private set; } = new();
	public PlotSystem Plot { get; private set; } = new();
	public SecretRealmSystem Realm { get; private set; } = new();
	public AchievementSystem Achievements { get; private set; } = new();
	public CombatSystem Combat { get; private set; } = new();
	public List<EquipmentData> AllEquipment { get; private set; } = new();

	// Sect state
	public string SectName { get; set; } = "无名剑宗";
	public int SectLevel { get; set; } = 1;
	public int SectReputation { get; set; } = 0;
	public int SectPower { get; set; } = 0;
	public int MaxDisciples { get; set; } = 5;
	public int OuterDiscipleCount { get; set; }
	public int MaxOuterDisciples => SectLevel * 15 + 5 + Facilities.AllFacilities.Where(f => f.IsBuilt).Sum(f => f.ManagementBonus);

	// 外门分工比例 (0-100, 总和不超过100, 余数为待命)
	public int OuterGatherRatio = 60;  // 采集：产灵草+矿石
	public int OuterTradeRatio = 20;   // 经商：产灵石
	// 待命(闲置): 100 - gather - trade, 不产出也不消耗
	public int OuterIdleRatio => 100 - OuterGatherRatio - OuterTradeRatio;

	public int OuterGatherCount => OuterDiscipleCount * OuterGatherRatio / 100;
	public int OuterTradeCount => OuterDiscipleCount * OuterTradeRatio / 100;
	public int OuterIdleCount => OuterDiscipleCount - OuterGatherCount - OuterTradeCount;

	public double OuterEfficiency => OuterDiscipleCount <= MaxOuterDisciples ? 1.0 : Math.Max(0.3, 1.0 - (OuterDiscipleCount - MaxOuterDisciples) * 0.05);
	private double _herbAccum, _oreAccum, _tradeAccum;

	public bool IsInitialized { get; private set; }
	public EventData? PendingEvent { get; private set; }

	// 内门选拔
	public int RecruitTournamentDays { get; internal set; } = -1; // -1 = 未安排
	public List<DiscipleData>? PendingRecruitCandidates { get; private set; }
	public int TournamentPicksRemaining { get; private set; }

	// 自动智能安排 — 需藏经阁 + Lv.2以上解锁
	public bool AutoAssignEnabled { get; set; }
	public bool CanAutoAssign => SectLevel >= 2 && Facilities.AllFacilities.Any(f => f.IsBuilt && f.Type == FacilityType.Library);

	public int RngSeed { get; private set; } = System.Environment.TickCount;
	private Random _rng = new(0);  // reset in InitializeNewGame or ApplyRngSeed
	private readonly List<DiscipleData> _pendingNewborns = new();
	public List<DiscipleData> Children { get; set; } = new();
	public List<LogEntry> EventLogEntries { get; set; } = new();

	// ====== Level thresholds ======
	private static readonly int[] LevelReq = { 0, 100, 300, 600, 1000, 2000, 2500 };
	private static readonly int[] MaxDiscPerLevel = { 5, 8, 12, 18, 25, 35, 50 };

	public string SectTitle => SectReputation switch
	{
		< 50 => "无名小宗",
		< 150 => "初露锋芒",
		< 400 => "小有名气",
		< 800 => "一方豪强",
		< 1500 => "名门大派",
		< 3000 => "威震四方",
		< 6000 => "仙道圣地",
		_ => "万宗至尊"
	};

	public string FullSectName => $"「{SectName}」· {SectTitle}";

	public override void _Ready()
	{
		if (Instance != null) { QueueFree(); return; }
		Instance = this;
		InitializeNewGame();
	}

	public void InitializeNewGame()
	{
		EventBus.Clear();
		AudioManager.ConnectEvents();
		EventBus.ChildBorn += (child, _, _) => _pendingNewborns.Add(child);
		EventBus.DiscipleRecruited += d => LogEvent($"{d.Name}加入宗门");
		EventBus.BreakthroughSuccess += d => LogEvent($"{d.Name}成功突破至{d.FullRealmName}！");
		EventBus.BreakthroughFailed += d => LogEvent($"{d.Name}突破失败，修为受损");
		EventBus.YearPassed += ShowYearSummary;
		EventBus.DiscipleDeparted += OnDiscipleDeparted;
		Time = new TimeSystem();
		Resources = new ResourceSystem();
		Disciples = new DiscipleSystem();
		Facilities = new FacilitySystem();
		Events = new EventSystem();
		Companions = new CompanionSystem();
		Quests = new SectQuestSystem();
		Plot = new PlotSystem();
		Quests.Initialize();
		Plot.Initialize();
		AllEquipment = new List<EquipmentData>();
		SectName = "无名剑宗";
		SectLevel = 1;
		SectReputation = 0;
		SectPower = 0;
		MaxDisciples = MaxDiscPerLevel[0];
		PendingEvent = null;
		RecruitTournamentDays = -1;
		PendingRecruitCandidates = null;
		EventLogEntries.Clear();
		_pendingNewborns.Clear();
		Children = new List<DiscipleData>();
		_herbAccum = _oreAccum = _tradeAccum = 0;
		_outerGrowthAccum = _outerPromoteAccum = 0;
		AutoAssignEnabled = false;
		RngSeed = System.Environment.TickCount;
		_rng = new Random(RngSeed);
		OuterDiscipleCount = 3 + _rng.Next(0, 3);
		// Starting equipment
		AllEquipment.Add(new EquipmentData { Id = 1, Name = "铁剑", Quality = EquipmentQuality.Common, Description = "宗门传承的铁剑", CombatBonus = 5 });
		AllEquipment.Add(new EquipmentData { Id = 2, Name = "静心蒲团", Quality = EquipmentQuality.Common, Description = "辅助修炼的蒲团", CultivationSpeedBonus = 0.05 });
		for (int i = 0; i < 1; i++) Disciples.Recruit();
		LogEvent($"宗门「{SectName}」于第{Time.Year}年创立");
		IsInitialized = true;
	}

	/// <summary>Record an event in the sect chronicle (max 300 entries).</summary>
	public void LogEvent(string message)
	{
		EventLogEntries.Add(new LogEntry { Title = "记事", Message = message, Day = Time.Day });
		if (EventLogEntries.Count > 300) EventLogEntries.RemoveAt(0);
	}

	// ====== Daily Pipeline ======

	public void AdvanceTime(int days)
	{
		if (!IsInitialized || PendingEvent != null || PendingRecruitCandidates != null) return;
		for (int i = 0; i < days; i++)
		{
			RunDailyCycle();
			if (PendingEvent != null || PendingRecruitCandidates != null) return;
			if (CheckGameOver()) return;
		}
	}

	public void FastForward(int days)
	{
		if (!IsInitialized || PendingEvent != null || PendingRecruitCandidates != null) return;
		for (int i = 0; i < days; i++)
		{
			RunDailyCycle();
			if (PendingEvent != null || PendingRecruitCandidates != null) return;
			if (CheckGameOver()) return;
		}
	}

	private void RunDailyCycle()
	{
		// 1. Facility construction progress & income calc
		Facilities.ProcessDaily(Resources);
		// 2. Resource income applied
		Resources.ApplyDailyIncome();
		ProcessOuterDisciples();
		// 3. Companions: affection, mood, and dual cultivation bonuses
		var compBonus = Companions.ProcessDaily(Disciples.AllDisciples.ToList(), Time.GetTotalDays());
		Companions.TryRandomPairing(Disciples.AllDisciples.ToList());
		// 4. Disciples: task effects + stamina
		int powerGain = Disciples.ProcessDaily(Resources, Facilities.AllFacilities, SectLevel, compBonus);
		SectPower += powerGain;
		AllEquipment.AddRange(Disciples.NewEquipmentToday);

		// 4.5 Auto-assign: reassign disciples based on condition
		if (AutoAssignEnabled && CanAutoAssign)
			RunAutoAssign();

		HandleNewborns();

		// 5. Event cooldowns
		Events.ProcessCooldowns();

		// 6. 内门选拔倒计时
		if (RecruitTournamentDays > 0)
		{
			RecruitTournamentDays--;
			if (RecruitTournamentDays == 0)
			{
				// Tournament ends — generate quality candidates based on reputation
				PendingRecruitCandidates = GenerateRecruitCandidates(SectReputation);
				TournamentPicksRemaining = TournamentPickCount;
				EventBus.EmitRecruitSelectionReady(PendingRecruitCandidates);
				return; // Pause daily cycle until player selects
			}
		}

		// 8. Sect level check
		CheckSectLevelUp();

		Quests.CheckProgress(this);
		Quests.RefreshCompleted(this);
		Plot.CheckProgress(this);
		CheckAchievements();

		// 9. Advance day
		Time.AdvanceDay();
		// Log daily income summary every 5 days
		if (Time.Day % 5 == 0) LogEvent(DailySummary());

		if (GameSettings.AutoSave && Time.GetTotalDays() % GameSettings.AutoSaveInterval == 0)
			SaveGame(9);

		// 10. Random event (triggers during fast-forward too)
		PendingEvent = Events.TryTriggerEvent(SectLevel);
		if (PendingEvent != null)
		{
			EventBus.EmitEventChoiceRequired(PendingEvent);
		}
	}

	void CheckAchievements()
	{
		var unlocked = Achievements.CheckAll(this);
		foreach (var ach in unlocked)
			EventBus.EmitNotification("成就解锁", $"「{ach.Title}」\n{ach.Description}\n声望+{ach.RewardReputation}");
	}

	// ====== Outer Disciples ======

	private double _outerGrowthAccum, _outerPromoteAccum;
	private void ProcessOuterDisciples()
	{
		double eff = OuterEfficiency;
		int gather = OuterGatherCount, trade = OuterTradeCount, idle = OuterIdleCount;

		// Gathering: herbs + ore
		_herbAccum += gather * 0.30 * eff;
		_oreAccum += gather * 0.22 * eff;
		int herbGain = (int)_herbAccum; if (herbGain > 0) { Resources.Add(ResourceType.Herb, herbGain); _herbAccum -= herbGain; }
		int oreGain = (int)_oreAccum; if (oreGain > 0) { Resources.Add(ResourceType.Ore, oreGain); _oreAccum -= oreGain; }

		// Trading: spirit stones
		_tradeAccum += trade * 0.40 * eff;
		int tradeGain = (int)_tradeAccum; if (tradeGain > 0) { Resources.Add(ResourceType.SpiritStone, tradeGain); _tradeAccum -= tradeGain; }

		// Maintenance: gather+trade cost spirit stones (idle costs nothing)
		int activeCount = gather + trade;
		int cost = Math.Max(0, (int)Math.Ceiling(activeCount * 0.25 / eff));
		if (cost > 0) Resources.Spend(ResourceType.SpiritStone, cost);

		// Passive growth
		int guestBonus = Facilities.AllFacilities.Where(f => f.IsBuilt && f.Type == FacilityType.GuestHall).Sum(f => f.Level);
		_outerGrowthAccum += SectReputation * 0.002 + guestBonus * 0.05;
		if (_outerGrowthAccum >= 1) { int gain = (int)_outerGrowthAccum; OuterDiscipleCount += gain; _outerGrowthAccum -= gain; }

		// Over-capacity attrition
		if (OuterDiscipleCount > MaxOuterDisciples && MaxOuterDisciples > 0)
		{
			int overflow = OuterDiscipleCount - MaxOuterDisciples;
			if (_rng.NextDouble() < overflow * 0.03) OuterDiscipleCount = Math.Max(MaxOuterDisciples, OuterDiscipleCount - 1);
		}

		// Promotion to inner: idle outer disciples sometimes show talent
		if (idle > 0)
		{
			_outerPromoteAccum += idle * 0.01;
			if (_outerPromoteAccum >= 1 && Disciples.Count < MaxDisciples)
			{
				_outerPromoteAccum = 0;
				var candidate = Disciples.GenerateCandidate();
				// Boost stats slightly (they've been training in their spare time)
				candidate.Talent = Math.Min(100, candidate.Talent + _rng.Next(3, 8));
				candidate.Comprehension = Math.Min(100, candidate.Comprehension + _rng.Next(3, 8));
				PendingRecruitCandidates = new List<DiscipleData> { candidate };
				TournamentPicksRemaining = 1;
				LogEvent($"一名外门弟子勤修苦练，展现出惊人天赋！");
			Achievements.TriggerOuterPromotion();
				EventBus.EmitNotification("外门英才", $"外门弟子「{candidate.Name}」展现出过人天赋，可破格收入内门！");
				EventBus.EmitRecruitSelectionReady(PendingRecruitCandidates);
			}
		}
	}

	/// <summary>Spend spirit stones to actively recruit outer disciples.</summary>
	public void RecruitOuterDisciples()
	{
		if (!IsInitialized) return;
		if (OuterDiscipleCount >= MaxOuterDisciples)
		{
			EventBus.EmitNotification("启禀", $"外门弟子已满（上限{MaxOuterDisciples}人），需提升管理能力。");
			return;
		}
		int cost = 50 + OuterDiscipleCount / 5;
		if (!Resources.Spend(ResourceType.SpiritStone, cost)) return;

		int gain = 3 + _rng.Next(3, 10) + SectReputation / 100;
		OuterDiscipleCount = Math.Min(MaxOuterDisciples, OuterDiscipleCount + gain);
		LogEvent($"花费{cost}灵石招募了{gain}名外门弟子");
		AudioManager.PlayClick();
		AdvanceTime(1);
	}

	/// <summary>Adjust outer disciple role ratios.</summary>
	public void SetOuterRoles(int gather, int trade)
	{
		OuterGatherRatio = Math.Clamp(gather, 0, 100);
		OuterTradeRatio = Math.Clamp(trade, 0, 100);
		if (OuterGatherRatio + OuterTradeRatio > 100)
			OuterTradeRatio = 100 - OuterGatherRatio;
	}

	// ====== Sect Level ======

	private void CheckSectLevelUp()
	{
		if (SectLevel >= LevelReq.Length) return;
		if (SectReputation >= LevelReq[SectLevel])
		{
			int prevLevel = SectLevel;
			SectLevel++;
		LogEvent($"宗门晋升至Lv.{SectLevel}，弟子名额增至{MaxDisciples}人");
			MaxDisciples = SectLevel <= MaxDiscPerLevel.Length
				? MaxDiscPerLevel[SectLevel - 1]
				: MaxDiscPerLevel[^1] + (SectLevel - MaxDiscPerLevel.Length) * 10;
			EventBus.EmitNotification("宗门晋升", $"宗门晋升至 Lv.{SectLevel}！最大弟子数增至{MaxDisciples}人。");

			// Check newly unlocked facilities
			var newFacilities = FacilityTable.GetAll().Where(kv => kv.Value.MinSectLevel == SectLevel).ToList();
			foreach (var (_, info) in newFacilities)
				EventBus.EmitNotification("灵筑解锁", $"新灵筑「{info.Name}」现已可建造！\n{info.Description}");

			// Check newly unlocked features
			if (prevLevel < 2 && SectLevel >= 2)
				EventBus.EmitNotification("功能解锁", "• 外门系统：可招募外门弟子辅助经营\n• 道侣系统：弟子之间可结为道缘\n• 丹房 / 藏经阁 / 会客厅 可建造");
			if (prevLevel < 3 && SectLevel >= 3)
				EventBus.EmitNotification("功能解锁", "• 秘境探索：可派遣弟子探索秘境寻宝\n• 阵法殿可建造：提升宗门防御\n• 高阶事件将陆续触发");
			if (prevLevel < 4 && SectLevel >= 4)
				EventBus.EmitNotification("功能解锁", "• 上古传承事件已解锁\n• 宗门之战等竞争事件将出现\n• 更多天材地宝机缘等待发掘");
			if (prevLevel < 5 && SectLevel >= 5)
				EventBus.EmitNotification("功能解锁", "• 天外邪魔事件：宗门面临重大挑战\n• 顶级机缘事件出现\n• 宗门威名远扬，万宗来朝");
			if (prevLevel < 6 && SectLevel >= 6)
				EventBus.EmitNotification("功能解锁", "• 天道碎片争夺\n• 仙器出世事件\n• 飞升雷劫观摩机缘");
		}
	}

	// ====== 内门选拔 ======

	/// <summary>Max candidates in tournament (base 5, + GuestHall bonus).</summary>
	public int TournamentCandidateCount
	{
		get
		{
			int guest = Facilities.AllFacilities.Where(f => f.IsBuilt && f.Type == FacilityType.GuestHall).Sum(f => f.Level);
			int training = Facilities.AllFacilities.Where(f => f.IsBuilt && f.Type == FacilityType.TrainingGround).Sum(f => f.Level);
			return 5 + guest * 2 + training;
		}
	}

	/// <summary>Max picks from tournament (base 1, + sect level / facility bonus).</summary>
	public int TournamentPickCount
	{
		get
		{
			int guest = Facilities.AllFacilities.Where(f => f.IsBuilt && f.Type == FacilityType.GuestHall).Sum(f => f.Level);
			return 1 + SectLevel / 2 + guest / 2;
		}
	}

	/// <summary>Start a 7-day entrance tournament. Candidates appear after countdown.</summary>
	public void ScheduleRecruitTournament()
	{
		if (!IsInitialized) return;
		if (Disciples.Count >= MaxDisciples)
		{
			EventBus.EmitNotification("启禀", $"内门弟子已满（上限{MaxDisciples}人）");
			return;
		}
		if (RecruitTournamentDays > 0)
		{
			EventBus.EmitNotification("启禀", $"内门选拔正在进行中（剩余{RecruitTournamentDays}天）");
			return;
		}
		RecruitTournamentDays = 7;
		int cand = TournamentCandidateCount;
		int pick = TournamentPickCount;
		EventBus.EmitNotification("宗门谕令", $"已发榜招募内门弟子，七日后选拔！\n预计{cand}人应试，可选{pick}人入门。");
		AdvanceTime(1);
	}

	/// <summary>Generate candidates with quality scaled by sect reputation.</summary>
	private List<DiscipleData> GenerateRecruitCandidates(int reputation)
	{
		var candidates = new List<DiscipleData>();
		int count = TournamentCandidateCount;

		// Fortune's Child: ~1% chance per tournament (scales with reputation)
		bool hasFortuneChild = _rng.NextDouble() < 0.01 + SectReputation * 0.0001;

		for (int i = 0; i < count; i++)
		{
			DiscipleData d;
			if (hasFortuneChild && i == 0)
			{
				d = GenerateFortuneChild();
				hasFortuneChild = false; // Only one per tournament
			}
			else
			{
				d = Disciples.GenerateCandidate();

				// Reputation-based quality boost
				if (reputation >= 100)
				{
					int boost = reputation >= 800 ? 15 : reputation >= 400 ? 10 : reputation >= 150 ? 5 : 3;
					switch (_rng.Next(4))
					{
						case 0: d.Talent = Math.Min(100, d.Talent + boost); break;
						case 1: d.Comprehension = Math.Min(100, d.Comprehension + boost); break;
						case 2: d.Constitution = Math.Min(100, d.Constitution + boost); break;
						case 3: d.Spirit = Math.Min(100, d.Spirit + boost); break;
					}
				}

				if (reputation >= 300 && d.SpiritRoot >= SpiritualRoot.ThreeElement)
				{
					if (_rng.NextDouble() < (reputation >= 1000 ? 0.30 : reputation >= 600 ? 0.15 : 0.05))
						d.SpiritRoot = RollBetterRoot(d.SpiritRoot);
				}
			}

			candidates.Add(d);
		}
		return candidates;
	}

	/// <summary>Generate a Fortune's Child — exceptionally talented disciple.</summary>
	private DiscipleData GenerateFortuneChild()
	{
		bool isMale = _rng.Next(2) == 0;
		var d = new DiscipleData
		{
			Name = DiscipleNameTable.GenerateName(isMale),
			Age = _rng.Next(12, 19),
			Talent = _rng.Next(80, 101),
			Comprehension = _rng.Next(80, 101),
			Constitution = _rng.Next(75, 101),
			Spirit = _rng.Next(80, 101),
			Realm = CultivationRealm.Mortal,
			IsMale = isMale,
			SpiritRoot = _rng.NextDouble() < 0.4 ? SpiritualRoot.Heavenly : SpiritualRoot.SingleElement,
			Background = "天降异象",
			Personality = "天命所归",
			Trait = "气运之子",
		};
		LogEvent($"天降异象！一位气运之子{d.Name}出现在内门选拔中！");
		Achievements.TriggerFortuneChild();
		return d;
	}

	private SpiritualRoot RollBetterRoot(SpiritualRoot current)
	{
		// Upgrade to next tier with some randomness
		return current switch
		{
			SpiritualRoot.ThreeElement => _rng.Next(2) == 0 ? SpiritualRoot.DualElement : SpiritualRoot.Special,
			SpiritualRoot.DualElement => _rng.Next(3) == 0 ? SpiritualRoot.SingleElement : SpiritualRoot.DualElement,
			SpiritualRoot.SingleElement => _rng.Next(4) == 0 ? SpiritualRoot.Heavenly : SpiritualRoot.SingleElement,
			_ => current,
		};
	}

	public void ConfirmRecruit(DiscipleData candidate)
	{
		if (!IsInitialized || candidate == null || TournamentPicksRemaining <= 0) return;
		Disciples.Admit(candidate);
		LogEvent($"{candidate.Name}通过内门选拔，正式加入宗门");
		PendingRecruitCandidates!.Remove(candidate);
		TournamentPicksRemaining--;
		if (TournamentPicksRemaining <= 0 || Disciples.Count >= MaxDisciples)
		{
			PendingRecruitCandidates = null;
			EventBus.EmitNotification("宗门谕令", $"{candidate.Name}通过内门选拔，正式加入宗门！选拔结束。");
			AdvanceTime(1);
		}
		else
		{
			EventBus.EmitNotification("宗门谕令", $"{candidate.Name}通过内门选拔！尚余{TournamentPicksRemaining}个名额。");
			EventBus.EmitRecruitSelectionReady(PendingRecruitCandidates);
		}
	}

	public void CancelRecruit()
	{
		PendingRecruitCandidates = null;
		TournamentPicksRemaining = 0;
	}

	/// <summary>Daily auto-assign: check each disciple and reassign unhealthy ones to rest.</summary>
	private void RunAutoAssign()
	{
		foreach (var d in Disciples.AllDisciples)
		{
			if (d.IsInBreakthrough) continue;
			// Low mood → rest
			if (d.Mood < 20 && d.CurrentTask != DiscipleTaskType.Rest)
				{ Disciples.AssignTask(d.Id, DiscipleTaskType.Rest); continue; }
			// Low stamina → rest
			if (d.CurrentStamina < d.MaxStamina * 0.2 && d.CurrentTask != DiscipleTaskType.Rest)
				{ Disciples.AssignTask(d.Id, DiscipleTaskType.Rest); continue; }
			// Low health → rest
			if (d.Health < d.MaxHealth * 0.3 && d.CurrentTask != DiscipleTaskType.Rest)
				{ Disciples.AssignTask(d.Id, DiscipleTaskType.Rest); continue; }
			// Recovered → return to best task
			if (d.CurrentTask == DiscipleTaskType.Rest
				&& d.Mood >= 50 && d.CurrentStamina >= d.MaxStamina * 0.7 && d.Health >= d.MaxHealth * 0.7)
			{
				var best = d.TaskProficiency.Count > 0
					? d.TaskProficiency.OrderByDescending(kv => kv.Value).First().Key
					: DiscipleTaskType.Cultivate;
				Disciples.AssignTask(d.Id, best);
			}
		}
	}


	// ====== Player Actions ======

	public void StartBuild(FacilityType type)
	{
		if (!IsInitialized) return;
		var info = FacilityTable.GetInfo(type);
		int maxFacilities = SectLevel * 2;
		if (Facilities.Count >= maxFacilities)
		{
			AudioManager.PlayClose();
			EventBus.EmitNotification("启禀", $"设施已达上限（Lv.{SectLevel}最多{maxFacilities}座）");
			return;
		}
		if (!Resources.Spend(ResourceType.SpiritStone, info.BaseBuildCost)) return;
		Facilities.Build(type);
			LogEvent($"开始营造{info.Name}（{info.BaseBuildCost}灵石）");
		AudioManager.PlayBuild();
		EventBus.EmitNotification("宗门谕令", $"开始建造{info.Name}");
		AdvanceTime(1);
	}

	public void UpgradeFacility(int facilityId)
	{
		if (!IsInitialized) return;
		var f = Facilities.Get(facilityId);
		if (f == null || !f.IsBuilt || f.IsUnderConstruction || f.Level >= f.MaxLevel) return;

		int cost = FacilityTable.GetUpgradeCost(f.Type, f.Level);
		if (!Resources.Spend(ResourceType.SpiritStone, cost)) return;

		Facilities.StartUpgrade(facilityId, Resources);
			LogEvent($"开始升级{f.TypeName}至Lv.{f.Level + 1}");
		AudioManager.PlayUpgrade();
		EventBus.EmitNotification("宗门谕令", $"开始升级{f.TypeName}至Lv.{f.Level + 1}，消耗{cost}灵石");
		AdvanceTime(1);
	}

	public void DismissDisciple(int id)
	{
		if (!IsInitialized) return;
		var d = Disciples.Get(id);
		if (d == null) return;
		if (d.CompanionId >= 0)
		{
			var comp = Companions.Get(d.CompanionId);
			if (comp != null)
			{
				int otherId = comp.DiscipleId1 == id ? comp.DiscipleId2 : comp.DiscipleId1;
				var other = Disciples.Get(otherId);
				if (other != null) { other.CompanionId = -1; other.IsMarried = false; other.Mood = Math.Max(0, other.Mood - 20); }
			}
			Companions.RemoveCompanion(d.CompanionId);
		}
		Disciples.Dismiss(id);
		SectReputation = Math.Max(0, SectReputation - 5);
		EventBus.EmitNotification("宗门谕令", $"弟子{d.Name}离开了宗门（声望-5）");
		AdvanceTime(1);
	}

	public void AssignTask(int discipleId, DiscipleTaskType task, int targetId = -1)
	{
		if (!IsInitialized) return;
		Disciples.AssignTask(discipleId, task, targetId);
	}

	public void NextDay() => AdvanceTime(1);

	// ====== Combat ======

	public List<CombatMission> GetCombatMissions()
	{
		if (!IsInitialized) return new();
		return Combat.GetAvailableMissions(SectLevel);
	}

	public CombatResult? ExecuteCombatMission(List<int> discipleIds, int missionId)
	{
		if (!IsInitialized) return null;
		var mission = Combat.GetAvailableMissions(SectLevel).FirstOrDefault(m => m.MissionId == missionId);
		if (mission == null) return null;
		var party = discipleIds.Select(id => Disciples.Get(id)).Where(d => d != null).ToList();
		if (party.Count == 0 || party.Count > mission.MaxDisciples) return null;
		var result = Combat.Resolve(party!, mission);
		// Apply rewards
		foreach (var kv in result.Rewards)
		{
			if (kv.Value > 0) Resources.Add(kv.Key, kv.Value);
			else if (kv.Value < 0) Resources.Spend(kv.Key, -kv.Value);
		}
		SectReputation = Math.Max(0, SectReputation + result.ReputationGain);
		if (result.EquipmentGained > 0)
		{
			var eq = EquipmentTable.CraftRandom(SectLevel);
			AllEquipment.Add(eq);
			LogEvent($"战斗缴获法器「{eq.Name}」");
		}
		string outcome = result.Victory ? "击败" : "败于";
		LogEvent($"出征{mission.Name}: {outcome}{mission.EnemyName}");
		AdvanceTime(1);
		return result;
	}

	// ====== Event ======

	public void ResolveEvent(int choiceIndex)
	{
		if (PendingEvent == null) return;
		int rep = SectReputation;
		int pow = SectPower;
		Events.ApplyOutcome(PendingEvent, choiceIndex, Resources, Disciples, ref rep, ref pow);
		SectReputation = Math.Max(0, rep);
		SectPower = Math.Max(0, pow);
		PendingEvent = null;
	}

	public void DismissEvent() => PendingEvent = null;

	private bool CheckGameOver()
	{
		if (Disciples.Count == 0)
		{
			EventBus.EmitNotification("宗门倾覆", "所有弟子都已离去，宗门名存实亡...");
			return true;
		}
		return false;
	}

	// ====== Offspring ======

	void HandleNewborns()
	{
		foreach (var child in _pendingNewborns)
		{
			Children.Add(child);
			LogEvent($"{child.Name}出生");
		}
		_pendingNewborns.Clear();
	}

	void GrowChildren()
	{
		var matured = new List<DiscipleData>();
		foreach (var child in Children)
		{
			child.Age++;
			if (child.Age >= 14) // 14 matches minimum Recruit() age (14-25), child becomes full disciple
				matured.Add(child);
		}
		foreach (var child in matured)
		{
			Children.Remove(child);
			Disciples.AddChildDisciple(child);
			LogEvent($"{child.Name}已满14岁，正式成为内门弟子");
			EventBus.EmitNotification("宗门之喜", $"{child.Name}已满14岁，正式拜入内门！");
		}
	}

	// ====== Companion ======

	public void IntroduceCompanions(int d1Id, int d2Id)
	{
		if (!IsInitialized) return;
		Companions.Introduce(d1Id, d2Id, Disciples.AllDisciples.ToList());
		AudioManager.PlayCompanion();
		AdvanceTime(1);
	}

	public void ProposeMarriage(int companionId)
	{
		if (!IsInitialized) return;
		Companions.ProposeMarriage(companionId, Disciples.AllDisciples.ToList());
		AudioManager.PlayCompanion();
		AdvanceTime(1);
	}

	public void GiveGiftToCompanion(int companionId, ResourceType giftType, int amount)
	{
		if (!IsInitialized) return;
		if (!Resources.Spend(giftType, amount)) return;
		int boost = giftType switch
		{
			ResourceType.Pill => amount * 5,
			ResourceType.Equipment => amount * 3,
			ResourceType.SpiritStone => amount / 10,
			_ => amount
		};
		Companions.GiveGift(companionId, boost);
		AudioManager.PlayClick();
		EventBus.EmitNotification("赠礼传情", $"赠送了礼物，道缘之间的情谊更加深厚了。");
		AdvanceTime(1);
	}

	public void BreakUpCompanion(int companionId)
	{
		if (!IsInitialized) return;
		Companions.BreakUp(companionId, Disciples.AllDisciples.ToList());
		SectReputation = Math.Max(0, SectReputation - 3);
		AudioManager.PlayClose();
		AdvanceTime(1);
	}

	// ====== Equipment ======

	public bool EquipItem(int equipmentId, int discipleId)
	{
		var eq = AllEquipment.FirstOrDefault(e => e.Id == equipmentId);
		var d = Disciples.Get(discipleId);
		if (eq == null || d == null || eq.EquippedById >= 0) return false;
		if (d.EquipmentIds.Count >= 2) return false;
		eq.EquippedById = discipleId;
		d.EquipmentIds.Add(equipmentId);
		RefreshEquipmentBonuses();
		return true;
	}

	public bool UnequipItem(int equipmentId)
	{
		var eq = AllEquipment.FirstOrDefault(e => e.Id == equipmentId);
		if (eq == null || eq.EquippedById < 0) return false;
		var d = Disciples.Get(eq.EquippedById);
		eq.EquippedById = -1;
		d?.EquipmentIds.Remove(equipmentId);
		RefreshEquipmentBonuses();
		return true;
	}

	public bool UpgradeEquipment(int equipmentId)
	{
		var eq = AllEquipment.FirstOrDefault(e => e.Id == equipmentId);
		if (eq == null || eq.Quality >= EquipmentQuality.Epic) return false;

		int oreCost = eq.Quality switch { EquipmentQuality.Common => 30, EquipmentQuality.Uncommon => 80, _ => 200 };
		int stoneCost = eq.Quality switch { EquipmentQuality.Common => 100, EquipmentQuality.Uncommon => 300, _ => 800 };

		if (!Resources.Spend(ResourceType.Ore, oreCost) || !Resources.Spend(ResourceType.SpiritStone, stoneCost))
			return false;

		eq.UpgradeQuality();
		RefreshEquipmentBonuses();
		AudioManager.PlayUpgrade();
		return true;
	}

	public void RefreshEquipmentBonuses()
	{
		foreach (var d in Disciples.AllDisciples)
		{
			var cache = new EquipmentBonusCache();
			foreach (int eqId in d.EquipmentIds)
			{
				var eq = AllEquipment.FirstOrDefault(e => e.Id == eqId);
				if (eq == null) continue;
				cache.TalentBonus += eq.TalentBonus;
				cache.ComprehensionBonus += eq.ComprehensionBonus;
				cache.ConstitutionBonus += eq.ConstitutionBonus;
				cache.SpiritBonus += eq.SpiritBonus;
				cache.CombatBonus += eq.CombatBonus;
				cache.CultivationSpeedBonus += eq.CultivationSpeedBonus;
			}
			d.EquipmentBonusCache = cache;
		}
	}

	void OnDiscipleDeparted(DiscipleData d)
	{
		LogEvent($"{d.Name}离开了宗门（声望-5）");
		if (d.CompanionId >= 0)
		{
			var comp = Companions.Get(d.CompanionId);
			if (comp != null)
			{
				int otherId = comp.DiscipleId1 == d.Id ? comp.DiscipleId2 : comp.DiscipleId1;
				var other = Disciples.Get(otherId);
				if (other != null) { other.CompanionId = -1; other.IsMarried = false; }
			}
			Companions.RemoveCompanion(d.CompanionId);
		}
	}


	string DailySummary()
	{
		int spirit = Resources.Get(ResourceType.SpiritStone);
		int herbs = Resources.Get(ResourceType.Herb);
		int ore = Resources.Get(ResourceType.Ore);
		return $"是日：灵石{spirit}，灵草{herbs}，矿石{ore}，弟子{Disciples.Count}人，声望{SectReputation}";
	}

	void ShowYearSummary(int year)
	{
		int prevYear = year - 1;
		int married = Companions.AllCompanions.Count(c => c.IsMarried);
		int unequipped = AllEquipment.Count(e => e.EquippedById < 0);
		string summary = $"—— 第{prevYear}年宗门总结 ——\n"
			+ $"宗门 {SectName} Lv.{SectLevel}  声望{SectReputation}  战力{SectPower}\n"
			+ $"弟子{Disciples.Count}人  设施{Facilities.Count}座  道侣{married}对\n"
			+ $"灵石{Resources.Get(ResourceType.SpiritStone)}  灵草{Resources.Get(ResourceType.Herb)}  矿石{Resources.Get(ResourceType.Ore)}\n"
			+ $"丹药{Resources.Get(ResourceType.Pill)}  法器库存{unequipped}件";
		EventBus.EmitNotification("年度总结", summary);
	}

	// ====== Save / Load ======

	public void SaveGame(int slot)
	{
		var data = SaveLoad.CreateSaveData(this);
		SaveLoad.SaveToSlot(slot, data);
	}

	public bool LoadGame(int slot)
	{
		var data = SaveLoad.LoadFromSlot(slot);
		if (data == null) return false;
		SaveLoad.ApplySaveData(data, this);
		PendingEvent = null;
		PendingRecruitCandidates = null;
		EventBus.Clear();
		EventBus.ChildBorn += (child, _, _) => _pendingNewborns.Add(child);
		EventBus.DiscipleRecruited += d => LogEvent($"{d.Name}加入宗门");
		EventBus.BreakthroughSuccess += d => LogEvent($"{d.Name}成功突破至{d.FullRealmName}！");
		EventBus.BreakthroughFailed += d => LogEvent($"{d.Name}突破失败，修为受损");
		EventBus.YearPassed += ShowYearSummary;
		EventBus.DiscipleDeparted += OnDiscipleDeparted;
		RefreshEquipmentBonuses();
		return true;
	}

	// ====== Save/Load helpers for private fields ======
	internal double GetHerbAccum() => _herbAccum;
	internal double GetOreAccum() => _oreAccum;
	internal double GetTradeAccum() => _tradeAccum;
	internal double GetOuterGrowthAccum() => _outerGrowthAccum;
	internal double GetOuterPromoteAccum() => _outerPromoteAccum;
	internal void SetHerbAccum(double v) => _herbAccum = v;
	internal void SetOreAccum(double v) => _oreAccum = v;
	internal void SetTradeAccum(double v) => _tradeAccum = v;
	internal void SetOuterGrowthAccum(double v) => _outerGrowthAccum = v;
	internal void SetOuterPromoteAccum(double v) => _outerPromoteAccum = v;

	internal void ApplyRngSeed(int seed)
	{
		RngSeed = seed;
		_rng = new Random(seed);
	}
}
