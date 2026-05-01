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
		EventLogEntries.Clear();
		_pendingNewborns.Clear();
		// Starting equipment
		AllEquipment.Add(new EquipmentData { Id = 1, Name = "铁剑", Quality = EquipmentQuality.Common, Description = "宗门传承的铁剑", CombatBonus = 5 });
		AllEquipment.Add(new EquipmentData { Id = 2, Name = "静心蒲团", Quality = EquipmentQuality.Common, Description = "辅助修炼的蒲团", CultivationSpeedBonus = 0.05 });
		for (int i = 0; i < 3; i++) Disciples.Recruit();
		IsInitialized = true;
	}

	// ====== Daily Pipeline ======

	public void AdvanceTime(int days)
	{
		if (!IsInitialized || PendingEvent != null) return;
		for (int i = 0; i < days; i++)
		{
			RunDailyCycle();
			if (PendingEvent != null) return;
			if (CheckGameOver()) return;
		}
	}

	public void FastForward(int days)
	{
		if (!IsInitialized || PendingEvent != null) return;
		for (int i = 0; i < days; i++)
		{
			RunDailyCycle(isFastForward: true);
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
		// 4. Disciples: task effects + stamina (returns sect power from Guard)
		int powerGain = Disciples.ProcessDaily(Resources, Facilities.AllFacilities, SectLevel, compBonus);
		SectPower += powerGain;
		AllEquipment.AddRange(Disciples.NewEquipmentToday);

		// Handle newborns from companions
		HandleNewborns();

		// 5. Event cooldowns
		Events.ProcessCooldowns();

		// 5. Auto-recruit based on reputation (5% base chance per day)
		if (_rng.NextDouble() < 0.08 * (1 + SectReputation / 200.0) && Disciples.Count < MaxDisciples)
		{
			var d = Disciples.Recruit();
			EventBus.EmitNotification("宗门动态", $"散人{d.Name}仰慕宗门声望，前来投奔！");
		}

		// 6. Sect level check
		CheckSectLevelUp();

		// Quest progress check
		Quests.CheckProgress(this);
		Quests.RefreshCompleted(this);

		// 7. Advance day first, then try random event
		Time.AdvanceDay();

		// Auto-save
		if (GameSettings.AutoSave && Time.GetTotalDays() % GameSettings.AutoSaveInterval == 0)
			SaveGame(9);

		// 8. Random event (skip during fast-forward)
		if (!isFastForward)
		{
			PendingEvent = Events.TryTriggerEvent(SectLevel);
			if (PendingEvent != null)
			{
				EventBus.EmitEventChoiceRequired(PendingEvent);
				// Don't advance day again — DayPassed already fired above
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
			MaxDisciples = SectLevel <= MaxDiscPerLevel.Length
				? MaxDiscPerLevel[SectLevel - 1]
				: MaxDiscPerLevel[^1] + (SectLevel - MaxDiscPerLevel.Length) * 10;
			EventBus.EmitNotification("宗门晋升", $"宗门晋升至 Lv.{SectLevel}！最大弟子数增至{MaxDisciples}人。");
		}
	}

	// ====== Player Actions (each costs 1 day) ======

	public void RecruitDisciple()
	{
		if (!IsInitialized) return;
		if (Disciples.Count >= MaxDisciples)
		{
			EventBus.EmitNotification("提示", $"弟子已满（上限{MaxDisciples}人）");
			return;
		}
		Disciples.Recruit();
		EventBus.EmitNotification("宗门动态", "新弟子加入宗门！");
		AdvanceTime(1);
	}

	public void StartBuild(FacilityType type)
	{
		if (!IsInitialized) return;
		var info = FacilityTable.GetInfo(type);
		// Limit facilities by sect level
		int maxFacilities = SectLevel * 2;
		if (Facilities.Count >= maxFacilities)
		{
			AudioManager.PlayClose();
			EventBus.EmitNotification("提示", $"设施已达上限（Lv.{SectLevel}最多{maxFacilities}座）");
			return;
		}
		if (!Resources.Spend(ResourceType.SpiritStone, info.BaseBuildCost)) return;
		Facilities.Build(type);
		AudioManager.PlayBuild();
		EventBus.EmitNotification("宗门动态", $"开始建造{info.Name}");
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
		AudioManager.PlayUpgrade();
		EventBus.EmitNotification("宗门动态", $"开始升级{f.TypeName}至Lv.{f.Level + 1}，消耗{cost}灵石");
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
		EventBus.EmitNotification("宗门动态", $"弟子{d.Name}离开了宗门（声望-5）");
		AdvanceTime(1);
	}

	public void AssignTask(int discipleId, DiscipleTaskType task, int targetId = -1)
	{
		if (!IsInitialized) return;
		Disciples.AssignTask(discipleId, task, targetId);
		// Task assignment is instant, does not consume time
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
			EventBus.EmitNotification("宗门覆灭", "所有弟子都已离去，宗门名存实亡...");
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
		EventBus.EmitNotification("赠礼传情", $"赠送了礼物，道侣间的情谊更加深厚了。");
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
		// Unequip existing item of same type? For simplicity, allow 2 items max
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
		EventBus.ChildBorn += (child, _, _) => _pendingNewborns.Add(child);
		EventBus.YearPassed += ShowYearSummary;
		EventBus.DiscipleDeparted += OnDiscipleDeparted;
		RefreshEquipmentBonuses();
		return true;
	}
}
