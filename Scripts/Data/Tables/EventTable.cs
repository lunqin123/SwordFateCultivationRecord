namespace SwordFateCultivationRecord;

public static class EventTable
{
    private static readonly List<EventData> _events = new()
    {
        new()
        {
            Id = 1, Category = EventCategory.Opportunity, Weight = 15,
            Title = "天降灵物",
            Description = "一颗流星划过天际，坠落在宗门后山，散发出浓郁的灵气波动。",
            Choice1Text = "派人前去探查",
            Choice2Text = "封锁消息，独自研究",
            Choice3Text = "当作异象，不予理会",
            Choice1Outcome = new() { ResultText = "发现了一块天外陨铁，价值不菲！", ResourceChanges = { [ResourceType.Ore] = 30, [ResourceType.SpiritStone] = 50 } },
            Choice2Outcome = new() { ResultText = "暗中研究后获得突破感悟。", DiscipleCultivationBonus = 20 },
            Choice3Outcome = new() { ResultText = "什么也没发生，但保持了低调。", ReputationChange = 5 },
        },
        new()
        {
            Id = 2, Category = EventCategory.Crisis, Weight = 12,
            Title = "妖兽袭击",
            Description = "一群妖兽在宗门附近出没，威胁到弟子们的安全。",
            Choice1Text = "组织弟子迎战",
            Choice2Text = "开启护山大阵",
            Choice3Text = "暂避锋芒",
            Choice1Outcome = new() { ResultText = "击退了妖兽，弟子们得到了历练！", DiscipleCultivationBonus = 10, PowerChange = 15, DiscipleStatEffects = new[] { 5, 10, -10 } },
            Choice2Outcome = new() { ResultText = "护山大阵成功抵御了攻击，但消耗了大量灵石。", ResourceChanges = { [ResourceType.SpiritStone] = -100 }, ReputationChange = 10 },
            Choice3Outcome = new() { ResultText = "妖兽肆虐后离去，宗门声誉受损。", ReputationChange = -15, PowerChange = -10 },
        },
        new()
        {
            Id = 3, Category = EventCategory.Visitor, Weight = 10,
            Title = "云游散人",
            Description = "一位修为高深的云游散人路过宗门，想要落脚歇息。",
            Choice1Text = "热情款待",
            Choice2Text = "以礼相待但保持距离",
            Choice3Text = "婉言谢绝",
            Choice1Outcome = new() { ResultText = "散人传授了珍贵的修炼心得，部分弟子悟性提升！", DiscipleStatEffects = new[] { 10, 15, 0 }, ResourceChanges = { [ResourceType.SpiritStone] = -30 } },
            Choice2Outcome = new() { ResultText = "散人留下了一些丹药作为答谢。", ResourceChanges = { [ResourceType.Pill] = 5 } },
            Choice3Outcome = new() { ResultText = "散人摇头离去，似乎对宗门印象不佳。", ReputationChange = -5 },
        },
        new()
        {
            Id = 4, Category = EventCategory.Internal, Weight = 15,
            Title = "弟子切磋",
            Description = "两名弟子因修炼心得不同产生争执，要求公开切磋。",
            Choice1Text = "允许切磋，点到为止",
            Choice2Text = "制止争执，各打五十大板",
            Choice3Text = "让他们进行生死斗",
            Choice1Outcome = new() { ResultText = "切磋之后两人惺惺相惜，成为了修炼伙伴。", DiscipleStatEffects = new[] { 5, 10, 0 }, DiscipleCultivationBonus = 5 },
            Choice2Outcome = new() { ResultText = "两人表面服从，但心中仍有不满。", DiscipleStatEffects = new[] { -5, -5, 0 } },
            Choice3Outcome = new() { ResultText = "生死斗导致一名弟子重伤，宗门上下议论纷纷。", DiscipleStatEffects = new[] { -15, -20, -30 }, ReputationChange = -10 },
        },
        new()
        {
            Id = 5, Category = EventCategory.Opportunity, Weight = 10,
            Title = "灵石矿脉",
            Description = "有弟子在宗门后山发现了一条小型灵石矿脉！",
            Choice1Text = "全力开采",
            Choice2Text = "有序开采，注意可持续发展",
            Choice3Text = "秘密开采，不让他人知晓",
            Choice1Outcome = new() { ResultText = "短期内获得了大量灵石！", ResourceChanges = { [ResourceType.SpiritStone] = 300, [ResourceType.Ore] = 20 } },
            Choice2Outcome = new() { ResultText = "稳定的灵石收入，细水长流。", ResourceChanges = { [ResourceType.SpiritStone] = 150 } },
            Choice3Outcome = new() { ResultText = "秘密开采没有被外界发现，但产量有限。", ResourceChanges = { [ResourceType.SpiritStone] = 80 } },
        },
        new()
        {
            Id = 6, Category = EventCategory.Competition, Weight = 8, MinSectLevel = 2,
            Title = "宗门大比",
            Description = "邻近宗门发来邀请，邀请贵宗参加三年一度的宗门大比。",
            Choice1Text = "派出最强弟子参加",
            Choice2Text = "派出多名弟子参与历练",
            Choice3Text = "婉拒邀请",
            Choice1Outcome = new() { ResultText = "弟子在比武中脱颖而出，为宗门赢得荣誉！", ResourceChanges = { [ResourceType.SpiritStone] = 200 }, ReputationChange = 30, PowerChange = 20, DiscipleCultivationBonus = 15 },
            Choice2Outcome = new() { ResultText = "弟子们虽未拔得头筹，但积累了宝贵经验。", DiscipleCultivationBonus = 10, ReputationChange = 10 },
            Choice3Outcome = new() { ResultText = "失去了一次扬名的机会。", ReputationChange = -5 },
        },
        new()
        {
            Id = 7, Category = EventCategory.Crisis, Weight = 8,
            Title = "魔修来袭",
            Description = "一群魔修在附近活动，意图血祭凡人修炼邪功。",
            Choice1Text = "派出精锐弟子除魔卫道",
            Choice2Text = "联合其他宗门共同讨伐",
            Choice3Text = "固守宗门，不予理会",
            Choice1Outcome = new() { ResultText = "成功击退魔修，宗门声望大增！", ReputationChange = 25, PowerChange = 20, DiscipleStatEffects = new[] { 5, 5, -15 } },
            Choice2Outcome = new() { ResultText = "联军成功剿灭魔修，虽消耗资源但收获善缘。", ResourceChanges = { [ResourceType.SpiritStone] = -80 }, ReputationChange = 20 },
            Choice3Outcome = new() { ResultText = "魔修肆虐后离开，附近凡人死伤惨重，宗门被指责不作为。", ReputationChange = -25, PowerChange = -15 },
        },
        new()
        {
            Id = 8, Category = EventCategory.Visitor, Weight = 12,
            Title = "商人来访",
            Description = "一位游商带着稀有物资来到宗门，想进行交易。",
            Choice1Text = "大量购入物资",
            Choice2Text = "以物易物",
            Choice3Text = "看看就走",
            Choice1Outcome = new() { ResultText = "获得了大量修炼物资！", ResourceChanges = { [ResourceType.SpiritStone] = -150, [ResourceType.Herb] = 30, [ResourceType.Ore] = 20, [ResourceType.Pill] = 10 } },
            Choice2Outcome = new() { ResultText = "用多余的灵草换了丹药，双方都满意。", ResourceChanges = { [ResourceType.Herb] = -15, [ResourceType.Pill] = 8 } },
            Choice3Outcome = new() { ResultText = "商人继续赶路，什么也没发生。" },
        },
        new()
        {
            Id = 9, Category = EventCategory.Internal, Weight = 10,
            Title = "灵脉喷发",
            Description = "宗门下方的灵脉突然喷发，大量灵气涌入宗门！",
            Choice1Text = "安排弟子全力吸收",
            Choice2Text = "引导灵气灌溉灵田",
            Choice3Text = "储存灵气以备后用",
            Choice1Outcome = new() { ResultText = "弟子们修为大进！", DiscipleCultivationBonus = 30 },
            Choice2Outcome = new() { ResultText = "灵田获得滋养，灵草产量大增。", ResourceChanges = { [ResourceType.Herb] = 40, [ResourceType.SpiritEssence] = 30 } },
            Choice3Outcome = new() { ResultText = "灵气被成功储存，宗门灵气浓郁。", ResourceChanges = { [ResourceType.SpiritEssence] = 50 } },
        },
        new()
        {
            Id = 10, Category = EventCategory.Opportunity, Weight = 8,
            Title = "上古遗迹",
            Description = "外出探索的弟子在深山中发现了一处上古修士留下的遗迹！",
            Choice1Text = "组织精锐队伍探索",
            Choice2Text = "先派斥候探查虚实",
            Choice3Text = "设下封印，日后再说",
            Choice1Outcome = new() { ResultText = "遗迹中收获了大量宝物和功法！", ResourceChanges = { [ResourceType.SpiritStone] = 300, [ResourceType.Pill] = 15, [ResourceType.Equipment] = 5 }, DiscipleCultivationBonus = 20, DiscipleStatEffects = new[] { 0, -5, -15 } },
            Choice2Outcome = new() { ResultText = "斥候发现了遗迹的秘密，安全收获了一批资源。", ResourceChanges = { [ResourceType.SpiritStone] = 150, [ResourceType.Pill] = 8 } },
            Choice3Outcome = new() { ResultText = "遗迹被其他宗门发现，错失良机。", ReputationChange = -10 },
        },
        new()
        {
            Id = 11, Category = EventCategory.Crisis, Weight = 10, MinSectLevel = 3,
            Title = "邪修入侵",
            Description = "一群邪修盯上了宗门的灵脉，集结人手准备强攻！",
            Choice1Text = "开启护山大阵死守",
            Choice2Text = "主动出击，先发制人",
            Choice3Text = "向邻近宗门求援",
            Choice1Outcome = new() { ResultText = "护山大阵成功抵挡了攻击，但消耗了大量灵石。", ResourceChanges = { [ResourceType.SpiritStone] = -200 }, ReputationChange = 10, PowerChange = 15 },
            Choice2Outcome = new() { ResultText = "突袭成功！邪修溃散，缴获不少战利品。", ResourceChanges = { [ResourceType.SpiritStone] = 100, [ResourceType.Pill] = 10 }, ReputationChange = 20, PowerChange = 25, DiscipleStatEffects = new[] { 5, 5, -20 } },
            Choice3Outcome = new() { ResultText = "援军赶到，邪修退去，但欠下了人情。", ReputationChange = 5, ResourceChanges = { [ResourceType.SpiritStone] = -80 } },
        },
        new()
        {
            Id = 12, Category = EventCategory.Opportunity, Weight = 10,
            Title = "灵兽来投",
            Description = "一只通体雪白的灵兽出现在宗门附近，似乎对宗门有好感。",
            Choice1Text = "收留下来",
            Choice2Text = "观察一段时间",
            Choice3Text = "任其离去",
            Choice1Outcome = new() { ResultText = "灵兽在宗门住下，灵气变得更加浓郁了！", ResourceChanges = { [ResourceType.SpiritEssence] = 20 }, DiscipleCultivationBonus = 5 },
            Choice2Outcome = new() { ResultText = "灵兽在附近徘徊不去，弟子们时常能看到它。", ResourceChanges = { [ResourceType.SpiritEssence] = 10 } },
            Choice3Outcome = new() { ResultText = "灵兽消失在远方。" },
        },
        new()
        {
            Id = 13, Category = EventCategory.Internal, Weight = 12,
            Title = "弟子顿悟",
            Description = "一名弟子在修炼中突然进入顿悟状态，灵光涌现！",
            Choice1Text = "全力护法，确保安全",
            Choice2Text = "召集其他弟子观摩学习",
            Choice3Text = "不去打扰，任其自然",
            Choice1Outcome = new() { ResultText = "弟子成功顿悟，修为大涨！其他弟子也从中获益。", DiscipleCultivationBonus = 25, DiscipleStatEffects = new[] { 5, 10, 0 } },
            Choice2Outcome = new() { ResultText = "观摩的弟子们都有所领悟。", DiscipleCultivationBonus = 15 },
            Choice3Outcome = new() { ResultText = "顿悟自然结束，弟子略有收获。", DiscipleCultivationBonus = 10 },
        },
        // ====== Companion Events (道侣) ======
        new()
        {
            Id = 20, Category = EventCategory.Internal, Weight = 10, MinSectLevel = 2,
            Title = "道侣情劫",
            Description = "一对道侣遭遇情劫考验，心魔丛生，若渡不过恐伤及感情和修为。",
            Choice1Text = "护法引导，助其渡过",
            Choice2Text = "让二人自行面对",
            Choice3Text = "强行分开闭关",
            Choice1Outcome = new() { ResultText = "道侣同心协力渡过情劫，感情更深，修为精进！", DiscipleCultivationBonus = 20, DiscipleStatEffects = new[] { 5, 15, 0 }, ResourceChanges = { [ResourceType.SpiritStone] = -50 } },
            Choice2Outcome = new() { ResultText = "二人勉强渡过，各有感悟但情感受创。", DiscipleCultivationBonus = 10, DiscipleStatEffects = new[] { -5, -10, -5 } },
            Choice3Outcome = new() { ResultText = "强行分开避免了大祸，但二人心中留下芥蒂。", DiscipleStatEffects = new[] { -10, -20, 0 }, ReputationChange = -5 },
        },
        new()
        {
            Id = 21, Category = EventCategory.Opportunity, Weight = 8,
            Title = "双修顿悟",
            Description = "宗内一对道侣在双修时心意相通，双双进入顿悟状态，灵气激荡！",
            Choice1Text = "全力供应灵石助其突破",
            Choice2Text = "护法守护，任其自然",
            Choice3Text = "打断修炼，以防不测",
            Choice1Outcome = new() { ResultText = "道侣双双突破，宗门灵气大盛！", DiscipleCultivationBonus = 35, ResourceChanges = { [ResourceType.SpiritStone] = -100 }, ReputationChange = 15 },
            Choice2Outcome = new() { ResultText = "二人自然完成顿悟，修为稳步提升。", DiscipleCultivationBonus = 20 },
            Choice3Outcome = new() { ResultText = "虽然打断了一次机缘，但避免了走火入魔的风险。", DiscipleStatEffects = new[] { -5, -10, 0 } },
        },
        new()
        {
            Id = 22, Category = EventCategory.Internal, Weight = 12,
            Title = "道侣争执",
            Description = "宗内一对道侣因修炼理念不合产生争执，气氛紧张。若不处理，可能影响整个宗门的和谐。",
            Choice1Text = "出面调解，劝说和好",
            Choice2Text = "安排二人共同执行任务",
            Choice3Text = "严厉训斥二人",
            Choice1Outcome = new() { ResultText = "二人和好如初，感情更胜从前。", DiscipleStatEffects = new[] { 5, 15, 0 }, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "任务中的配合让二人重新找回了默契。", DiscipleStatEffects = new[] { 3, 5, 0 }, DiscipleCultivationBonus = 5 },
            Choice3Outcome = new() { ResultText = "二人表面服从，但心中更加不满。", DiscipleStatEffects = new[] { -10, -15, 0 }, ReputationChange = -5 },
        },
        new()
        {
            Id = 23, Category = EventCategory.Visitor, Weight = 6, MinSectLevel = 2,
            Title = "情敌挑衅",
            Description = "一位外来修士看中了宗门中的一位弟子，试图以宝物诱惑，挑拨道侣关系。",
            Choice1Text = "严词拒绝，维护宗门风气",
            Choice2Text = "收下宝物但不予理会",
            Choice3Text = "观望事态发展",
            Choice1Outcome = new() { ResultText = "外来修士灰溜溜离去，宗门风气正派，声望提升。", ReputationChange = 10, DiscipleStatEffects = new[] { 3, 5, 0 } },
            Choice2Outcome = new() { ResultText = "宝物到手但引起道侣间猜疑，感情受损。", ResourceChanges = { [ResourceType.SpiritStone] = 50 }, DiscipleStatEffects = new[] { -5, -10, 0 } },
            Choice3Outcome = new() { ResultText = "弟子道心动摇，道缘出现裂痕。", DiscipleStatEffects = new[] { -10, -15, 0 }, ReputationChange = -10 },
        },
        new()
        {
            Id = 24, Category = EventCategory.Opportunity, Weight = 8, MinSectLevel = 2,
            Title = "道缘天赐",
            Description = "天降异象，一道七彩霞光笼罩了宗内一对道侣。古籍记载此为「天赐道缘」，是可遇不可求的机缘！",
            Choice1Text = "举办盛大庆典",
            Choice2Text = "低调吸收天赐灵气",
            Choice3Text = "顺其自然",
            Choice1Outcome = new() { ResultText = "庆典吸引了四方散修观礼，宗门声望大涨，道侣感情升华！", ReputationChange = 20, DiscipleCultivationBonus = 15, DiscipleStatEffects = new[] { 10, 20, 0 }, ResourceChanges = { [ResourceType.SpiritStone] = -80 } },
            Choice2Outcome = new() { ResultText = "天赐灵气被二人全数吸收，修为大进。", DiscipleCultivationBonus = 25, DiscipleStatEffects = new[] { 5, 10, 0 } },
            Choice3Outcome = new() { ResultText = "霞光自然消散，留下淡淡余韵，二人心有感悟。", DiscipleCultivationBonus = 10, ReputationChange = 5 },
        },
    };

    public static IReadOnlyList<EventData> GetAllEvents() => _events;

    public static List<EventData> GetAvailableEvents(int sectLevel)
    {
        return _events.Where(e => e.MinSectLevel <= sectLevel).ToList();
    }
}
