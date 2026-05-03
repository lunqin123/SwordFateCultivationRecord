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
	public List<EquipmentData> AllEquipment { get; private set; } = new();

	// Sect state
	public string SectName { get; set; } = "无名剑宗";
	public int SectLevel { get; set; } = 1;
	public int SectReputation { get; set; } = 0;
	public int SectPower { get; set; } = 0;
	public int MaxDisciples { get; set; } = 5;

	public bool IsInitialized { get; private set; }
	public EventData? PendingEvent { get; private set; }

	// 入门大比
	public int RecruitTournamentDays { get; private set; } = -1; // -1 = 未安排
	public List<DiscipleData>? PendingRecruitCandidates { get; private set; }

	// 自动智能安排 — 需藏经阁 + Lv.2以上解锁
	public bool AutoAssignEnabled { get; set; }
	public bool CanAutoAssign => SectLevel >= 2 && Facilities.AllFacilities.Any(f => f.IsBuilt && f.Type == FacilityType.Library);

	private readonly Random _rng = new();
	private readonly List<DiscipleData> _pendingNewborns = new();
	public List<LogEntry> EventLogEntries { get; set; } = new();

	// ====== Level thresholds ======
	private static readonly int[] LevelReq = { 0, 100, 300, 600, 1000, 1500, 2500 };
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
		Quests.Initialize();
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
		// Starting equipment
		AllEquipment.Add(new EquipmentData { Id = 1, Name = "铁剑", Quality = EquipmentQuality.Common, Description = "宗门传承的铁剑", CombatBonus = 5 });
		AllEquipment.Add(new EquipmentData { Id = 2, Name = "静心蒲团", Quality = EquipmentQuality.Common, Description = "辅助修炼的蒲团", CultivationSpeedBonus = 0.05 });
		for (int i = 0; i < 3; i++) Disciples.Recruit();
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
			RunDailyCycle(isFastForward: true);
			if (PendingEvent != null || PendingRecruitCandidates != null) return;
			if (CheckGameOver()) return;
		}
	}

	private void RunDailyCycle(bool isFastForward = false)
	{
		// 1. Facility construction progress & income calc
		Facilities.ProcessDaily(Resources);
		// 2. Resource income applied
		Resources.ApplyDailyIncome();
		// 3. Companions: affection, mood, and dual cultivation bonuses
		var compBonus = Companions.ProcessDaily(Disciples.AllDisciples.ToList(), Time.GetTotalDays());
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

		// 6. 入门大比 countdown
		if (RecruitTournamentDays > 0)
		{
			RecruitTournamentDays--;
			if (RecruitTournamentDays == 0)
			{
				// Tournament ends — generate quality candidates based on reputation
				PendingRecruitCandidates = GenerateRecruitCandidates(SectReputation);
				EventBus.EmitRecruitSelectionReady(PendingRecruitCandidates);
				return; // Pause daily cycle until player selects
			}
		}

		// 7. Auto-recruit
		if (_rng.NextDouble() < 0.08 * (1 + SectReputation / 200.0) && Disciples.Count < MaxDisciples)
		{
			var d = Disciples.Recruit();
			EventBus.EmitNotification("宗门谕令", $"散人{d.Name}仰慕宗门声望，前来投奔！");
		}

		// 8. Sect level check
		CheckSectLevelUp();

		Quests.CheckProgress(this);
		Quests.RefreshCompleted(this);

		// 9. Advance day
		Time.AdvanceDay();
		// Log daily income summary every 5 days
		if (Time.Day % 5 == 0) LogEvent(DailySummary());

		if (GameSettings.AutoSave && Time.GetTotalDays() % GameSettings.AutoSaveInterval == 0)
			SaveGame(9);

		// 10. Random event (skip during fast-forward)
		if (!isFastForward)
		{
			PendingEvent = Events.TryTriggerEvent(SectLevel);
			if (PendingEvent != null)
			{
				EventBus.EmitEventChoiceRequired(PendingEvent);
			}
		}
	}

	// ====== Sect Level ======

	private void CheckSectLevelUp()
	{
		if (SectLevel >= LevelReq.Length) return;
		if (SectReputation >= LevelReq[SectLevel])
		{
			SectLevel++;
		LogEvent($"宗门晋升至Lv.{SectLevel}，弟子名额增至{MaxDisciples}人");
			MaxDisciples = SectLevel <= MaxDiscPerLevel.Length
				? MaxDiscPerLevel[SectLevel - 1]
				: MaxDiscPerLevel[^1] + (SectLevel - MaxDiscPerLevel.Length) * 10;
			EventBus.EmitNotification("宗门晋升", $"宗门晋升至 Lv.{SectLevel}！最大弟子数增至{MaxDisciples}人。");
		}
	}

	// ====== 入门大比 ======

	/// <summary>Start a 7-day entrance tournament. Candidates appear after countdown.</summary>
	public void ScheduleRecruitTournament()
	{
		if (!IsInitialized) return;
		if (Disciples.Count >= MaxDisciples)
		{
			EventBus.EmitNotification("启禀", $"弟子已满（上限{MaxDisciples}人）");
			return;
		}
		if (RecruitTournamentDays > 0)
		{
			EventBus.EmitNotification("启禀", $"入门大比正在进行中（剩余{RecruitTournamentDays}天）");
			return;
		}
		RecruitTournamentDays = 7;
		EventBus.EmitNotification("宗门谕令", "已发出入门大比公告，七日后举行选拔！");
		AdvanceTime(1);
	}

	/// <summary>Generate candidates with quality scaled by sect reputation.</summary>
	private List<DiscipleData> GenerateRecruitCandidates(int reputation)
	{
		var candidates = new List<DiscipleData>();
		int count = reputation >= 1000 ? 7 : reputation >= 500 ? 6 : 5;

		for (int i = 0; i < count; i++)
		{
			var d = Disciples.GenerateCandidate();

			// Reputation-based quality boost
			if (reputation >= 100)
			{
				// Boost one random stat
				int boost = reputation >= 800 ? 15 : reputation >= 400 ? 10 : reputation >= 150 ? 5 : 3;
				switch (_rng.Next(4))
				{
					case 0: d.Talent = Math.Min(100, d.Talent + boost); break;
					case 1: d.Comprehension = Math.Min(100, d.Comprehension + boost); break;
					case 2: d.Constitution = Math.Min(100, d.Constitution + boost); break;
					case 3: d.Spirit = Math.Min(100, d.Spirit + boost); break;
				}
			}

			// Higher reputation → better spiritual root odds
			if (reputation >= 300 && d.SpiritRoot >= SpiritualRoot.ThreeElement)
			{
				// Reroll chance for better root
				if (_rng.NextDouble() < (reputation >= 1000 ? 0.30 : reputation >= 600 ? 0.15 : 0.05))
					d.SpiritRoot = RollBetterRoot(d.SpiritRoot);
			}

			candidates.Add(d);
		}
		return candidates;
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
		if (!IsInitialized || candidate == null) return;
		Disciples.Admit(candidate);
		LogEvent($"{candidate.Name}通过入门大比，正式加入宗门");
		PendingRecruitCandidates = null;
		EventBus.EmitNotification("宗门谕令", $"{candidate.Name}通过入门大比，正式加入宗门！");
		AdvanceTime(1);
	}

	public void CancelRecruit()
	{
		PendingRecruitCandidates = null;
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
			Disciples.AddChildDisciple(child);
		}
		_pendingNewborns.Clear();
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
		RecruitTournamentDays = -1;
		PendingRecruitCandidates = null;
		EventBus.ChildBorn += (child, _, _) => _pendingNewborns.Add(child);
		EventBus.DiscipleRecruited += d => LogEvent($"{d.Name}加入宗门");
		EventBus.BreakthroughSuccess += d => LogEvent($"{d.Name}成功突破至{d.FullRealmName}！");
		EventBus.BreakthroughFailed += d => LogEvent($"{d.Name}突破失败，修为受损");
		EventBus.YearPassed += ShowYearSummary;
		EventBus.DiscipleDeparted += OnDiscipleDeparted;
		RefreshEquipmentBonuses();
		return true;
	}
}
