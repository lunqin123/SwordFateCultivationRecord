using System.ComponentModel;

namespace DeepSeekDeskBand;

public class Form1 : Form
{
    private Label _costValue = null!;
    private Label _costSub = null!;
    private Label _balMini = null!;
    private Label _tokMini = null!;
    private Label _cacheMini = null!;
    private Label _statusLabel = null!;
    private System.Windows.Forms.Timer _refreshTimer = null!;
    private NotifyIcon _trayIcon = null!;
    private bool _updating, _expanded;
    private Panel? _expandPanel;
    private readonly List<UsageDayData> _usageData = [];
    private double _cacheRate;
    private string _lastBalText = "余额 —";
    private string _cachedCost = "¥ —", _cachedTok = "—", _cachedTokSub = "",
                   _cachedCache = "—", _cachedCacheSub = "", _cachedReq = "—";
    private readonly Dictionary<string, Label> _expandLabels = [];

    static readonly Color Bg = Color.FromArgb(14, 16, 26);
    static readonly Color BorderC = Color.FromArgb(30, 34, 55);
    static readonly Color Cyan = Color.FromArgb(0, 229, 255);
    static readonly Color Green = Color.FromArgb(0, 230, 118);
    static readonly Color Pink = Color.FromArgb(255, 64, 129);
    static readonly Color Purple = Color.FromArgb(179, 136, 255);
    static readonly Color Orange = Color.FromArgb(255, 145, 0);
    static readonly Color Text1 = Color.FromArgb(232, 232, 232);
    static readonly Color Text2 = Color.FromArgb(120, 130, 140);

    private const int CompactW = 260;
    private const int CompactH = 40;
    private const int ExpandH = 240;

    public Form1()
    {
        FormBorderStyle = FormBorderStyle.None;
        Size = new(CompactW, CompactH);
        BackColor = Bg;
        TopMost = true;
        ShowInTaskbar = false;
        MinimumSize = new(200, CompactH);
        Opacity = 0.90;

        var fB = new Font("Microsoft YaHei UI", 13f, FontStyle.Bold);
        var fS = new Font("Microsoft YaHei UI", 8.5f);
        var fXS = new Font("Microsoft YaHei UI", 7.5f);

        _costValue = new() { Text = "¥ —", Font = fB, ForeColor = Pink, AutoSize = true, Location = new(10, 4) };
        _costSub = new() { Text = "今日消费", Font = fXS, ForeColor = Text2, AutoSize = true, Location = new(10, 25) };
        _balMini = new() { Text = "余额 —", Font = fS, ForeColor = Text2, AutoSize = true, Location = new(130, 12) };
        _tokMini = new() { Text = "T —", Font = fS, ForeColor = Green, AutoSize = true, Location = new(200, 4) };
        _cacheMini = new() { Text = "缓存 —", Font = fS, ForeColor = Purple, AutoSize = true, Location = new(200, 20) };
        _statusLabel = new() { Text = "", Font = fXS, ForeColor = Text2, AutoSize = true, Location = new(230, 12) };

        Controls.AddRange([_costValue, _costSub, _balMini, _tokMini, _cacheMini, _statusLabel]);

        Click += (s, e) => ToggleExpand();
        _costValue.Click += (s, e) => ToggleExpand();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("⚙ 设置", null, (s, e) => ShowSettings());
        ctx.Items.Add("隐藏到托盘", null, (s, e) => HideToTray());
        ctx.Items.Add("退出", null, (s, e) => Quit());
        ContextMenuStrip = ctx;

        _trayIcon = new()
        {
            Text = "DeepSeek · 用量看板",
            Icon = SystemIcons.Shield,
            Visible = true,
        };
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("显示", null, (s, e) => ShowFromTray());
        trayMenu.Items.Add("退出", null, (s, e) => Quit());
        _trayIcon.ContextMenuStrip = trayMenu;
        _trayIcon.DoubleClick += (s, e) => ShowFromTray();

        _refreshTimer = new() { Interval = ConfigManager.LoadInterval() * 1000 };
        _refreshTimer.Tick += async (s, e) => await RefreshData();
        _refreshTimer.Start();

        Load += async (s, e) =>
        {
            await Task.Delay(1500);
            await RefreshData();
        };
    }

    private void ToggleExpand()
    {
        _expanded = !_expanded;
        if (_expanded) Expand(); else Compact();
    }

    private void Compact()
    {
        _expanded = false;
        _expandPanel?.Dispose(); _expandPanel = null;
        Size = new(CompactW, CompactH);
        Controls.Clear();
        Controls.AddRange([_costValue, _costSub, _balMini, _tokMini, _cacheMini, _statusLabel]);
        Reposition();
    }

