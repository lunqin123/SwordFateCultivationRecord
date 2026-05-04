namespace SwordFateCultivationRecord;

public class SecretRealmSystem
{
    private readonly Random _rng = new();
    private static readonly string[] RealmNames = { "云霄洞天", "幽冥秘境", "上古战场", "太虚幻境", "星辰遗迹", "龙脉深渊", "剑冢秘境", "丹霞仙府" };

    public SecretRealmState CurrentRealm { get; private set; } = new();

    /// <summary>Start a new secret realm exploration.</summary>
    public SecretRealmState StartRealm(int sectLevel)
    {
        int rooms = _rng.Next(3, 6); // 3-5 rooms
        var realm = new SecretRealmState
        {
            IsActive = true,
            CurrentRoom = 0,
            TotalRooms = rooms,
            Rooms = new List<RealmRoom>(),
            TreasureScore = 0,
            DamageTaken = 0,
            RealmName = RealmNames[_rng.Next(RealmNames.Length)],
        };

        for (int i = 0; i < rooms; i++)
        {
            realm.Rooms.Add(GenerateRoom(i, rooms, sectLevel));
        }

        CurrentRealm = realm;
        return realm;
    }

    /// <summary>Process player's choice. Returns (success, resultText, rewards).</summary>
    public (bool success, string text, Dictionary<ResourceType, int> resources, int rep, int cult, int health) ProcessChoice(int choiceIndex, GameManager gm)
    {
        if (!CurrentRealm.IsActive) return (false, "秘境已经结束。", new(), 0, 0, 0);

        var room = CurrentRealm.Rooms[CurrentRealm.CurrentRoom];
        if (choiceIndex < 0 || choiceIndex >= room.Choices.Count)
            return (false, "无效的选择。", new(), 0, 0, 0);

        var choice = room.Choices[choiceIndex];

        // Success chance based on sect power vs room danger + choice risk
        double successChance = Math.Clamp(0.5 + (gm.SectPower / 100.0) / (room.DangerLevel + choice.RiskLevel), 0.1, 0.95);
        bool success = _rng.NextDouble() < successChance;

        string text;
        var resources = new Dictionary<ResourceType, int>();
        int rep = 0, cult = 0, health = 0;

        if (success)
        {
            text = choice.SuccessText;
            foreach (var kv in choice.SuccessResources) resources[kv.Key] = kv.Value;
            rep = choice.SuccessReputation;
            cult = choice.SuccessCultivation;
            CurrentRealm.TreasureScore += room.DangerLevel + choice.RiskLevel * 2;
        }
        else
        {
            text = choice.FailText;
            foreach (var kv in choice.FailResources) resources[kv.Key] = kv.Value;
            rep = choice.FailReputation;
            health = choice.FailHealth;
            CurrentRealm.DamageTaken += choice.RiskLevel;
        }

        // Apply results
        foreach (var kv in resources)
        {
            if (kv.Value > 0) gm.Resources.Add(kv.Key, kv.Value);
            else if (kv.Value < 0) gm.Resources.Spend(kv.Key, -kv.Value);
        }
        if (rep > 0) gm.SectReputation += rep;
        else if (rep < 0) gm.SectReputation = Math.Max(0, gm.SectReputation + rep);
        if (cult > 0)
        {
            foreach (var d in gm.Disciples.AllDisciples)
                d.CultivationProgress += cult;
        }
        if (health < 0)
        {
            foreach (var d in gm.Disciples.AllDisciples)
                d.Health = Math.Max(1, d.Health + health);
        }

        CurrentRealm.CurrentRoom++;
        if (CurrentRealm.CurrentRoom >= CurrentRealm.TotalRooms)
        {
            CurrentRealm.IsActive = false;
			GameManager.Instance.Achievements.RecordExploration();
            // Final bonus based on treasure score
            int bonus = CurrentRealm.TreasureScore * 10;
            gm.Resources.Add(ResourceType.SpiritStone, bonus);
            text += $"\n\n—— 秘境探索结束 ——\n总收获: 灵石+{bonus}";
        }

        return (success, text, resources, rep, cult, health);
    }

