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
        // ====== 外门 & 资源事件 ======
        new()
        {
            Id = 30, Category = EventCategory.Opportunity, Weight = 12,
            Title = "外门扩招",
            Description = "附近村镇的凡人听说宗门待遇优厚，大批年轻人前来投奔，希望成为外门弟子。",
            Choice1Text = "择优收录，扩充外门",
            Choice2Text = "挑选少数精英",
            Choice3Text = "暂时不收",
            Choice1Outcome = new() { ResultText = "外门弟子数量大增，宗门基础产出提升！", ResourceChanges = { [ResourceType.SpiritStone] = -30 }, ReputationChange = 10, DiscipleStatEffects = new[] { 0, 5, 0 } },
            Choice2Outcome = new() { ResultText = "收录了几名资质不错的弟子，其中一人或有内门之资。", DiscipleCultivationBonus = 5 },
            Choice3Outcome = new() { ResultText = "求道者失望离去，宗门口碑受损。", ReputationChange = -5 },
        },
        new()
        {
            Id = 31, Category = EventCategory.Crisis, Weight = 10,
            Title = "外门骚乱",
            Description = "外门弟子因待遇不公产生不满，聚集请愿，要求改善修炼环境。若处理不当，可能引发大规模离去。",
            Choice1Text = "拨款改善外门条件",
            Choice2Text = "派出内门弟子安抚",
            Choice3Text = "武力镇压",
            Choice1Outcome = new() { ResultText = "外门弟子感激涕零，工作效率大升！", ResourceChanges = { [ResourceType.SpiritStone] = -100 }, DiscipleStatEffects = new[] { 0, 15, 0 } },
            Choice2Outcome = new() { ResultText = "内门弟子的劝说起了作用，事态平息。", DiscipleStatEffects = new[] { 3, 0, 0 } },
            Choice3Outcome = new() { ResultText = "外门弟子敢怒不敢言，大量人员悄然离去。", ReputationChange = -15, DiscipleStatEffects = new[] { -5, -20, 0 } },
        },
        new()
        {
            Id = 32, Category = EventCategory.Opportunity, Weight = 8,
            Title = "坊市繁荣",
            Description = "宗门周边的坊市日渐繁荣，不少散修在此交易，形成了一个小型修仙集市。",
            Choice1Text = "参与经营，设点收税",
            Choice2Text = "鼓励弟子在此交换物资",
            Choice3Text = "保持距离，不涉商业",
            Choice1Outcome = new() { ResultText = "坊市为宗门带来了稳定的灵石收入！", ResourceChanges = { [ResourceType.SpiritStone] = 120, [ResourceType.Herb] = 15, [ResourceType.Ore] = 10 }, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "弟子们获得了更多修炼资源，心情愉悦。", DiscipleStatEffects = new[] { 0, 10, 0 }, ResourceChanges = { [ResourceType.Pill] = 5 } },
            Choice3Outcome = new() { ResultText = "坊市独立发展，宗门收获有限。" },
        },
        new()
        {
            Id = 33, Category = EventCategory.Internal, Weight = 10,
            Title = "灵田丰收",
            Description = "今年风调雨顺，灵气充沛，宗门的灵田迎来了前所未有的大丰收！",
            Choice1Text = "扩大种植面积",
            Choice2Text = "储存灵草以备后用",
            Choice3Text = "将多余灵草对外出售",
            Choice1Outcome = new() { ResultText = "大面积种植了高产灵草，来年产量有望倍增！", ResourceChanges = { [ResourceType.Herb] = 60 } },
            Choice2Outcome = new() { ResultText = "灵草被妥善保存，足够宗门一年之用。", ResourceChanges = { [ResourceType.Herb] = 40 } },
            Choice3Outcome = new() { ResultText = "出售灵草获得了不少灵石。", ResourceChanges = { [ResourceType.Herb] = 10, [ResourceType.SpiritStone] = 80 } },
        },
        new()
        {
            Id = 34, Category = EventCategory.Visitor, Weight = 10, MinSectLevel = 2,
            Title = "炼器大师",
            Description = "一位炼器大师慕名来到宗门，愿意传授独门炼器技巧，但需要提供材料供其实验。",
            Choice1Text = "全力支持，供其所需",
            Choice2Text = "限量供应材料",
            Choice3Text = "请大师指导弟子即可",
            Choice1Outcome = new() { ResultText = "大师感动于宗门诚意，留下了几件法器并传授了技艺！", ResourceChanges = { [ResourceType.Ore] = -30, [ResourceType.Equipment] = 8 }, DiscipleCultivationBonus = 5 },
            Choice2Outcome = new() { ResultText = "大师完成了几件实验作品，留作酬谢。", ResourceChanges = { [ResourceType.Ore] = -15, [ResourceType.Equipment] = 3 } },
            Choice3Outcome = new() { ResultText = "弟子们学到了基础炼器技巧，收获有限但也有用。", ResourceChanges = { [ResourceType.Equipment] = 1 } },
        },
        new()
        {
            Id = 35, Category = EventCategory.Crisis, Weight = 8, MinSectLevel = 2,
            Title = "丹药失窃",
            Description = "宗门库房中的一批丹药不翼而飞！经查，乃是一名弟子监守自盗。",
            Choice1Text = "严惩不贷，杀一儆百",
            Choice2Text = "从轻发落，给其改过的机会",
            Choice3Text = "私下处理，低调解决",
            Choice1Outcome = new() { ResultText = "严厉的惩罚震慑了其他人，但宗门气氛变得紧张。", DiscipleStatEffects = new[] { 5, -10, 0 }, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "犯错弟子痛哭流涕，发誓重新做人。但丹药已无法追回。", ResourceChanges = { [ResourceType.Pill] = -5 }, DiscipleStatEffects = new[] { 0, -5, 0 } },
            Choice3Outcome = new() { ResultText = "事情被压了下来，但私下闲言碎语不断。", DiscipleStatEffects = new[] { -5, -5, 0 } },
        },
        new()
        {
            Id = 36, Category = EventCategory.Opportunity, Weight = 8, MinSectLevel = 3,
            Title = "秘境入口",
            Description = "宗门附近空间突然扭曲，一个上古秘境入口凭空出现！但这个秘境似乎只在每月的月圆之夜开放。",
            Choice1Text = "组织精锐探索秘境",
            Choice2Text = "先驻扎封锁，慢慢研究",
            Choice3Text = "邀请友好宗门共同探索",
            Choice1Outcome = new() { ResultText = "秘境中收获了大量上古遗宝和功法！但部分弟子受了轻伤。", ResourceChanges = { [ResourceType.SpiritStone] = 250, [ResourceType.Pill] = 12, [ResourceType.Equipment] = 6 }, DiscipleCultivationBonus = 15, DiscipleStatEffects = new[] { 0, 5, -15 } },
            Choice2Outcome = new() { ResultText = "逐步探索减少了风险，稳定获得了一批资源。", ResourceChanges = { [ResourceType.SpiritStone] = 150, [ResourceType.Pill] = 6 }, ReputationChange = 10 },
            Choice3Outcome = new() { ResultText = "合作探索虽然分走了一些宝物，但加深了宗门友谊。", ResourceChanges = { [ResourceType.SpiritStone] = 100, [ResourceType.Pill] = 8 }, ReputationChange = 20 },
        },
        new()
        {
            Id = 37, Category = EventCategory.Internal, Weight = 10,
            Title = "散修投效",
            Description = "几位在当地小有名气的散修前来宗门，表示愿意加入，但他们要求直接成为内门弟子。",
            Choice1Text = "破格录取为内门弟子",
            Choice2Text = "从外门做起，凭实力晋升",
            Choice3Text = "婉言谢绝",
            Choice1Outcome = new() { ResultText = "散修们加入后，宗门的实力有所提升。但部分老弟子心中不平。", DiscipleCultivationBonus = 10, DiscipleStatEffects = new[] { -5, 5, 0 }, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "散修们虽有不悦，但最终接受。其中一人展现出惊人天赋。", DiscipleStatEffects = new[] { 0, -3, 0 }, ReputationChange = 3 },
            Choice3Outcome = new() { ResultText = "散修们摇头离去，转而加入了邻近宗门。", ReputationChange = -5 },
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

        // ====== 高阶事件 (Lv.3+) ======
        new()
        {
            Id = 40, Category = EventCategory.Crisis, Weight = 8, MinSectLevel = 3,
            Title = "灵脉枯竭",
            Description = "宗门下方的灵脉出现了衰竭迹象，灵气浓度持续下降。若灵脉彻底枯竭，宗门根基将受重创！",
            Choice1Text = "投入大量灵石滋养灵脉",
            Choice2Text = "寻找新的灵脉来源",
            Choice3Text = "节约使用，减缓消耗",
            Choice1Outcome = new() { ResultText = "灵石投入后灵脉恢复了活力，甚至比以前更浓郁了！", ResourceChanges = { [ResourceType.SpiritStone] = -300, [ResourceType.SpiritEssence] = 40 }, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "探索队在百里外发现了一条新的小型灵脉，暂时缓解了危机。", ResourceChanges = { [ResourceType.SpiritEssence] = 20, [ResourceType.SpiritStone] = -80 } },
            Choice3Outcome = new() { ResultText = "灵气浓度持续下降，宗门修炼效率大降。", DiscipleStatEffects = new[] { 0, -10, 0 }, DiscipleCultivationBonus = -10 },
        },
        new()
        {
            Id = 41, Category = EventCategory.Opportunity, Weight = 8, MinSectLevel = 3,
            Title = "天劫余波",
            Description = "远方有修士渡劫失败，但其渡劫产生的天地能量尚未散去。若能善加利用，对门下弟子的修炼将是极大的助力。",
            Choice1Text = "派遣弟子前去感悟",
            Choice2Text = "收集残余天雷之力",
            Choice3Text = "不值得冒险",
            Choice1Outcome = new() { ResultText = "弟子们在劫雷余韵中获得了珍贵的感悟，部分人修为大增！", DiscipleCultivationBonus = 30, DiscipleStatEffects = new[] { 0, 5, -10 } },
            Choice2Outcome = new() { ResultText = "收集到的天雷之力被炼化成了淬体宝物。", ResourceChanges = { [ResourceType.Equipment] = 5, [ResourceType.Pill] = 8 } },
            Choice3Outcome = new() { ResultText = "天劫余波自行消散。", ReputationChange = -3 },
        },
        new()
        {
            Id = 42, Category = EventCategory.Competition, Weight = 10, MinSectLevel = 2,
            Title = "外门大比",
            Description = "外门弟子们自发组织了一场比武大会，获胜者希望获得晋升内门的机会。",
            Choice1Text = "正式承认大比，给予晋升名额",
            Choice2Text = "允许举办但不承诺晋升",
            Choice3Text = "外门弟子无需大比",
            Choice1Outcome = new() { ResultText = "外门弟子斗志昂扬，优胜者晋升内门，宗门实力增长！", DiscipleStatEffects = new[] { 5, 15, 0 }, DiscipleCultivationBonus = 8, ReputationChange = 5 },
            Choice2Outcome = new() { ResultText = "大比依然举办，但弟子们有些失望，士气略有影响。", DiscipleStatEffects = new[] { 0, -5, 0 } },
            Choice3Outcome = new() { ResultText = "外门弟子大失所望，不少人萌生去意。", DiscipleStatEffects = new[] { -5, -15, 0 } },
        },
        new()
        {
            Id = 43, Category = EventCategory.Visitor, Weight = 8, MinSectLevel = 3,
            Title = "仙人讲道",
            Description = "一位路过的化神期大修士被宗门的修炼氛围打动，愿意停留三日为弟子们讲授大道至理。",
            Choice1Text = "盛情接待，广邀听众",
            Choice2Text = "低调安排内门弟子听讲",
            Choice3Text = "询问仙人意图后再决定",
            Choice1Outcome = new() { ResultText = "讲道三日，听者如云。弟子们收获巨大，宗门声望大涨！", DiscipleCultivationBonus = 35, ReputationChange = 20, ResourceChanges = { [ResourceType.Pill] = -10 } },
            Choice2Outcome = new() { ResultText = "内门弟子受益匪浅，不少人有突破迹象。", DiscipleCultivationBonus = 25 },
            Choice3Outcome = new() { ResultText = "仙人满意离去，留下几本修炼心得。", DiscipleCultivationBonus = 15, ResourceChanges = { [ResourceType.Pill] = 3 } },
        },
        new()
        {
            Id = 44, Category = EventCategory.Crisis, Weight = 10, MinSectLevel = 2,
            Title = "瘟疫蔓延",
            Description = "一种奇异的灵草瘟疫在药田中蔓延，染病的灵草迅速枯萎。若不及时控制，宗门灵草供给将受严重影响！",
            Choice1Text = "用灵石购买特效药",
            Choice2Text = "隔离病株，烧毁感染区",
            Choice3Text = "尝试用丹药救治灵草",
            Choice1Outcome = new() { ResultText = "特效药效果显著，灵草保住了大半。", ResourceChanges = { [ResourceType.SpiritStone] = -80, [ResourceType.Herb] = -10 } },
            Choice2Outcome = new() { ResultText = "烧毁了近半灵田，但瘟疫被彻底控制。", ResourceChanges = { [ResourceType.Herb] = -30 } },
            Choice3Outcome = new() { ResultText = "丹药效果有限，灵草损失严重，但获得了宝贵的防疫经验。", ResourceChanges = { [ResourceType.Herb] = -20, [ResourceType.Pill] = -5 } },
        },
        new()
        {
            Id = 45, Category = EventCategory.Opportunity, Weight = 10, MinSectLevel = 2,
            Title = "古宝出土",
            Description = "暴雨冲刷后，宗门后山露出一处古墓入口，隐约可见其中藏有古宝。但古墓中可能潜藏危险。",
            Choice1Text = "率队深入古墓探查",
            Choice2Text = "小心挖掘，逐步推进",
            Choice3Text = "封存古墓，来日再探",
            Choice1Outcome = new() { ResultText = "古墓中获得了数件古宝和大量丹药！但遭遇了机关陷阱，部分弟子受伤。", ResourceChanges = { [ResourceType.Equipment] = 10, [ResourceType.Pill] = 8, [ResourceType.SpiritStone] = 50 }, DiscipleStatEffects = new[] { 0, 5, -20 } },
            Choice2Outcome = new() { ResultText = "安全获得了一批古宝，虽然数量少了些，但无人伤亡。", ResourceChanges = { [ResourceType.Equipment] = 5, [ResourceType.Pill] = 3 } },
            Choice3Outcome = new() { ResultText = "古墓被封存，但消息已经传出，两个月后被他人捷足先登。", ReputationChange = -5 },
        },
        new()
        {
            Id = 46, Category = EventCategory.Opportunity, Weight = 6, MinSectLevel = 4,
            Title = "天道感应",
            Description = "天地异象骤然降临！宗门的道统似乎与天道产生了某种奇妙的共鸣。一股浩瀚的天地之力笼罩了整个宗门。",
            Choice1Text = "引导天道之力灌注宗门根基",
            Choice2Text = "引导天道之力融入弟子修炼",
            Choice3Text = "尝试参悟天道玄机",
            Choice1Outcome = new() { ResultText = "天道之力融入宗门根基，灵脉变得更加深厚，声望大振！", ResourceChanges = { [ResourceType.SpiritEssence] = 100, [ResourceType.SpiritStone] = 200 }, ReputationChange = 30, DiscipleStatEffects = new[] { 5, 10, 0 } },
            Choice2Outcome = new() { ResultText = "天道之力涌入弟子体内，多人当场突破！", DiscipleCultivationBonus = 50, DiscipleStatEffects = new[] { 10, 20, 0 }, ReputationChange = 10 },
            Choice3Outcome = new() { ResultText = "你从天道感应中领悟了一丝大道至理。", DiscipleCultivationBonus = 25, ReputationChange = 15 },
        },
    };

    public static IReadOnlyList<EventData> GetAllEvents() => _events;

    public static List<EventData> GetAvailableEvents(int sectLevel)
    {
        return _events.Where(e => e.MinSectLevel <= sectLevel).ToList();
    }
}