    private void Expand()
    {
        _expanded = true;
        SuspendLayout();
        Controls.Clear();

        var fB = new Font("Microsoft YaHei UI", 20f, FontStyle.Bold);
        var f = new Font("Microsoft YaHei UI", 10f);
        var fS = new Font("Microsoft YaHei UI", 9f);
        var fXS = new Font("Microsoft YaHei UI", 8f);

        int y = 8;
        AddLabel("title", "DeepSeek", new Font("Microsoft YaHei UI", 11f, FontStyle.Bold), Cyan, 12, y); y += 24;
        AddLabel("cost", _cachedCost, fB, Pink, 12, y); y += 28;
        AddLabel("", "今日消费", fS, Text2, 14, y); y += 24;
        AddLine(12, y); y += 8;
        AddLabel("bal", _lastBalText, f, Text1, 12, y); y += 22;
        AddLabel("tok", _cachedTok, f, Green, 12, y);
        AddLabel("tokSub", _cachedTokSub, fXS, Text2, 14, y + 18); y += 38;
        AddLabel("cache", _cachedCache, f, Purple, 12, y);
        AddLabel("cacheSub", _cachedCacheSub, fXS, Text2, 14, y + 18); y += 38;
        AddLabel("req", $"请求 {_cachedReq}", f, Orange, 12, y); y += 26;
        AddLabel("sts", $"✓ {DateTime.Now:HH:mm:ss}", fXS, Text2, 12, y);

        Size = new(CompactW, ExpandH);
        ResumeLayout();
        Reposition();
    }

    private void AddLabel(string key, string text, Font font, Color color, int x, int y)
    {
        var lbl = new Label { Text = text, Font = font, ForeColor = color, AutoSize = true, Location = new(x, y) };
        Controls.Add(lbl);
        if (!string.IsNullOrEmpty(key)) _expandLabels[key] = lbl;
    }

    private void AddLine(int x, int y)
    {
        Controls.Add(new Panel { BackColor = BorderC, Size = new(CompactW - 24, 1), Location = new(x, y) });
    }

    private void Reposition() => Program.RepositionTopmost(this);

    private async Task RefreshData()
    {
        if (_updating) return;
        _updating = true;
        try
        {
            var key = ConfigManager.LoadApiKey();
            if (string.IsNullOrEmpty(key)) { SetCompact("¥ —", "余额 —", "T —", "缓存 —", "未设置Key"); _updating = false; return; }
            ApiClient.SetApiKey(key);

            var bal = await ApiClient.FetchBalanceAsync();
            if (bal == null) { SetCompact("✗", "连接失败", "—", "—", "✗"); _updating = false; return; }

            var sym = bal.Currency == "CNY" ? "¥" : "$";
            _lastBalText = $"余额 {sym}{bal.TotalBalance:F2} | 赠{sym}{bal.GrantedBalance:F2}";

            var prev = ConfigManager.GetLastBalance();
            if (prev > 0 && prev > bal.TotalBalance + 0.0001)
            {
                var delta = prev - bal.TotalBalance;
                ConfigManager.AddConsumption(DateTime.Today.ToString("yyyy-MM-dd"), delta, (long)(delta / 2.0 * 1_000_000));
            }
            ConfigManager.SaveLastBalance(bal.TotalBalance);

            if (_usageData.Count == 0)
            {
                var recs = await ApiClient.FetchUsageAsync(DateTime.Today.AddDays(-29), DateTime.Today);
                if (recs.Count > 0) { _usageData.Clear(); _usageData.AddRange(Aggregate(recs)); CalcCache(); }
            }

            var today = ConfigManager.GetTodayConsumption();
            string cost, tok, tokSub, req;
            if (today != null && today.Cost > 0)
            {
                cost = $"{sym}{today.Cost:F2}"; tok = $"{Fmt(today.Tokens)} T";
                tokSub = $"¥{today.Cost:F2} · {today.Events}次"; req = today.Events.ToString();
            }
            else
            {
                var tu = _usageData.FirstOrDefault(d => d.Date == DateTime.Today.ToString("yyyy-MM-dd"));
                if (tu != null && tu.TotalTokens > 0)
                {
                    cost = $"{sym}{tu.Cost:F2}"; tok = $"{Fmt(tu.TotalTokens)} T";
                    tokSub = $"入{Fmt(tu.PromptTokens)} 出{Fmt(tu.CompletionTokens)}"; req = tu.Requests.ToString();
                }
                else { cost = "¥ —"; tok = "T —"; tokSub = ""; req = "—"; }
            }

            var cache = _usageData.Count > 0 ? $"{_cacheRate:F1}%" : "—";
            var cacheSub = _usageData.Count > 0 ? $"入{Fmt(_usageData.Sum(d => d.PromptTokens))} 出{Fmt(_usageData.Sum(d => d.CompletionTokens))}" : "";
            _cachedCost = cost; _cachedTok = tok; _cachedTokSub = tokSub;
            _cachedCache = cache; _cachedCacheSub = cacheSub; _cachedReq = req;

            var ts = $"✓ {DateTime.Now:HH:mm:ss}";
            SetCompact(cost, _lastBalText, tok, cache, ts);
            if (_expanded)
            {
                SetExpand("cost", cost); SetExpand("bal", _lastBalText);
                SetExpand("tok", tok); SetExpand("tokSub", tokSub);
                SetExpand("cache", cache); SetExpand("cacheSub", cacheSub);
                SetExpand("req", $"请求 {req}"); SetExpand("sts", ts);
            }
            _trayIcon.Text = $"DeepSeek · {cost} · {tok}";
        }
        catch { SetCompact("✗", "错误", "—", "—", "✗"); }
        _updating = false;
    }