    private RealmRoom GenerateRoom(int depth, int total, int sectLevel)
    {
        int baseDanger = 1 + depth + sectLevel / 2;
        var room = new RealmRoom { DangerLevel = Math.Min(10, baseDanger) };

        // Pick a room template based on depth
        int template = _rng.Next(8);
        switch (template)
        {
            case 0: // Treasure chamber
                room.Description = "前方出现一座金光闪闪的密室，堆满了灵石和宝物。但门口有古老的禁制守护。";
                room.ImageHint = "💎";
                room.Choices.Add(new RealmChoice
                {
                    Text = "强行破解禁制",
                    SuccessText = "禁制应声而破！密室的宝物尽归宗门所有！", FailText = "禁制反噬，灵石碎裂大半……",
                    SuccessResources = { [ResourceType.SpiritStone] = 200 + depth * 50, [ResourceType.Equipment] = 2 + depth },
                    FailResources = { [ResourceType.SpiritStone] = 30 }, FailHealth = -5 + depth,
                    RiskLevel = 2,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "慢慢研究禁制原理",
                    SuccessText = "耐心破解了禁制，安全获取了宝物。", FailText = "禁制太过复杂，只能放弃。",
                    SuccessResources = { [ResourceType.SpiritStone] = 100 + depth * 30, [ResourceType.Pill] = 5 + depth },
                    RiskLevel = 1,
                });
                break;

            case 1: // Monster lair
                room.Description = "通道尽头传来低沉的咆哮——一只守护妖兽盘踞在此，挡住了去路。";
                room.ImageHint = "🐉";
                room.Choices.Add(new RealmChoice
                {
                    Text = "正面迎战妖兽",
                    SuccessText = "妖兽被击败！它的巢穴中藏着不少宝物。", FailText = "妖兽太过强大，弟子们受伤撤退……",
                    SuccessResources = { [ResourceType.Ore] = 40 + depth * 15, [ResourceType.SpiritStone] = 100 + depth * 30 },
                    SuccessReputation = 10 + depth * 5, FailHealth = -10 - depth * 3,
                    RiskLevel = 2,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "绕道而行",
                    SuccessText = "成功绕开了妖兽，发现了一条捷径。", FailText = "绕路时迷路了，浪费了不少时间。",
                    SuccessCultivation = 5 + depth * 3, RiskLevel = 1,
                });
                break;

            case 2: // Puzzle room
                room.Description = "一块巨大的石碑立在面前，上面刻满了古老的符文。若能解开其中奥秘，必有收获。";
                room.ImageHint = "📜";
                room.Choices.Add(new RealmChoice
                {
                    Text = "全力参悟石碑符文",
                    SuccessText = "参悟了石碑中的上古秘法！弟子修为大增。", FailText = "心神耗尽却一无所获……",
                    SuccessCultivation = 15 + depth * 10, FailHealth = -3,
                    RiskLevel = 1,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "临摹符文带回宗门研究",
                    SuccessText = "符文被完整记录，带回宗门后大有用途。", FailText = "符文力量消散，留下的只是普通石刻。",
                    SuccessResources = { [ResourceType.SpiritEssence] = 20 + depth * 10 },
                    SuccessReputation = 5 + depth * 3, RiskLevel = 1,
                });
                break;

            case 3: // Trap corridor
                room.Description = "一条狭窄的通道，两侧墙壁上布满了密密麻麻的小孔——似乎是某种机关陷阱。";
                room.ImageHint = "⚠";
                room.Choices.Add(new RealmChoice
                {
                    Text = "快速冲过去",
                    SuccessText = "以迅雷不及掩耳之势冲过了陷阱区！毫发无伤。", FailText = "被机关击中，损失了不少丹药……",
                    SuccessResources = { [ResourceType.SpiritStone] = 50 + depth * 20 },
                    FailResources = { [ResourceType.Pill] = -3 - depth }, FailHealth = -8,
                    RiskLevel = 2,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "拆卸机关再通过",
                    SuccessText = "成功拆卸了机关，还获得了机关设计图纸。", FailText = "拆卸时触发了机关，矿石被毁。",
                    SuccessResources = { [ResourceType.Ore] = 30 + depth * 10 },
                    FailResources = { [ResourceType.Ore] = -5 }, RiskLevel = 1,
                });
                break;

            case 4: // Spirit spring
                room.Description = "洞中有一汪清澈的灵泉，泉水散发着浓郁的灵气。饮用泉水对修炼大有裨益。";
                room.ImageHint = "🌊";
                room.Choices.Add(new RealmChoice
                {
                    Text = "畅饮灵泉",
                    SuccessText = "灵泉入体，全身经脉舒畅，修为大增！", FailText = "泉水冰凉，效果打了折扣。",
                    SuccessCultivation = 20 + depth * 10, SuccessResources = { [ResourceType.SpiritEssence] = 15 + depth * 10 },
                    RiskLevel = 1,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "采集泉水带回宗门",
                    SuccessText = "用玉瓶装了大量灵泉，可以分给其他弟子。", FailText = "玉瓶不够，只能带走少量。",
                    SuccessResources = { [ResourceType.Pill] = 5 + depth * 3, [ResourceType.SpiritEssence] = 10 + depth * 5 },
                    RiskLevel = 1,
                });
                break;

            case 5: // Ancient armory
                room.Description = "一座古修士的兵器库！虽然大部分法器已经腐朽，但仍有一些散发着微光。";
                room.ImageHint = "⚔";
                room.Choices.Add(new RealmChoice
                {
                    Text = "深入库房搜寻",
                    SuccessText = "在最深处找到了几件保存完好的上品法器！", FailText = "库房坍塌，法器被埋在碎石中……",
                    SuccessResources = { [ResourceType.Equipment] = 5 + depth * 2 },
                    FailResources = { [ResourceType.Equipment] = 1 }, FailHealth = -5,
                    RiskLevel = 2,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "收集还能用的法器残片",
                    SuccessText = "残片可以回炉重铸，也是不错的收获。", FailText = "残片质量太差，基本无法利用。",
                    SuccessResources = { [ResourceType.Equipment] = 2 + depth, [ResourceType.Ore] = 15 },
                    RiskLevel = 1,
                });
                break;

            case 6: // Cultivation chamber
                room.Description = "一间修炼密室，墙壁上刻满了功法图解。这是某位大能闭关修炼的地方，空气中还残留着道韵。";
                room.ImageHint = "🧘";
                room.Choices.Add(new RealmChoice
                {
                    Text = "在此原地修炼感悟",
                    SuccessText = "道韵入体，所有弟子都获得了珍贵的感悟！", FailText = "道韵太淡，收获有限。",
                    SuccessCultivation = 25 + depth * 15, SuccessReputation = 5,
                    RiskLevel = 1,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "将功法图解全部拓印",
                    SuccessText = "拓印的功法成为宗门新的修炼教材。", FailText = "图解太复杂，拓印不完整。",
                    SuccessCultivation = 10 + depth * 5, SuccessResources = { [ResourceType.SpiritEssence] = 10 },
                    RiskLevel = 1,
                });
                break;

            case 7: // Final treasure (only for deep rooms)
                room.Description = "你来到了秘境的最深处。一座巨大的石台上，摆放着一个古老的宝箱。但石台周围环绕着强大的守护阵法。";
                room.ImageHint = "👑";
                room.Choices.Add(new RealmChoice
                {
                    Text = "全力破解阵法夺取宝箱",
                    SuccessText = "宝箱开启！里面是满满的上古宝物和丹药！", FailText = "阵法反击，重伤了几名弟子，宝箱也损毁了……",
                    SuccessResources = { [ResourceType.SpiritStone] = 300 + depth * 80, [ResourceType.Pill] = 10 + depth * 5, [ResourceType.Equipment] = 8 + depth * 3 },
                    SuccessReputation = 15 + depth * 5, FailHealth = -15 - depth * 5,
                    RiskLevel = 3,
                });
                room.Choices.Add(new RealmChoice
                {
                    Text = "小心研究，逐步破解",
                    SuccessText = "阵法被逐一破解，宝箱安全到手。", FailText = "只破解了外层阵法，内层无法突破。",
                    SuccessResources = { [ResourceType.SpiritStone] = 150 + depth * 40, [ResourceType.Pill] = 5 + depth * 2 },
                    RiskLevel = 1,
                });
                break;
        }

        return room;
    }

    public void CancelRealm()
    {
        CurrentRealm.IsActive = false;
			GameManager.Instance.Achievements.RecordExploration();
    }
}
