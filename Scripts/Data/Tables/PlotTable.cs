namespace SwordFateCultivationRecord;

public static class PlotTable
{
    public static List<PlotStageDef> AllStages => new()
    {
        new PlotStageDef
        {
            Id = 1, ChapterId = 1, Order = 1,
            ChapterTitle = "卷一·凡尘初醒",
            Title = "序·祖师遗命",
            Narrative = "太古洪荒，仙道茫茫。\n\n你本是山中一介樵夫，偶遇天降陨星，坠入古洞，得遇一具坐化千年的仙人遗骸。仙人虽已陨落，一缕神识尚存，临终前将毕生所学——《剑元经》授予于你。\n\n「吾乃云霄剑尊，三千年前渡劫失败，神魂将散。今将剑道传承托付于你，望你开宗立派，光大道统，莫使剑道断绝于世间...」\n\n剑尊神识化作点点星光消散，古洞中仅余一柄古剑与一卷剑经。\n\n你背负传承，走出深山，在一块灵气充沛的山谷中立下剑碑，刻上宗门之名——「无名剑宗」。\n\n修仙之路，自此而始。",
            Objective = "了解宗门概况",
            CompletionHint = "浏览宗门总览页，熟悉界面布局后，点击下方「确认」按钮继续",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.None },
            CompletionConditions = new List<PlotCondition> { new() { Type = PlotConditionType.None } },
            CompletionText = "你定下心神，环顾宗门。石碑已立，道统当兴。从今日起，你便是这无名剑宗的掌门。",
        },
        new PlotStageDef
        {
            Id = 2, ChapterId = 1, Order = 2,
            Title = "壹·广纳门徒",
            Narrative = "宗门已立，然而孤掌难鸣。\n\n你抚摸着剑尊留下的古剑，心中明白——修仙之路漫漫，一人之力终究有限。要想在修真界立足，必须广纳贤才，培养弟子。\n\n你走进附近城镇，在茶楼酒肆间宣扬宗门之道。渐渐地，开始有凡人慕名而来，渴望踏上修仙之路...",
            Objective = "招募弟子达到3人",
            CompletionHint = "点底部「内门选拔」按钮，七日后选拔弟子加入宗门",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 1 },
            CompletionConditions = new List<PlotCondition> { new() { Type = PlotConditionType.DiscipleCount, TargetValue = 3 } },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 100 } },
            CompletionText = "三位弟子恭敬行礼，齐声道：「师父！」你看着眼前这些青涩的面孔，心中涌起一股暖流。这便是你宗门的第一批弟子，也是你在这条修仙路上最初的同行者。",
        },
        new PlotStageDef
        {
            Id = 3, ChapterId = 1, Order = 3,
            Title = "贰·安身立命",
            Narrative = "弟子已有，然而宗门尚无遮风避雨之处。\n\n弟子们在露天地打坐修炼，风吹日晒，修炼效率十分低下。你决定建造修炼设施，为弟子们提供一个安心修炼的场所。\n\n你召集弟子，开始规划宗门的第一座灵筑...",
            Objective = "营造1座灵筑",
            CompletionHint = "切换到「营造」页，选择建造任意灵筑（修炼室/药园等）",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 2 },
            CompletionConditions = new List<PlotCondition> { new() { Type = PlotConditionType.FacilityCount, TargetValue = 1 } },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.Ore, 30 } },
            CompletionText = "灵筑落成之日，弟子们欢呼雀跃。从此，宗门有了第一座属于自己的灵筑。虽然简陋，却是宗门发展的基石。",
        },
        new PlotStageDef
        {
            Id = 4, ChapterId = 1, Order = 4,
            Title = "叁·初窥门径",
            Narrative = "宗门运转渐入正轨，弟子们每日勤修不辍。\n\n然而，你还未曾见过真正的修仙者。凡人期的弟子虽然勤奋，但离真正的「修仙」还有一步之遥——突破至练气期。\n\n练气期是修仙的门槛，只有突破此境，才算真正踏上了仙途。你悉心指导弟子修炼，期待第一位突破者的出现...",
            Objective = "让任意1名弟子突破至练气期",
            CompletionHint = "将弟子分配至「修炼」任务，等待修为足够后自动尝试突破",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 3 },
            CompletionConditions = new List<PlotCondition> { new() { Type = PlotConditionType.DiscipleRealm, TargetValue = (int)CultivationRealm.QiRefining } },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.Pill, 5 } },
            RewardReputation = 30,
            CompletionText = "一道灵光冲天而起！那名弟子周身灵气涌动，终于突破了凡人桎梏，踏入练气之境。你感受到天地灵气向宗门汇聚——这便是修仙的感觉！",
        },
        new PlotStageDef
        {
            Id = 5, ChapterId = 1, Order = 5,
            Title = "肆·宗门初立",
            Narrative = "有了练气期弟子坐镇，宗门终于不再是无名小卒。\n\n但这还远远不够。要想在修真界真正立足，宗门需要声望和资源。只有积累足够的声名和财富，才能吸引更多天才弟子，建造更强的灵筑。\n\n你开始全力发展宗门，向着真正的修仙门派迈进...",
            Objective = "声望达到100，灵石达到500",
            CompletionHint = "持续推演天时，发展宗门各项事务。建造更多灵筑、培养弟子以积累声望和灵石",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 4 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 100 },
                new() { Type = PlotConditionType.ResourceAmount, TargetValue = 500, ResourceType = ResourceType.SpiritStone },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 200 } },
            RewardReputation = 50,
            CompletionText = "—— 卷一·终 ——\n\n宗门终于站稳了脚跟。从一个人到一群弟子，从荒山野岭到初具规模——你用了短短数年，便在这片大陆上刻下了「无名剑宗」的名字。\n\n然而，这仅仅是开始。修真界浩瀚无垠，元婴大能、化神至尊、甚至传说中的大乘飞升...都在前方等着你。",
        },

        // ===== 卷二·崭露头角 =====
        new PlotStageDef
        {
            Id = 6, ChapterId = 2, Order = 1,
            ChapterTitle = "卷二·崭露头角",
            Title = "壹·声名渐起",
            Narrative = "宗门初立，声名渐起。\n\n附近村镇的凡人开始传颂「无名剑宗」的名号。有修炼资质的年轻人纷纷前来投奔，周边的小势力也开始注意到这个新兴的修仙门派。\n\n然而，名气也带来了麻烦。一些散修开始窥探宗门的灵脉，更有甚者试图混入宗门窃取功法。你意识到，光有弟子还不够——宗门需要更强的实力来守护自己。",
            Objective = "声望达到200，灵筑达到2座",
            CompletionHint = "继续发展宗门，建造更多灵筑，通过事件和宗门令积累声望",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 5 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 200 },
                new() { Type = PlotConditionType.FacilityCount, TargetValue = 2 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 300 } },
            RewardReputation = 30,
            CompletionText = "随着宗门名声传开，慕名而来的求道者越来越多。你站在山门前，看着络绎不绝的人群，心中涌起一股自豪——这个曾经默默无闻的小宗门，终于在这片大陆上拥有了自己的一席之地。",
        },
        new PlotStageDef
        {
            Id = 7, ChapterId = 2, Order = 2,
            Title = "贰·丹器之道",
            Narrative = "宗门壮大，弟子的需求也日益增长。\n\n修炼需要丹药辅助，战斗需要法器护身。仅靠从外界购买远远不够——价格昂贵不说，品质也难以保证。你决定大力发展炼丹和炼器之道，让宗门自给自足。\n\n你召集了最有天赋的弟子，传授从剑尊遗物中领悟的炼丹炼器之法...",
            Objective = "拥有丹药10颗，法器5件",
            CompletionHint = "指派弟子执行「炼丹」「炼器」任务，建造丹房和炼药房可加速产出",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 6 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.ResourceAmount, TargetValue = 10, ResourceType = ResourceType.Pill },
                new() { Type = PlotConditionType.ResourceAmount, TargetValue = 5, ResourceType = ResourceType.Equipment },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.Ore, 50 }, { ResourceType.Herb, 50 } },
            CompletionText = "第一炉丹药出炉时，药香弥漫整个山谷。弟子们围在丹房外，眼中闪烁着兴奋的光芒。从此，宗门有了自己的丹药和法器来源，不再受制于人。",
        },
        new PlotStageDef
        {
            Id = 8, ChapterId = 2, Order = 3,
            Title = "叁·广收门徒",
            Narrative = "宗门声名日盛，前来求道者络绎不绝。\n\n但你也深知——弟子贵精不贵多。只有经过严格选拔的内门弟子，才能真正传承剑尊的道统。你决定扩大内门选拔的规模，从更多求道者中筛选真正的天才。\n\n与此同时，外门弟子的人数也在快速增长，宗门周边形成了一片热闹的坊市...",
            Objective = "内门弟子达到5人",
            CompletionHint = "举办内门选拔，建造会客厅可以增加候选人数和可选名额",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 7 },
            CompletionConditions = new List<PlotCondition> { new() { Type = PlotConditionType.DiscipleCount, TargetValue = 5 } },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 200 } },
            CompletionText = "五位内门弟子在演武场上列队而立，气势初显。你看着他们，心中明白——这五人才是宗门真正的根基。外门数百弟子提供资源，内门弟子传承道法，宗门终于有了一个真正修仙门派的样子。",
        },
        new PlotStageDef
        {
            Id = 9, ChapterId = 2, Order = 4,
            Title = "肆·风云渐起",
            Narrative = "树大招风。宗门的崛起引起了周边势力的注意。\n\n一些散修开始在宗门附近出没，偶尔还有妖兽袭扰外围。虽然暂时构不成威胁，但你知道——修真界从来不是和平之地。弱小的宗门会被吞并，强大的宗门才能生存。\n\n你必须做好准备，应对随时可能到来的危机...",
            Objective = "宗门等级达到2级，战力达到50",
            CompletionHint = "继续积累声望提升宗门等级，安排弟子执行「守卫」「训练」任务提升战力",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 8 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.SectLevel, TargetValue = 2 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 300 } },
            RewardReputation = 30,
            CompletionText = "宗门晋升至Lv.2！一股无形的气势从山门扩散开来，方圆百里的散修都感受到了这股波动。他们知道——无名剑宗，已经不是那个可以随意欺凌的小门派了。",
        },
        new PlotStageDef
        {
            Id = 10, ChapterId = 2, Order = 5,
            Title = "伍·名动一方",
            Narrative = "宗门终于从「无名小宗」蜕变为「初露锋芒」的修仙势力。\n\n你的名声开始传遍这片大陆的东南一隅。更有一些小型修仙家族主动遣使来访，希望与宗门建立联系。你明白，这既是机遇也是挑战。\n\n要在修真界真正立足，宗门需要更强的实力、更高的声望。你决定向更高的目标发起冲击——让无名剑宗成为这片大陆上不可忽视的力量。",
            Objective = "声望达到400，宗门等级达到3级",
            CompletionHint = "全面发展宗门：建造更多灵筑、培养弟子突破、完成宗门令积累声望",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 9 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 400 },
                new() { Type = PlotConditionType.SectLevel, TargetValue = 3 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 500 } },
            RewardReputation = 50,
            CompletionText = "—— 卷二·终 ——\n\n无名剑宗的名字，终于在这片大陆上有了分量。从昔日的荒山小派，到如今小有名气的修仙势力——这条路你走了数年，但你知道，这仅仅是开始。\n\n远处的地平线上，更大的机遇和更深的危机正在酝酿。而你，已经做好了迎接一切的准备。",
        },

        // ===== 卷三·威震一方 =====
        new PlotStageDef
        {
            Id = 11, ChapterId = 3, Order = 1,
            ChapterTitle = "卷三·威震一方",
            Title = "壹·邻宗来访",
            Narrative = "宗门在东南一隅已小有名气。\n\n一日，山门外锣鼓喧天——邻近的「青云门」派遣使者前来，递上了正式的外交文书。青云门是这片区域的老牌宗门，实力不俗。他们的来访，意味着无名剑宗已经被视为平等的对话对象。\n\n使者表达了建立友好关系的意愿，但也暗示修真界暗流涌动——据传北方的魔道势力正在蠢蠢欲动。",
            Objective = "宗门等级达到3级，内门弟子8人",
            CompletionHint = "继续提升宗门等级和招收内门弟子。建造会客厅可增加内门选拔候选人数。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 10 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.SectLevel, TargetValue = 3 },
                new() { Type = PlotConditionType.DiscipleCount, TargetValue = 8 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 400 } },
            RewardReputation = 40,
            CompletionText = "青云门使者满意而归，两宗正式建立了友好关系。从此，无名剑宗不再是一个孤立的门派——你开始在这片大陆的修仙势力中拥有了一席之地。",
        },
        new PlotStageDef
        {
            Id = 12, ChapterId = 3, Order = 2,
            Title = "贰·魔影重现",
            Narrative = "青云门的警告并非空穴来风。\n\n北方的魔道势力「血煞宗」开始向东南扩张。多个小宗门已被吞并，附近的散修纷纷南逃。据逃来的散修所言，血煞宗有一位金丹期的长老坐镇，实力远超这片区域的任何一个宗门。\n\n你必须做好准备。战争，或许不可避免。",
            Objective = "拥有法器10件",
            CompletionHint = "安排弟子执行「炼器」任务积累法器装备，建造炼药房和丹房可加速产出",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 11 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.ResourceAmount, TargetValue = 10, ResourceType = ResourceType.Equipment },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.Pill, 10 }, { ResourceType.Ore, 40 } },
            RewardReputation = 30,
            CompletionText = "弟子们披挂整齐，法器在手。虽然面对血煞宗的威胁仍显不足，但宗门已经做好了战斗的准备。你知道——真正的修仙界，从来不是温室。",
        },
        new PlotStageDef
        {
            Id = 13, ChapterId = 3, Order = 3,
            Title = "叁·联盟之势",
            Narrative = "面对血煞宗的威胁，单打独斗绝非明智之举。\n\n你决定联络周边宗门，组建抗魔联盟。除了青云门之外，还有「碧水阁」「金鼎派」等中小势力。你需要说服他们——只有联合起来，才能抵御血煞宗的入侵。\n\n这不仅需要实力，更需要声望和诚意。",
            Objective = "声望达到600，宗门等级达到4级",
            CompletionHint = "通过完成宗门令、处理随机事件积累声望。建造阵法殿可增强宗门防御。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 12 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 600 },
                new() { Type = PlotConditionType.SectLevel, TargetValue = 4 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 600 } },
            RewardReputation = 50,
            CompletionText = "四宗联盟正式成立！你被推举为盟主——这是对你实力的认可，也是对无名剑宗的信任。联盟的旗帜在山门外高高飘扬，方圆千里的修士都感受到了一股新的力量正在崛起。",
        },
        new PlotStageDef
        {
            Id = 14, ChapterId = 3, Order = 4,
            Title = "肆·秘境深处",
            Narrative = "联盟初成，但情报不足。\n\n你从一处古籍中得知，在东南山脉深处，隐藏着一座上古大能的洞府——「云霄洞天」。据传洞天之中不仅有海量资源，还有那位大能留下的传承功法。\n\n然而，洞天外围遍布上古禁制，进入其中风险极大。但若能成功探索，宗门实力将迎来质的飞跃。",
            Objective = "拥有15颗丹药",
            CompletionHint = "安排弟子炼丹积累丹药。建造丹房和炼药房可加速丹药产出。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 13 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.ResourceAmount, TargetValue = 15, ResourceType = ResourceType.Pill },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 400 }, { ResourceType.Equipment, 8 }, { ResourceType.Pill, 10 } },
            RewardReputation = 40,
            CompletionText = "探索队从云霄洞天平安归来！虽然损失了几件法器，但带回了大量修炼资源和一本残缺的上古功法。弟子们围在篝火旁，眼中闪烁着兴奋的光芒——这就是修仙，充满了危险，也充满了机遇。",
        },
        new PlotStageDef
        {
            Id = 15, ChapterId = 3, Order = 5,
            Title = "伍·威震一方",
            Narrative = "血煞宗终于按捺不住，派出一支精锐向联盟发起试探性进攻。\n\n这是无名剑宗成立以来面临的最大考验。你站在山门前，身后是所有的内门弟子和联盟援军。远处，血煞宗的黑色旗帜在风中猎猎作响。\n\n这一战，将决定宗门在修真界的地位。",
            Objective = "声望达到1000，至少1名弟子达到筑基期",
            CompletionHint = "全力发展宗门：升级灵筑、培养弟子突破、积累声望。突破筑基期需要修为进度达到180，突破概率60%。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 14 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 1000 },
                new() { Type = PlotConditionType.DiscipleRealm, TargetValue = (int)CultivationRealm.Foundation },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 800 } },
            RewardReputation = 80,
            CompletionText = "—— 卷三·终 ——\n\n血煞宗的试探被成功击退！联盟弟子的欢呼声响彻云霄。从今日起，无名剑宗从「一方豪强」正式踏入了真正强者的行列。\n\n然而，血煞宗的主力并未出动。你知道，真正的决战还在前方。但此刻，你有盟友、有弟子、有实力——无论前方是什么，你都不会退缩。",
		},

        // ===== 卷四·问道长生 =====
        new PlotStageDef
        {
            Id = 16, ChapterId = 4, Order = 1,
            ChapterTitle = "卷四·问道长生",
            Title = "壹·金丹大道",
            Narrative = "血煞宗暂时退去，但你深知——要彻底解决威胁，宗门必须有金丹期强者坐镇。\n\n金丹期，是修仙路上的第一道真正的分水岭。一颗金丹吞入腹，始知我命不由天。你召集了最有潜力的弟子，倾尽宗门资源助其冲击金丹之境。\n\n这不仅关乎一场战争的胜负，更是宗门能否踏入真正修仙大派行列的关键。",
            Objective = "至少1名弟子达到金丹期，宗门等级达到5级",
            CompletionHint = "集中培养一名弟子，优先安排修炼任务。金丹期需要修为进度700，突破概率35%，需要消耗大量灵石。建造静修室、布置灵气加成可提效。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 15 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.DiscipleRealm, TargetValue = (int)CultivationRealm.CoreFormation },
                new() { Type = PlotConditionType.SectLevel, TargetValue = 5 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 1000 }, { ResourceType.Pill, 15 } },
            RewardReputation = 100,
            CompletionText = "天雷轰鸣，金丹结成！一道金色光柱从修炼室冲天而起，方圆百里的修士都感受到了这股威压。宗门第一位金丹真人诞生了！弟子们跪拜在地，眼中满是敬畏——从今日起，无名剑宗真正踏入了强者之列。",
        },
        new PlotStageDef
        {
            Id = 17, ChapterId = 4, Order = 2,
            Title = "贰·决战血煞",
            Narrative = "金丹真人坐镇，联盟士气大振。\n\n你收到消息——血煞宗的主力正在集结，其宗主——一位金丹后期的魔道强者——发誓要将联盟夷为平地。决战的时刻到了。\n\n你站在中军帐中，面前是联盟各宗的掌门。所有人都在等待你的命令。这一战，将决定东南修仙界未来百年的格局。",
            Objective = "声望达到2000，宗门战力达到500",
            CompletionHint = "通过完成高级宗门令、处理高阶随机事件积累声望。安排弟子训练和守卫可提升战力，法器装备也能大幅增加战力。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 16 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 2000 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 1200 }, { ResourceType.Equipment, 12 } },
            RewardReputation = 150,
            CompletionText = "血煞宗主力在联盟的围攻下溃不成军！其宗主重伤遁逃，魔道势力从此在东南一蹶不振。联盟各宗欢声雷动，你被尊为「东南盟主」——无名剑宗，已成这片区域当之无愧的霸主。",
        },
        new PlotStageDef
        {
            Id = 18, ChapterId = 4, Order = 3,
            Title = "叁·宗门鼎盛",
            Narrative = "血煞宗覆灭后，宗门进入了前所未有的鼎盛时期。\n\n四方散修来投，外门弟子数以千计。内门之中，筑基弟子已非罕见，金丹真人也增至数位。宗门的灵筑遍布山谷，丹房的炉火日夜不息，演武场上剑气纵横。\n\n然而，你心中仍有更高的追求——云霄剑尊的遗愿是「光大道统」，这远不是一方霸主就能满足的。",
            Objective = "声望达到3500，宗门等级达到7级（满级）",
            CompletionHint = "这是最后的冲刺。全面发展宗门各项指标，升级所有灵筑，培养更多高阶弟子。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 17 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 3500 },
                new() { Type = PlotConditionType.SectLevel, TargetValue = 7 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 2000 }, { ResourceType.Pill, 20 }, { ResourceType.Equipment, 10 } },
            RewardReputation = 200,
            CompletionText = "宗门晋升至 Lv.7——「仙道圣地」！数千外门弟子、数十内门精英、数位金丹真人……从昔日荒山中的一块剑碑，到如今令整个大陆侧目的修仙圣地。剑尊在天之灵，当可安息。",
        },
        new PlotStageDef
        {
            Id = 19, ChapterId = 4, Order = 4,
            Title = "肆·天道之问",
            Narrative = "宗门鼎盛，但修仙之路永无止境。\n\n一日，天降异象——一道天道光芒笼罩了你的闭关之所。冥冥中，你感受到了一股超越元婴、超越化神的力量召唤。那是传说中的「飞升之机」，是所有修仙者梦寐以求的终极目标。\n\n但天道无情，欲得飞升之机，宗门必须通过天道的考验——证明你所开创的道统，足以流传万世。",
            Objective = "声望达到5000，拥有弟子合计超过100人（內门+外门）",
            CompletionHint = "继续积累声望，同时大力发展外门弟子数量。建造会客厅和藏经阁可提升管理上限。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 18 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 5000 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 2500 }, { ResourceType.Pill, 25 } },
            RewardReputation = 200,
            CompletionText = "天道之光笼罩宗门整整七日。七日之中，所有弟子都感受到了大道的洗礼，修为各有精进。天道认可了你的道统——无名剑宗，已不仅仅是东南一隅的霸主，而是受到天道眷顾的仙道圣地。",
        },
        new PlotStageDef
        {
            Id = 20, ChapterId = 4, Order = 5,
            Title = "伍·万宗至尊",
            Narrative = "天道认可，万宗来朝。\n\n来自大陆各地的宗门纷纷遣使朝贺。元婴大能、化神至尊——这些曾经只在传说中出现的名字，如今都向你发来了邀请。\n\n云霄剑尊若在天有灵，当微笑颔首。他留下的传承，不仅没有断绝，反而在这片大陆上发扬光大。\n\n你站在山门最高处，俯瞰脚下繁荣的宗门。远处，一道彩虹横跨天际——那是天道为你铺就的大道之路。修仙之道，永无止境。而你，已经在这条路上，刻下了不可磨灭的印记。",
            Objective = "声望达到6000，拥有「万宗至尊」称号",
            CompletionHint = "这是最终目标。全力冲刺吧——你离修仙界的巅峰，只差这最后一步。",
            TriggerCondition = new PlotCondition { Type = PlotConditionType.StageCompleted, TargetValue = 19 },
            CompletionConditions = new List<PlotCondition>
            {
                new() { Type = PlotConditionType.Reputation, TargetValue = 6000 },
            },
            RewardResources = new Dictionary<ResourceType, int> { { ResourceType.SpiritStone, 5000 }, { ResourceType.Pill, 50 }, { ResourceType.Equipment, 20 } },
            RewardReputation = 500,
            CompletionText = "—— 卷四·终 · 全剧终 ——\n\n无名剑宗。\n\n从荒山中的一块孤零零的剑碑，到如今大陆之巅的万宗至尊。这一路走来，有弟子的离去，有强敌的威胁，有资源的匮乏，有天道的考验。\n\n但你从未放弃。\n\n因为你相信——修仙之道，不在天，不在命，而在人。\n\n云霄剑尊的传承在你手中发扬光大。而你开创的道统，也将在弟子们的手中代代相传，直到永远。\n\n【剑缘修仙录 · 全篇完】",
        },
    };

    public static PlotStageDef? Get(int id) => AllStages.FirstOrDefault(s => s.Id == id);
    public static List<PlotStageDef> GetByChapter(int chapterId) => AllStages.Where(s => s.ChapterId == chapterId).OrderBy(s => s.Order).ToList();
}