    private void SetCompact(string cost, string bal, string tok, string cache, string sts)
    {
        if (InvokeRequired) { BeginInvoke(() => SetCompact(cost, bal, tok, cache, sts)); return; }
        _costValue.Text = cost; _balMini.Text = bal; _tokMini.Text = tok;
        _cacheMini.Text = cache; _statusLabel.Text = sts;
    }

    private void SetExpand(string key, string text)
    {
        if (_expandLabels.TryGetValue(key, out var lbl)) lbl.Text = text;
    }

    private static List<UsageDayData> Aggregate(List<UsageRecord> records)
    {
        var days = new Dictionary<string, UsageDayData>();
        foreach (var r in records)
        {
            var dk = r.Timestamp.Length >= 10 ? r.Timestamp[..10] : DateTime.Today.ToString("yyyy-MM-dd");
            if (!days.TryGetValue(dk, out var d)) days[dk] = d = new() { Date = dk };
            d.PromptTokens += r.PromptTokens; d.CompletionTokens += r.CompletionTokens; d.Requests++;
            var p = ApiClient.GetPricing(r.Model);
            d.Cost += (r.PromptTokens / 1_000_000.0) * p.InputPrice + (r.CompletionTokens / 1_000_000.0) * p.OutputPrice;
        }
        return [.. days.Values.OrderBy(d => d.Date)];
    }

    private void CalcCache()
    {
        long tp = _usageData.Sum(d => d.PromptTokens);
        long tc = _usageData.Sum(d => d.CompletionTokens);
        if (tp == 0) { _cacheRate = 0; return; }
        var pr = ApiClient.GetPricing("deepseek-chat");
        double noCache = (tp / 1_000_000.0) * pr.InputPrice + (tc / 1_000_000.0) * pr.OutputPrice;
        double actual = _usageData.Sum(d => d.Cost);
        if (actual >= noCache * 0.99) { _cacheRate = 0; return; }
        double saved = noCache - actual, diff = pr.InputPrice - pr.CacheHitPrice;
        if (diff > 0) _cacheRate = Math.Min(saved / diff * 1_000_000 / tp * 100, 99.9);
    }

    private static string Fmt(long n) => n >= 1_000_000 ? $"{n / 1_000_000.0:F2}M"
        : n >= 1_000 ? $"{n / 1000.0:F1}K" : n.ToString();

    private void ShowSettings()
    {
        using var dlg = new SettingsDialog();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _refreshTimer.Interval = dlg.Interval * 1000;
            _refreshTimer.Stop(); _refreshTimer.Start();
            Task.Run(RefreshData);
        }
    }

    private void HideToTray() { Hide(); _refreshTimer.Stop(); }
    private void ShowFromTray() { Show(); Reposition(); _refreshTimer.Start(); Task.Run(RefreshData); }
    private void Quit() { _refreshTimer.Stop(); _trayIcon.Visible = false; _trayIcon.Dispose(); Application.Exit(); }
}

public class SettingsDialog : Form
{
    private readonly TextBox _keyBox;
    private readonly ComboBox _intCombo;
    public int Interval => int.TryParse(_intCombo.Text, out var i) ? i : 30;

    public SettingsDialog()
    {
        Text = "DeepSeek 设置"; Size = new(400, 170);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new("Microsoft YaHei UI", 10f);
        BackColor = Color.FromArgb(18, 22, 36); ForeColor = Color.White;

        int y = 15;
        Controls.Add(new Label { Text = "API Key:", Location = new(12, y), AutoSize = true });
        _keyBox = new()
        {
            Location = new(85, y - 2), Size = new(290, 24),
            PasswordChar = '●', Text = ConfigManager.LoadApiKey(),
        };
        y += 35;
        Controls.Add(new Label { Text = "刷新间隔:", Location = new(12, y), AutoSize = true });
        _intCombo = new()
        {
            Location = new(85, y - 2), Size = new(70, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        _intCombo.Items.AddRange(["15", "30", "60", "120", "300"]);
        _intCombo.Text = ConfigManager.LoadInterval().ToString();
        y += 40;

        var btn = new Button
        {
            Text = "保存", Location = new(150, y), Size = new(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 200, 100), ForeColor = Color.Black,
        };
        btn.Click += (s, e) =>
        {
            var key = _keyBox.Text.Trim();
            if (!string.IsNullOrEmpty(key)) ConfigManager.SaveApiKey(key, Interval);
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(_keyBox); Controls.Add(btn);
    }
}
