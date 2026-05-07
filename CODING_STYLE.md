# 剑缘修仙录 · 代码规范

## 缩进与格式

| 规则 | 标准 | 说明 |
|------|------|------|
| 缩进 | **4空格** | 不使用 Tab。所有 `.cs` 文件统一。 |
| 大括号 | **同行**（K&R风格） | `if (x) {` 左大括号不换行 |
| 命名空间 | **file-scoped** | `namespace SwordFateCultivationRecord;` 无大括号包裹 |
| 行尾 | **CRLF**（Windows兼容） | Git 自动转换 |

## 命名规范

| 类型 | 风格 | 示例 |
|------|------|------|
| 类/枚举/record | **PascalCase** | `DiscipleSystem`, `CultivationRealm` |
| 方法/属性 | **PascalCase** | `ProcessDaily()`, `CombatPower` |
| 局部变量/参数 | **camelCase** | `discipleCount`, `healthStr` |
| 私有字段 | `_camelCase` | `_pendingNewborns`, `rng` |
| 常量/static readonly | **PascalCase** | `MaxSlots`, `DaysPerMonth` |
| 枚举值 | **PascalCase** | `QiRefining`, `SpiritStone` |

## 项目约定

### C# 特性
- 使用 `=>` 表达式体（expression-bodied members）用于简单属性/方法
- 使用 `record` 类型定义数据模板（如 `FacilityInfo`, `RealmInfo`）
- 使用 `init` 属性设置器（`{ get; init; }`）定义不可变模板数据
- 使用 `is` 模式匹配而非 `==` 比较枚举

### Godot 约定
- **UI 全程序化构建**：不使用 `.tscn` 场景文件，所有 UI 在 `_Ready()` 中通过 C# 代码构建
- 颜色使用 `Color.Color8(r, g, b, a)` 而非 `new Color(r, g, b)`（后者是 0-1 浮点）
- `_Ready` 期间不能直接 `AddChild` 到 root，需 `CallDeferred`
- Panel 非 Container，用 `PanelContainer` 或手动 `SetAnchors`
- 不使用 Godot 信号系统，用 `EventBus` 静态 C# 事件解耦

### 数据层
- `Scripts/Data/Definitions/` — 枚举定义，纯文件
- `Scripts/Data/Models/` — 数据模型（`class`，属性 `{ get; set; }`），保持 JSON 可序列化
- `Scripts/Data/Tables/` — 静态数值表（`static class`，`record` 模板）

### 子系统架构
- 每个子系统是纯 C# 类（不继承 `Node`），由 `GameManager` 持有并驱动
- `EventBus` 静态事件总线的订阅必须在 `GameManager.InitializeNewGame()` 和 `LoadGame()` 中重建
- 存档兼容：新增字段必须在 `SaveGameData` + `SaveLoadManager.CreateSaveData/ApplySaveData` 中三处同步修改
- 每日流水线在 `GameManager.RunDailyCycle()` 中顺序执行

### 健康检查清单（改前过一遍）

- [ ] 新增文件缩进是 **4空格** 吗？
- [ ] 新增字段能在存档中正确保存/读取吗？
- [ ] UI 用程序化构建了吗（不用 .tscn）？
- [ ] 事件订阅在 `_ExitTree` 或 `LoadGame` 中正确重建了吗？
- [ ] 新增颜色用 `Color.Color8()` 吗？
- [ ] 文件命名用 PascalCase 吗？
