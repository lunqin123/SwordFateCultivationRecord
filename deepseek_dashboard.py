#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DeepSeek · 悬浮用量看板
亚克力半透明 · 可调边框 · 实时余额/Token/缓存命中率 · 系统托盘
"""

import json, math, os, re, sys, threading
from collections import defaultdict
from dataclasses import dataclass
from datetime import datetime, timedelta, date

import requests
import customtkinter as ctk
from PIL import Image, ImageDraw
import pystray

# ─── 路径 ─────────────────────────────────────────────────
CDIR = os.path.dirname(os.path.abspath(
    sys.executable if getattr(sys, 'frozen', False) else __file__))
CFG = os.path.join(CDIR, ".deepseek_config.json")

BALANCE_API = "https://api.deepseek.com/user/balance"
USAGE_API   = "https://api.deepseek.com/v1/usage"
PRICING_URL = "https://api-docs.deepseek.com/zh-cn/quick_start/pricing"

DEFAULT_PRICING = {
    "deepseek-v4-flash": {"input": 1.0, "output": 2.0, "cache_hit": 0.02},
    "deepseek-chat":     {"input": 1.0, "output": 2.0, "cache_hit": 0.02},
    "deepseek-v4-pro":   {"input": 3.0, "output": 6.0, "cache_hit": 0.025},
    "deepseek-reasoner": {"input": 3.0, "output": 6.0, "cache_hit": 0.025},
    "default":           {"input": 1.0, "output": 2.0, "cache_hit": 0.02},
}

# ─── 亚克力样式 ──────────────────────────────────────────
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("blue")
F = "Microsoft YaHei UI"
def ff(s=12, w="normal"): return ctk.CTkFont(family=F, size=s, weight=w)

A = {
    "bg": "#0c0e1a", "card": "#151830", "border": "#252a50",
    "cyan": "#00e5ff", "green": "#00e676", "pink": "#ff4081",
    "purple": "#b388ff", "orange": "#ff9100",
    "text": "#e8e8e8", "text2": "#6a7a8a",
}
WINDOW_ALPHA = 0.88
RESIZE_MARGIN = 6


# ═══════════════════════════════════════════════════════════
@dataclass
class Balance:
    total: float = 0.0; granted: float = 0.0; topped: float = 0.0
    currency: str = "CNY"

@dataclass
class UsageDay:
    date: date; prompt_tokens: int = 0; completion_tokens: int = 0
    cost: float = 0.0; requests: int = 0
    @property
    def tokens(self): return self.prompt_tokens + self.completion_tokens


# ═══════════════════════════════════════════════════════════
class Config:
    def __init__(self):
        self.d = self._load()
    def _load(self):
        if os.path.exists(CFG):
            try:
                with open(CFG, "r", encoding="utf-8") as f: return json.load(f)
            except: pass
        return {"api_key": "", "pricing": {}, "interval": 30,
                "consumption": {}, "last_balance": None, "usage_cache": [],
                "win_geo": ""}
    def save(self):
        with open(CFG, "w", encoding="utf-8") as f:
            json.dump(self.d, f, indent=2, ensure_ascii=False)

    @property
    def key(self): return self.d.get("api_key", "")
    @key.setter
    def key(self, v): self.d["api_key"] = v.strip(); self.save()

    @property
    def interval(self): return self.d.get("interval", 30)
    @interval.setter
    def interval(self, v): self.d["interval"] = max(10, min(3600, v)); self.save()

    def pricing(self, m):
        c = self.d.get("pricing", {})
        return c.get(m) or DEFAULT_PRICING.get(m, DEFAULT_PRICING["default"])

    def get_c(self): return self.d.get("consumption", {})
    def add_c(self, day, cost, tokens):
        cd = self.d.setdefault("consumption", {})
        cd.setdefault(day, {"cost": 0.0, "tokens": 0, "events": 0})
        cd[day]["cost"] += cost; cd[day]["tokens"] += tokens
        cd[day]["events"] += 1
        self.d["consumption"] = dict(sorted(cd.items())[-90:]); self.save()

    @property
    def last_bal(self): return self.d.get("last_balance")
    @last_bal.setter
    def last_bal(self, t):
        self.d["last_balance"] = {"total": t, "time": datetime.now().isoformat()}; self.save()

    def load_usage(self):
        return [UsageDay(date=date.fromisoformat(x["date"]),
                prompt_tokens=x["p"], completion_tokens=x["c"],
                cost=x["cost"], requests=x["n"])
                for x in self.d.get("usage_cache", [])]
    def save_usage(self, data):
        self.d["usage_cache"] = [{"date": d.date.isoformat(), "p": d.prompt_tokens,
                "c": d.completion_tokens, "cost": d.cost, "n": d.requests} for d in data]
        self.save()

    def save_geo(self, geo): self.d["win_geo"] = geo; self.save()
    def load_geo(self): return self.d.get("win_geo", "")


# ═══════════════════════════════════════════════════════════
class API:
    def __init__(self, key=""):
        self.s = requests.Session()
        if key: self.s.headers.update({"Authorization": f"Bearer {key}",
                                        "Accept": "application/json"})
    def set_key(self, k):
        self.s.headers.update({"Authorization": f"Bearer {k}",
                               "Accept": "application/json"})
    def balance(self):
        try:
            r = self.s.get(BALANCE_API, timeout=15)
            if r.status_code == 200:
                d = r.json(); b = Balance()
                for bi in d.get("balance_infos", []):
                    b.currency = bi.get("currency", "CNY")
                    b.total += float(bi.get("total_balance", 0))
                    b.granted += float(bi.get("granted_balance", 0))
                    b.topped += float(bi.get("topped_up_balance", 0))
                if not d.get("balance_infos"):
                    b.total = float(d.get("total_balance", 0))
                return True, b
            return False, None
        except: return False, None

    def usage(self, start, end):
        try:
            r = self.s.get(USAGE_API, timeout=30,
                          params={"start_date": start.isoformat(),
                                  "end_date": end.isoformat()})
            if r.status_code == 200:
                return True, r.json().get("data", []) if isinstance(r.json(), dict) else []
            return False, []
        except: return False, []


# ═══════════════════════════════════════════════════════════
class App(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.cfg = Config()
        self.api = API(self.cfg.key)
        self._tid = None; self._busy = False; self._bal = None
        self._cache_rate = 0.0; self._pa = 0.0
        self._usage = self.cfg.load_usage()
        self._in_tray = False  # 窗口是否已最小化到托盘

        # 亚克力悬浮窗
        self.overrideredirect(True)
        self.attributes("-topmost", True)
        self.attributes("-alpha", WINDOW_ALPHA)
        self.configure(fg_color=A["bg"])

        geo = self.cfg.load_geo()
        self.geometry(geo if geo else
                     f"290x275+{self.winfo_screenwidth()-310}+{40}")
        self.minsize(230, 210)

        # 事件
        self.bind("<Button-1>", self._drag_start)
        self.bind("<B1-Motion>", self._drag_motion)
        self.bind("<Motion>", self._on_motion)
        self.bind("<ButtonRelease-1>", self._release)
        self.bind("<Button-3>", self._rclick)
        self._drag_mode = None
        self._drag_sx = self._drag_sy = 0
        self._start_w = self._start_h = self._start_x = self._start_y = 0

        # 托盘
        self._tray = None
        self._build_tray()

        self._build()
        self._anim()

        # 拦截窗口关闭 → 最小化到托盘
        self.protocol("WM_DELETE_WINDOW", self._to_tray)

        if self.cfg.key:
            self.after(600, self._refresh)
            self._start()

    # ─── 系统托盘 ─────────────────────────────────────────
    def _build_tray(self):
        # 用 Pillow 画 64x64 托盘图标
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        # 圆角矩形底
        draw.rounded_rectangle([4, 4, 60, 60], radius=12,
                               fill=(0, 229, 255, 220))
        # DS 文字
        draw.text((14, 12), "DS", fill=(0, 0, 0, 255))
        self._tray_icon_img = img

        menu = pystray.Menu(
            pystray.MenuItem("显示/隐藏", self._tray_toggle, default=True),
            pystray.MenuItem("退出", self._tray_quit),
        )
        self._tray = pystray.Icon(
            "deepseek", self._tray_icon_img, "DeepSeek · 用量看板", menu)
        threading.Thread(target=self._tray.run, daemon=True).start()

    def _to_tray(self):
        """关闭窗口 → 最小化到托盘"""
        self.withdraw()
        self._in_tray = True

    def _tray_toggle(self, icon=None, item=None):
        """托盘图标点击 → 切换窗口可见性"""
        self.after(0, self._toggle_window)

    def _toggle_window(self):
        if self._in_tray:
            self.deiconify()
            self.attributes("-topmost", True)
            self._in_tray = False
        else:
            self.withdraw()
            self._in_tray = True

    def _tray_quit(self, icon=None, item=None):
        """托盘退出 → 真正关闭"""
        self._stop()
        if hasattr(self, '_aid'): self.after_cancel(self._aid)
        if self._tray: self._tray.stop()
        self.cfg.save_geo(self.geometry())
        self.destroy()

    # ─── 边缘检测 / 缩放 ──────────────────────────────────
    def _get_resize_mode(self, ex, ey):
        w, h = self.winfo_width(), self.winfo_height()
        m = RESIZE_MARGIN
        l = ex <= m; r = ex >= w - m; t = ey <= m; b = ey >= h - m
        if l and t: return "nw"
        if r and t: return "ne"
        if l and b: return "sw"
        if r and b: return "se"
        if l: return "w"
        if r: return "e"
        if t: return "n"
        if b: return "s"
        return None

    def _on_motion(self, e):
        mode = self._get_resize_mode(e.x, e.y)
        cmap = {"nw":"size_nw_se","se":"size_nw_se","ne":"size_ne_sw",
                "sw":"size_ne_sw","n":"size_ns","s":"size_ns",
                "w":"size_we","e":"size_we"}
        self.configure(cursor=cmap.get(mode, "arrow"))

    def _drag_start(self, e):
        self._drag_sx = e.x_root; self._drag_sy = e.y_root
        self._drag_mode = self._get_resize_mode(e.x, e.y)
        if self._drag_mode is None: self._drag_mode = "move"
        g = self.geometry()
        parts = g.split("+")
        self._start_w, self._start_h = map(int, parts[0].split("x"))
        self._start_x = int(parts[1]); self._start_y = int(parts[2])

    def _drag_motion(self, e):
        tdx = e.x_root - self._drag_sx; tdy = e.y_root - self._drag_sy
        if self._drag_mode == "move":
            self.geometry(f"+{self._start_x+tdx}+{self._start_y+tdy}")
            return
        w0, h0 = self._start_w, self._start_h
        x0, y0 = self._start_x, self._start_y
        mode = self._drag_mode
        nw, nh = w0, h0; nx, ny = x0, y0
        if "e" in mode: nw = max(w0+tdx, 200)
        if "s" in mode: nh = max(h0+tdy, 160)
        if "w" in mode:
            nw = max(w0-tdx, 200)
            if nw > 200: nx = x0 + tdx
        if "n" in mode:
            nh = max(h0-tdy, 160)
            if nh > 160: ny = y0 + tdy
        self.geometry(f"{int(nw)}x{int(nh)}+{int(nx)}+{int(ny)}")

    def _release(self, e):
        if self._drag_mode and self._drag_mode != "move":
            self.cfg.save_geo(self.geometry())
        self._drag_mode = None

    def _rclick(self, e):
        m = ctk.CTkToplevel(self); m.overrideredirect(True)
        m.configure(fg_color=A["card"])
        m.geometry(f"100x55+{e.x_root}+{e.y_root}")
        ctk.CTkButton(m, text="最小化到托盘", font=ff(9), fg_color="transparent",
                     command=lambda: [m.destroy(), self._to_tray()]
                     ).pack(fill="x", padx=2, pady=1)
        ctk.CTkButton(m, text="⚙ 设置", font=ff(9), fg_color="transparent",
                     command=lambda: [m.destroy(), self._settings()]
                     ).pack(fill="x", padx=2, pady=1)
        ctk.CTkButton(m, text="退出", font=ff(9), fg_color="transparent",
                     command=self._tray_quit
                     ).pack(fill="x", padx=2, pady=1)
        m.focus_set(); m.bind("<FocusOut>", lambda _: m.destroy())

    # ─── UI ───────────────────────────────────────────────
    def _build(self):
        # 顶栏：标题 + 三个按钮
        top = ctk.CTkFrame(self, fg_color="transparent", height=22)
        top.pack(fill="x", padx=8, pady=(6, 0)); top.pack_propagate(False)

        self.tl = ctk.CTkLabel(top, text="DeepSeek", font=ff(13, "bold"),
                               text_color=A["cyan"])
        self.tl.pack(side="left", padx=(2, 0))

        for txt, cmd, clr in [
            ("⟳", self._refresh, A["cyan"]),
            ("⚙", self._settings, A["text2"]),
            ("—", self._to_tray, A["text2"]),  # 最小化
            ("✕", self._tray_quit, A["pink"]),
        ]:
            ctk.CTkButton(top, text=txt, width=20, height=20, font=ff(11),
                         fg_color="transparent", hover_color=A["border"],
                         text_color=clr, command=cmd).pack(side="right", padx=1)

        # 余额
        self.bal_v = ctk.CTkLabel(self, text="—", font=ff(24, "bold"),
                                   text_color=A["text"])
        self.bal_v.pack(pady=(6, 0))
        self.bal_s = ctk.CTkLabel(self, text="总余额", font=ff(9), text_color=A["text2"])
        self.bal_s.pack()

        ctk.CTkFrame(self, height=1, fg_color=A["border"]
                    ).pack(fill="x", padx=16, pady=3)

        # Token
        self.tok_v = ctk.CTkLabel(self, text="—", font=ff(13, "bold"),
                                   text_color=A["green"])
        self.tok_v.pack()
        self.tok_s = ctk.CTkLabel(self, text="今日 Token", font=ff(9),
                                   text_color=A["text2"])
        self.tok_s.pack()

        ctk.CTkFrame(self, height=1, fg_color=A["border"]
                    ).pack(fill="x", padx=24, pady=2)

        # 缓存
        self.cache_v = ctk.CTkLabel(self, text="—", font=ff(13, "bold"),
                                     text_color=A["purple"])
        self.cache_v.pack()
        self.cache_s = ctk.CTkLabel(self, text="缓存命中率", font=ff(9),
                                     text_color=A["text2"])
        self.cache_s.pack()

        ctk.CTkFrame(self, height=1, fg_color=A["border"]
                    ).pack(fill="x", padx=24, pady=2)

        # 请求
        self.req_v = ctk.CTkLabel(self, text="—", font=ff(13, "bold"),
                                   text_color=A["orange"])
        self.req_v.pack()
        self.req_s = ctk.CTkLabel(self, text="今日请求", font=ff(9),
                                   text_color=A["text2"])
        self.req_s.pack()

        # 状态
        self.sts = ctk.CTkLabel(self, text="", font=ff(8), text_color=A["text2"])
        self.sts.pack(pady=(4, 2))

        # 缩放把手
        ctk.CTkLabel(self, text="◢", font=ff(8), text_color=A["border"]
                    ).place(relx=1.0, rely=1.0, x=-8, y=-8, anchor="se")

    # ─── 动画 ─────────────────────────────────────────────
    def _anim(self):
        self._pa += 0.04
        r = int(120 + 80*(math.sin(self._pa)+1)/2)
        g = int(200 + 40*(math.sin(self._pa+1.5)+1)/2)
        b = int(230 + 25*(math.sin(self._pa-1)+1)/2)
        self.tl.configure(text_color=f"#{min(255,r):02x}{min(255,g):02x}{min(255,b):02x}")
        self._aid = self.after(40, self._anim)

    # ─── 控制 ─────────────────────────────────────────────
    def _start(self):
        self._stop()
        self._tid = self.after(self.cfg.interval * 1000, self._tick)
    def _stop(self):
        if self._tid: self.after_cancel(self._tid); self._tid = None
    def _tick(self):
        if not self._busy: self._refresh()
        self._tid = self.after(self.cfg.interval * 1000, self._tick)

    # ─── 数据 ─────────────────────────────────────────────
    def _refresh(self):
        if self._busy or not self.cfg.key: return
        self._busy = True
        self._sts("刷新中...")
        def _f():
            ok, bal = self.api.balance()
            self.after(0, lambda: self._on_bal(ok, bal))
        threading.Thread(target=_f, daemon=True).start()

    def _on_bal(self, ok, bal):
        self._busy = False
        if ok and bal:
            sym = "¥" if bal.currency == "CNY" else "$"
            self.bal_v.configure(text=f"{sym}{bal.total:.2f}")
            self.bal_s.configure(
                text=f"赠{sym}{bal.granted:.2f} · 充{sym}{bal.topped:.2f}")

            prev = self.cfg.last_bal
            if prev and prev["total"] > bal.total:
                delta = prev["total"] - bal.total
                if delta > 0.0001:
                    est = int(delta / 2.0 * 1_000_000)
                    self.cfg.add_c(date.today().isoformat(), delta, est)
            self.cfg.last_bal = bal.total

            if not self._usage: self._fetch_usage()
            self._update_today()
            ts = datetime.now().strftime("%H:%M:%S")
            self._sts(f"✓ {ts}")
        else:
            self._sts("✗ 余额获取失败")

    def _fetch_usage(self):
        end = date.today(); start = end - timedelta(29)
        def _f():
            ok, recs = self.api.usage(start, end)
            if ok and recs: self.after(0, lambda: self._merge(recs))
        threading.Thread(target=_f, daemon=True).start()

    def _merge(self, recs):
        days = {}
        for item in recs:
            try: d = date.fromisoformat(item.get("timestamp", "")[:10])
            except: d = date.today()
            if d not in days: days[d] = UsageDay(date=d)
            ud = days[d]
            m = item.get("model", "unknown")
            pt = int(item.get("prompt_tokens", 0))
            ct = int(item.get("completion_tokens", 0))
            ud.prompt_tokens += pt; ud.completion_tokens += ct
            ud.requests += 1
            p = self.cfg.pricing(m)
            ud.cost += (pt/1_000_000)*p["input"] + (ct/1_000_000)*p["output"]
        self._usage = sorted(days.values(), key=lambda x: x.date)
        self.cfg.save_usage(self._usage)
        self._calc_cache()
        self._update_today()

    def _update_today(self):
        td = date.today().isoformat()
        c = self.cfg.get_c().get(td)
        ud = None
        today = date.today()
        for u in self._usage:
            if u.date == today: ud = u; break

        if ud and ud.tokens > 0:
            self.tok_v.configure(text=f"{self._fmt(ud.tokens)} Token")
            self.tok_s.configure(
                text=f"¥{ud.cost:.2f} · "
                     f"入{self._fmt(ud.prompt_tokens)} "
                     f"出{self._fmt(ud.completion_tokens)}")
            self.req_v.configure(text=str(ud.requests))
            self.req_s.configure(text="今日请求 (官方)")
        elif c and c["tokens"] > 0:
            self.tok_v.configure(text=f"{self._fmt(c['tokens'])} Token")
            self.tok_s.configure(text=f"¥{c['cost']:.2f} · {c['events']}次 (估)")
            self.req_v.configure(text=str(c["events"]))
            self.req_s.configure(text="今日请求 (余额追踪)")
        else:
            self.tok_v.configure(text="—")
            self.tok_s.configure(text="今日 Token"); self.req_v.configure(text="—")

    def _calc_cache(self):
        tp = sum(d.prompt_tokens for d in self._usage)
        tc = sum(d.completion_tokens for d in self._usage)
        if tp == 0:
            self.cache_v.configure(text="N/A")
            self.cache_s.configure(text="缓存命中率 (暂无数据)"); return
        pr = self.cfg.pricing("deepseek-chat")
        no_cache = (tp/1_000_000)*pr["input"] + (tc/1_000_000)*pr["output"]
        actual = sum(d.cost for d in self._usage)
        rate = 0.0
        if actual < no_cache * 0.99:
            saved = no_cache - actual
            diff = pr["input"] - pr["cache_hit"]
            if diff > 0:
                rate = min(saved / diff * 1_000_000 / tp * 100, 99.9)
        self._cache_rate = rate
        self.cache_v.configure(text=f"{rate:.1f}%")
        self.cache_s.configure(
            text=f"近30天 {self._fmt(tp+tc)} Token · "
                 f"入{self._fmt(tp)} 出{self._fmt(tc)}")

    def _sts(self, t): self.sts.configure(text=t)

    # ─── 设置 ─────────────────────────────────────────────
    def _settings(self):
        dlg = ctk.CTkToplevel(self); dlg.title("设置"); dlg.geometry("420x470")
        dlg.configure(fg_color=A["bg"]); dlg.transient(self); dlg.grab_set()

        ctk.CTkLabel(dlg, text="API Key", font=ff(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(12, 2), anchor="w")
        kf = ctk.CTkFrame(dlg, fg_color="transparent")
        kf.pack(fill="x", padx=16)
        kv = ctk.StringVar(value=self.cfg.key)
        ctk.CTkEntry(kf, width=260, height=28, placeholder_text="sk-...",
                    show="●", textvariable=kv, font=ff(10)).pack(side="left", padx=(0, 4))
        ctk.CTkButton(kf, text="保存", width=50, height=28, font=ff(10),
                     command=lambda: self._save_test(kv, dlg)).pack(side="left", padx=2)
        ctk.CTkButton(kf, text="测试", width=50, height=28,
                     fg_color=A["green"], hover_color="#00c853",
                     text_color="#000", font=ff(10),
                     command=lambda: self._test_key(kv)).pack(side="left", padx=2)
        self._sst = ctk.CTkLabel(dlg, text="", font=ff(9), text_color=A["text2"])
        self._sst.pack(padx=16, pady=(2, 8), anchor="w")

        ctk.CTkLabel(dlg, text="刷新间隔", font=ff(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(4, 2), anchor="w")
        inf = ctk.CTkFrame(dlg, fg_color="transparent")
        inf.pack(fill="x", padx=16, pady=(0, 8))
        iv = ctk.StringVar(value=str(self.cfg.interval))
        ctk.CTkComboBox(inf, width=70, height=26,
                       values=["15","30","60","120","300"],
                       variable=iv, font=ff(10)).pack(side="left", padx=(0, 8))
        ctk.CTkLabel(inf, text="秒", text_color=A["text2"], font=ff(10)).pack(side="left")
        ctk.CTkButton(inf, text="应用", width=50, height=26, font=ff(10),
                     command=lambda: self._apply_int(iv)).pack(side="left", padx=(12, 0))

        ctk.CTkLabel(dlg, text="模型定价 (元/百万Token)", font=ff(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(8, 2), anchor="w")
        def _fetch():
            fb2.configure(state="disabled", text="获取中...")
            def _f():
                try:
                    r = self.api.s.get(PRICING_URL, timeout=15,
                        headers={"User-Agent": "Mozilla/5.0"})
                    if r.status_code == 200:
                        r.encoding = "utf-8"
                        idx = r.text.find("deepseek-v4-flash")
                        if idx > 0:
                            chunk = r.text[idx:idx+3000]
                            import re
                            prices = re.findall(r'([\d.]+)\s*元',
                                              re.sub(r'<del>.*?</del>', '', chunk))
                            if len(prices) >= 6:
                                pr = {
                                    "deepseek-v4-flash": {"input": float(prices[2]),
                                        "output": float(prices[4]),
                                        "cache_hit": float(prices[0])},
                                    "deepseek-v4-pro": {"input": float(prices[3]),
                                        "output": float(prices[5]),
                                        "cache_hit": float(prices[1])},
                                }
                                self.after(0, lambda: self._apply_pr(pr))
                except: pass
                self.after(0, lambda: fb2.configure(
                    state="normal", text="🌐 获取官方定价"))
            threading.Thread(target=_f, daemon=True).start()

        fb2 = ctk.CTkButton(dlg, text="🌐 获取官方定价", height=24,
                           command=_fetch, font=ff(10))
        fb2.pack(pady=(0, 4))

        scroll = ctk.CTkScrollableFrame(dlg, fg_color=A["card"],
                                         corner_radius=8, height=150)
        scroll.pack(fill="x", padx=16, pady=2)

        hdr = ctk.CTkFrame(scroll, fg_color="transparent")
        hdr.pack(fill="x")
        for t in ["模型", "输入", "输出", "缓存"]:
            ctk.CTkLabel(hdr, text=t, font=ff(9, "bold"),
                        text_color=A["text2"]).pack(side="left", padx=6, expand=True)

        self._pe = {}
        ct = self.cfg.d.get("pricing", {})
        for mn in list(DEFAULT_PRICING.keys()):
            if mn == "default": continue
            row = ctk.CTkFrame(scroll, fg_color="transparent")
            row.pack(fill="x", pady=1)
            nm = mn.replace("deepseek-", "")
            ctk.CTkLabel(row, text=nm, font=ff(9), text_color=A["text"]).pack(
                side="left", padx=4, expand=True)
            i2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["input"]))
            o2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["output"]))
            ch2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn]).get(
                "cache_hit", DEFAULT_PRICING[mn].get("cache_hit", 0.02))))
            self._pe[mn] = (i2, o2, ch2)
            ctk.CTkEntry(row, width=50, height=20, textvariable=i2,
                        font=ff(9)).pack(side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=20, textvariable=o2,
                        font=ff(9)).pack(side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=20, textvariable=ch2,
                        font=ff(9)).pack(side="left", padx=2, expand=True)

        bf = ctk.CTkFrame(dlg, fg_color="transparent")
        bf.pack(fill="x", padx=16, pady=(6, 10))
        def _reset():
            for mn, (iv2, ov2, cv2) in self._pe.items():
                if mn in DEFAULT_PRICING:
                    iv2.set(str(DEFAULT_PRICING[mn]["input"]))
                    ov2.set(str(DEFAULT_PRICING[mn]["output"]))
                    cv2.set(str(DEFAULT_PRICING[mn].get("cache_hit", 0.02)))
        ctk.CTkButton(bf, text="默认", width=50, command=_reset,
                     font=ff(10)).pack(side="left", padx=2)
        ctk.CTkButton(bf, text="保存定价", width=70, fg_color=A["cyan"],
                     command=lambda: self._save_pr(dlg),
                     font=ff(10)).pack(side="right", padx=2)

    def _save_test(self, kv, dlg):
        k = kv.get().strip()
        if not k: return
        self.cfg.key = k; self.api.set_key(k)
        self._sst.configure(text="已保存，测试中...", text_color=A["text2"])
        def _t():
            ok, bal = self.api.balance()
            if ok:
                self.after(0, lambda: self._sst.configure(
                    text="✓ 连接成功", text_color=A["green"]))
                self.after(0, lambda: self._on_bal(True, bal))
                self._start()
            else:
                self.after(0, lambda: self._sst.configure(
                    text="✗ 连接失败", text_color=A["pink"]))
        threading.Thread(target=_t, daemon=True).start()

    def _test_key(self, kv):
        k = kv.get().strip()
        if not k: return
        self.api.set_key(k); self._sst.configure(text="测试中...", text_color=A["text2"])
        def _t():
            ok, _ = self.api.balance()
            self.after(0, lambda: self._sst.configure(
                text="✓ 连接成功" if ok else "✗ 连接失败",
                text_color=A["green"] if ok else A["pink"]))
        threading.Thread(target=_t, daemon=True).start()

    def _apply_int(self, iv):
        try:
            self.cfg.interval = int(iv.get())
            self._stop(); self._start()
            self._sst.configure(text="✓ 已应用", text_color=A["green"])
        except: pass

    def _apply_pr(self, pr):
        for mn, (iv, ov, cv) in self._pe.items():
            if mn in pr:
                iv.set(str(pr[mn]["input"])); ov.set(str(pr[mn]["output"]))
                cv.set(str(pr[mn].get("cache_hit", 0.02)))

    def _save_pr(self, dlg):
        np = {}
        for mn, (iv, ov, cv) in self._pe.items():
            try:
                i = float(iv.get()); o = float(ov.get()); ch = float(cv.get())
                if i >= 0 and o >= 0 and ch >= 0:
                    np[mn] = {"input": i, "output": o, "cache_hit": ch}
            except: pass
        self.cfg.d["pricing"] = np; self.cfg.save()
        self._sst.configure(text="✓ 定价已保存", text_color=A["green"])

    @staticmethod
    def _fmt(n):
        if n >= 1_000_000: return f"{n/1_000_000:.2f}M"
        if n >= 1_000: return f"{n/1_000:.1f}K"
        return str(n)


def main():
    App().mainloop()

if __name__ == "__main__":
    main()
