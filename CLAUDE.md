# 「剑缘修仙录」项目文档

## 必须先读
- **修改代码前，先读 `CODING_STYLE.md`** — 缩进／命名／项目约定全在里面
- 改前检查清单在 `CODING_STYLE.md` 末尾，过一遍再动手

## 项目概览
修仙宗门模拟经营游戏，Godot 4.5.1 Mono (C#)，2D UI为主。

## 技术栈
- Godot 4.5.1 Mono: `C:\Users\16689\Downloads\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64.exe`
- .NET SDK 9.0
- C# 脚本，Godot.NET.Sdk/4.5.1

## 架构
- GameManager (Autoload 单例) → 驱动所有子系统
- 子系统: TimeSystem, ResourceSystem, DiscipleSystem, FacilitySystem, EventSystem, SaveLoadManager
- EventBus: 静态C#事件总线，解耦各系统
- 数据模型: DiscipleData, FacilityData, ResourceData, EventData, SaveGameData (JSON存档)

## 场景流程
- StartMenu.tscn (主菜单) → "新游戏" → Main.tscn (游戏主界面)
- StartMenu.tscn → "继续游戏" → 选存档位 → Main.tscn
- GameManager 是 Autoload，场景切换时保持存活
- MainUI 在 _ExitTree 中退订 EventBus，防止事件泄漏
- 开始新游戏时调用 EventBus.Clear() 清理所有订阅

## 回合制时间系统
- 时间不会自动流失，由玩家操作推进
- 每次操作消耗 1 天时间（招募弟子、建造设施、指派任务、遣散弟子）
- 事件选择不消耗时间（即时处理）
- 提供"下一天"、"7天快进"、"30天快进"按钮
- 快进时若触发随机事件，会停下等待玩家选择
- 每日流水线: 设施建造/产出 → 资源到账 → 弟子修炼 → 冷却 → 随机事件

## 每日处理流水线 (GameManager.AdvanceTime)
1. FacilitySystem.ProcessDaily() → 建造推进 + 计算收入
2. ResourceSystem.ApplyDailyIncome() → 资源到账
3. DiscipleSystem.ProcessDaily() → 修炼/突破/体力消耗
4. EventSystem.ProcessCooldowns() → 冷却减少
5. EventSystem.TryTriggerEvent() → 15%随机事件(触发则暂停等待)

## 当前实现状态 (2026-05-03 更新)
### 已完成
- 所有枚举定义 (CultivationRealm, FacilityType, ResourceType, DiscipleTaskType, EventCategory)
- 所有数据模型 (DiscipleData, FacilityData, ResourceData, EventData, SaveGameData, LogEntry)
- EventBus (静态事件总线)
- 静态数值表 (CultivationTable, FacilityTable, EventTable(13个事件), DiscipleNameTable)
- TimeSystem (回合制，AdvanceDays手动推进)
- ResourceSystem (资源增删查，每日收入)
- DiscipleSystem (全部9种任务类型: 修炼/训练/采集/炼丹/炼器/授课/守卫/探索/休息，各有独立产出和成长，贡献度系统，技能学习与加成)
- FacilitySystem (建造/升级/每日产出，升级后正确增加等级，区分建造中和升级中)
- EventSystem (随机事件，权重选择，冷却)
- SaveLoadManager (JSON序列化存档，10个存档位，删除存档)
- GameManager (Autoload单例，每日流水线，宗门等级系统(7级)，自动招募，事件派发)
- MainUI (全程序化UI：顶栏资源/弟子数上限/时间，Tab页，事件弹窗，任务下拉选择，设施升级按钮，保存/读档/删除存档，弟子详情面板，游戏内读档)
- StartMenu (开始菜单: 新游戏/读档/退出，存档位选择弹窗，删除存档)
- 项目配置: 主场景StartMenu.tscn, Autoload GameManager
- 宗门等级: 声望达标自动晋升，每级增加弟子上限和设施上限
- 弟子技能系统: 7种技能(修炼加速/战斗技巧/炼丹精通/炼器精通/采集专精/教导有方/探索老手)，通过执行对应任务有几率习得，Lv.1每级提供10-15%加成
- 法器装备系统: EquipmentData模型，EquipmentTable(4品质16种装备)，炼器产出装备物品，装备/卸下UI，装备加成属性(天赋/悟性/体质/神识/战力/修炼速度)，最高2件法器
- 道侣系统: CompanionData模型，CompanionSystem(牵线/表白/双修/赠礼/和离)，好感度0-100分6档，双修加成5-50%，每日好感增减，自动求婚，道侣Tab页完整UI，弟子卡片道侣标识，详情面板道侣信息，5个道侣专属事件(情劫/双修顿悟/争执/情敌/天赐道缘)，总计18个事件
- 数值平衡: 初始灵石500/矿石20，自动招募8%/天，凡人期体力120，开局赠送铁剑+静心蒲团
- AudioManager音效系统: BGM播放/暂停/恢复，音效音量调节，交互点击音效
- 键盘快捷键: ESC键关闭弹窗/返回上级
- 侧边栏图标优化: 统一尺寸24x24改用Button.Icon，重制去透明边距+内容区居中，新增7个标签图标
- 卷宗境界分布改为中文显示
- 资源获取浮动动画: 右上角图标处跳出+X提示飘动淡出
- 入门大比完善: 确认弹窗，点击音效，卡片统一高度
- 道侣系统增强: 弟子2%/日随机结缘自动触发，牵线搭桥UI放大+显示弟子详细信息(头像/四维/心情/体力/任务/身世)
- UI重叠修复: 卡片裁剪边距、道缘按钮居中排列、宽卡片横向ExpandFill、牵线搭桥重叠、卡片悬浮动画溢出

### 待实现
- 游戏美术资源 (字体、背景、头像)
- 剧情/任务链系统
- 战斗/冒险系统
- 子嗣系统 (道侣生子，继承父母属性)

## 文件结构
- Scripts/Core/ - 核心系统 (GameManager, TimeSystem, EventBus, SaveLoadManager)
- Scripts/Data/Definitions/ - 枚举
- Scripts/Data/Models/ - 数据模型 (DiscipleData, FacilityData, ResourceData, EventData, SaveGameData, LogEntry, EquipmentData, CompanionData)
- Scripts/Data/Tables/ - 静态数值表 (CultivationTable, FacilityTable, EventTable(18个事件), DiscipleNameTable, EquipmentTable(16种装备))
- Scripts/Systems/ - 游戏子系统 (DiscipleSystem, FacilitySystem, ResourceSystem, EventSystem, CompanionSystem)
- Scripts/UI/ - UI控制器 (MainUI, StartMenu)
- Scenes/ - .tscn场景文件 (Main, GameManager, StartMenu)
- Resources/ - 美术/字体/音频资源

## 编码规范 (详见 CODING_STYLE.md)
- 缩进: **4空格** (全项目统一，不混用tab)
- namespace: file-scoped (`namespace SwordFateCultivationRecord;`)
- 类/方法: PascalCase，私有字段: `_camelCase`
- 数据模型: `{ get; set; }` 属性，JSON可序列化
- UI: 全程序化构建(无.tscn)，控制面板用 `PanelContainer`
- 子系统: 纯C#类，由GameManager持有并驱动
- 事件: EventBus静态事件总线解耦，非Godot信号
- 颜色: `Color.Color8()` 而非 `new Color(int)`
