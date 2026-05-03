namespace SwordFateCultivationRecord;

public static class DiscipleNameTable
{
    private static readonly string[] _surnames =
    {
        "林", "陈", "李", "王", "张", "刘", "赵", "周", "吴", "郑",
        "慕容", "上官", "南宫", "西门", "司徒", "欧阳", "令狐", "独孤",
        "叶", "萧", "陆", "苏", "白", "顾", "沈", "江", "谢", "秦",
        "柳", "云", "韩", "唐", "许", "杨", "朱", "徐", "何", "孙",
        "司马", "诸葛", "东方", "夏侯", "尉迟", "宇文", "长孙", "皇甫",
        "温", "卫", "姜", "蒋", "范", "彭", "鲁", "韦", "马", "方",
        "任", "袁", "邓", "曹", "梁", "宋", "潘", "杜", "田", "董",
        "晏", "薛", "段", "雷", "贺", "龙", "万", "钟", "汪", "阮",
    };

    // Male-style given name characters: heroic, strong, scholarly
    private static readonly string[] _maleA = // first character pool
    {
        "剑", "浩", "云", "天", "玄", "子", "长", "明", "飞", "文",
        "景", "正", "宏", "玉", "志", "承", "修", "行", "启", "昭",
        "永", "元", "宗", "仁", "俊", "彦", "松", "柏", "恒", "远",
    };

    private static readonly string[] _maleB = // second character pool
    {
        "风", "然", "宇", "辰", "清", "轩", "白", "尘", "羽", "锋",
        "阳", "平", "辉", "山", "川", "峰", "林", "河", "海", "庭",
        "生", "成", "华", "初", "言", "诚", "之", "和", "安", "宁",
    };

    // Female-style given name characters: graceful, beautiful, elegant
    private static readonly string[] _femaleA =
    {
        "若", "如", "清", "雪", "灵", "婉", "紫", "凝", "静", "思",
        "映", "含", "梦", "诗", "雨", "晓", "秋", "月", "芳", "幽",
        "素", "碧", "琼", "瑶", "采", "冰", "凤", "云", "蝶", "落",
    };

    private static readonly string[] _femaleB =
    {
        "薇", "烟", "兰", "嫣", "芸", "蓉", "萱", "霜", "琴", "画",
        "影", "汐", "漪", "珊", "纱", "荷", "莹", "音", "笛", "筝",
        "鸾", "曦", "蕊", "竹", "梅", "柳", "桃", "莲", "菊", "芝",
    };

    // Neutral suffix that works for either gender (single-char given names)
    private static readonly string[] _neutralGiven =
    {
        "逸", "瑾", "彦", "瑜", "熙", "瑞", "霖", "晟", "哲", "昊",
        "皓", "烨", "煜", "炜", "炫", "琰", "珺", "琦", "琪", "琳",
        "瑶", "玥", "瑄", "璇", "璞", "琬", "玦", "琨", "琮", "璎",
    };

    private static readonly Random _rng = new();

    public static string GenerateName(bool isMale)
    {
        var surname = _surnames[_rng.Next(_surnames.Length)];

        // 70% chance: two-char given name (A+B)
        // 20% chance: single-char neutral name
        // 10% chance: three-char name (A+B+C from the same pools, shuffled)
        double roll = _rng.NextDouble();

        if (roll < 0.20)
        {
            // Single-character given name
            return surname + _neutralGiven[_rng.Next(_neutralGiven.Length)];
        }
        else if (roll < 0.30 && isMale)
        {
            // Three-character male name: A + B + another from A or B
            var pool = _rng.Next(2) == 0 ? _maleA : _maleB;
            return surname + _maleA[_rng.Next(_maleA.Length)]
                          + _maleB[_rng.Next(_maleB.Length)]
                          + pool[_rng.Next(pool.Length)];
        }
        else if (roll < 0.30 && !isMale)
        {
            // Three-character female name
            var pool = _rng.Next(2) == 0 ? _femaleA : _femaleB;
            return surname + _femaleA[_rng.Next(_femaleA.Length)]
                          + _femaleB[_rng.Next(_femaleB.Length)]
                          + pool[_rng.Next(pool.Length)];
        }
        else
        {
            // Standard two-character given name
            var a = isMale ? _maleA : _femaleA;
            var b = isMale ? _maleB : _femaleB;
            return surname + a[_rng.Next(a.Length)] + b[_rng.Next(b.Length)];
        }
    }

    public static (int talent, int comprehension, int constitution, int spirit) GenerateStats()
    {
        int remaining = 200;
        int t = _rng.Next(20, 81);
        remaining -= t;
        int co = _rng.Next(20, Math.Min(81, remaining + 20));
        remaining -= co - 20;
        int cn = _rng.Next(20, Math.Min(81, remaining + 20));
        remaining -= cn - 20;
        int s = Math.Clamp(remaining + 20, 20, 100);
        return (t, co, cn, s);
    }

    // ===== Background / Personality / Trait Generation =====

    private static readonly (string name, int talentBias, int compBias, int constBias, int spiritBias)[] _backgrounds =
    {
        ("农家子弟",       0,  0, +15,  0),
        ("书香门第",       0, +15,  0,  0),
        ("武道世家",     +15,  0,  0,  0),
        ("商贾之后",       0,  0,  0, +15),
        ("猎户之子",     +10,  0, +10,  0),
        ("医道世家",       0, +10,  0, +10),
        ("没落贵族",     +5, +5,  0,  0),
        ("深山隐修",     +5,  0,  0, +10),
        ("渔家少年",       0,  0, +5, +5),
        ("铁匠传人",     +10,  0, +5,  0),
        ("流浪孤儿",       0,  0,  0,  0),
        ("镖师之后",       0,  0, +10,  0),
        ("画师门第",       0, +10,  0, +5),
        ("乐师世家",       0, +5,  0, +10),
        ("方士传人",       0, +5, +5, +5),
    };

    private static readonly string[] _personalities =
    {
        "沉稳持重", "机敏灵动", "坚韧不拔", "温润如玉",
        "孤傲清高", "豪爽豁达", "内敛沉静", "争强好胜",
        "随遇而安", "野心勃勃", "谦逊和善", "冷峻寡言",
        "活泼开朗", "心思缜密", "刚正不阿", "洒脱不羁",
    };

    private static readonly string[] _traits =
    {
        "过目不忘", "天生神力", "剑心通明", "灵觉敏锐",
        "道心坚定", "巧手匠心", "福缘深厚", "百折不挠",
        "无", "无", "无", "无",  // 40% chance of no special trait
    };

    public static (string bg, string pers, string trait) GenerateFlavor()
    {
        var bg = _backgrounds[_rng.Next(_backgrounds.Length)];
        string pers = _personalities[_rng.Next(_personalities.Length)];
        string trait = _traits[_rng.Next(_traits.Length)];
        return (bg.name, pers, trait);
    }

    /// <summary>Adjust stats based on background bias, then clamp.</summary>
    public static (int t, int co, int cn, int s) ApplyBackgroundBias(
        int t, int co, int cn, int s, string bgName)
    {
        var bg = _backgrounds.FirstOrDefault(b => b.name == bgName);
        if (bg.name == null) return (t, co, cn, s);
        return (
            Math.Clamp(t + bg.talentBias, 15, 100),
            Math.Clamp(co + bg.compBias,  15, 100),
            Math.Clamp(cn + bg.constBias, 15, 100),
            Math.Clamp(s  + bg.spiritBias, 15, 100)
        );
    }
}
