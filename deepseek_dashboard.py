#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DeepSeek · 悬浮用量看板
亚克力半透明 · 实时余额/Token/缓存命中率
"""

import json, math, os, re, sys, threading
from collections import defaultdict
from dataclasses import dataclass, field
from datetime import datetime, timedelta, date
from typing import Optional

import requests
import customtkinter as ctk

# ─── 配置路径 ─────────────────────────────────────────────
if getattr(sys, 'frozen', False):
    CDIR = os.path.dirname(os.path.abspath(sys.executable))
else:
    CDIR = os.path.dirname(os.path.abspath(__file__))
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

# ─── 样式 ──────────────────────────────────────────────────
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("blue")
F = "Microsoft YaHei UI"
def _f(s=12, w="normal"): return ctk.CTkFont(family=F, size=s, weight=w)

# 亚克力半透明配色
A = {  # alpha-prefixed for "acrylic"
    "bg":      "#0c0e1a",
    "card":    "#151830",
    "border":  "#252a50",
    "cyan":    "#00e5ff",
    "green":   "#00e676",
    "pink":    "#ff4081",
    "purple":  "#b388ff",
    "text":    "#e8e8e8",
    "text2":   "#6a7a8a",
}
WINDOW_ALPHA = 0.88  # 窗口整体透明度


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
                "consumption": {}, "last_balance": None, "usage_cache": []}
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


# ═══════════════════════════════════════════════════════════
class API:
    def __init__(self, key=""):
        self.s = requests.Session()
        if key: self.s.headers.update({"Authorization": f"Bearer {key}", "Accept": "application/json"})
    def set_key(self, k):
        self.s.headers.update({"Authorization": f"Bearer {k}", "Accept": "application/json"})
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
                          params={"start_date": start.isoformat(), "end_date": end.isoformat()})
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
        self._cache_rate = 0.0; self._drx = self._dry = 0; self._pa = 0.0
        self._usage = self.cfg.load_usage()
        self._connected = False

        # 亚克力悬浮窗
        self.overrideredirect(True)
        self.attributes("-topmost", True)
        self.attributes("-alpha", WINDOW_ALPHA)
        self.configure(fg_color=A["bg"])
        self.geometry(f"240x190+{self.winfo_screenwidth()-260}+{30}")
        self.minsize(200, 160)

        self.bind("<Button-1>", self._ds); self.bind("<B1-Motion>", self._dm)
        self.bind("<Button-3>", self._rc)

        self._build()
        self._anim()
        if self.cfg.key:
            self.after(600, self._refresh)
            self._start()

    # ─── 拖拽 ─────────────────────────────────────────────
    def _ds(self, e): self._drx = e.x; self._dry = e.y
    def _dm(self, e): self.geometry(f"+{e.x_root-self._drx}+{e.y_root-self._dry}")
    def _rc(self, e):
        m = ctk.CTkToplevel(self); m.overrideredirect(True)
        m.configure(fg_color=A["card"])
        m.geometry(f"100x55+{e.x_root}+{e.y_root}")
        ctk.CTkButton(m, text="关闭", font=_f(10), fg_color="transparent",
                     command=self._close).pack(fill="x", padx=2, pady=1)
        ctk.CTkButton(m, text="⚙ 设置", font=_f(10), fg_color="transparent",
                     command=self._settings).pack(fill="x", padx=2, pady=1)
        m.focus_set(); m.bind("<FocusOut>", lambda _: m.destroy())

    # ─── UI ───────────────────────────────────────────────
    def _build(self):
        # 标题行
        top = ctk.CTkFrame(self, fg_color="transparent", height=22)
        top.pack(fill="x", padx=10, pady=(6, 0)); top.pack_propagate(False)
        self.tl = ctk.CTkLabel(top, text="DeepSeek", font=_f(14, "bold"), text_color=A["cyan"])
        self.tl.pack(side="left")
        ctk.CTkButton(top, text="✕", width=18, height=18, font=_f(10),
                     fg_color="transparent", hover_color="#331111",
                     text_color=A["text2"], command=self._close).pack(side="right")

        # 余额
        self.bal_v = ctk.CTkLabel(self, text="—", font=_f(24, "bold"), text_color=A["text"])
        self.bal_v.pack(pady=(8, 0))
        self.bal_s = ctk.CTkLabel(self, text="总余额", font=_f(9), text_color=A["text2"])
        self.bal_s.pack()

        # 分隔
        ctk.CTkFrame(self, height=1, fg_color=A["border"]).pack(fill="x", padx=16, pady=3)

        # Token
        self.tok_v = ctk.CTkLabel(self, text="—", font=_f(14, "bold"), text_color=A["green"])
        self.tok_v.pack()
        self.tok_s = ctk.CTkLabel(self, text="今日 Token", font=_f(9), text_color=A["text2"])
        self.tok_s.pack()

        # 缓存
        self.cache_v = ctk.CTkLabel(self, text="—", font=_f(14, "bold"), text_color=A["purple"])
        self.cache_v.pack()
        self.cache_s = ctk.CTkLabel(self, text="缓存命中率", font=_f(9), text_color=A["text2"])
        self.cache_s.pack()

        # 底栏
        bot = ctk.CTkFrame(self, fg_color="transparent")
        bot.pack(fill="x", padx=10, pady=(3, 4))

        self.sts = ctk.CTkLabel(bot, text="", font=_f(8), text_color=A["text2"])
        self.sts.pack(side="left")

        ctk.CTkButton(bot, text="⟳", width=22, height=22, font=_f(10),
                     command=self._refresh).pack(side="right", padx=1)
        ctk.CTkButton(bot, text="⚙", width=22, height=22, font=_f(10),
                     command=self._settings).pack(side="right", padx=1)

    # ─── 动画 ─────────────────────────────────────────────
    def _anim(self):
        self._pa += 0.04
        r = int(120 + 80*(math.sin(self._pa)+1)/2)
        g = int(200 + 40*(math.sin(self._pa+1.5)+1)/2)
        b = int(230 + 25*(math.sin(self._pa-1)+1)/2)
        self.tl.configure(text_color=f"#{min(255,r):02x}{min(255,g):02x}{min(255,b):02x}")
        self._aid = self.after(40, self._anim)

    # ─── 控制 ─────────────────────────────────────────────
    def _close(self):
        self._stop()
        if hasattr(self, '_aid'): self.after_cancel(self._aid)
        self.destroy()

    def _start(self):
        self._stop()
        self._tid = self.after(self.cfg.interval * 1000, self._tick)
    def _stop(self):
        if self._tid: self.after_cancel(self._tid); self._tid = None
    def _tick(self):
        if not self._busy: self._refresh()
        self._tid = self.after(self.cfg.interval * 1000, self._tick)

    def _sts(self, m): self.sts.configure(text=m)

    # ─── 数据 ─────────────────────────────────────────────
    def _refresh(self):
        if self._busy or not self.cfg.key: return
        self._busy = True
        def _f():
            ok, bal = self.api.balance()
            self.after(0, lambda: self._on_bal(ok, bal))
        threading.Thread(target=_f, daemon=True).start()

    def _on_bal(self, ok, bal):
        self._busy = False
        if ok and bal:
            self._connected = True
            sym = "¥" if bal.currency == "CNY" else "$"
            self.bal_v.configure(text=f"{sym}{bal.total:.2f}")
            self.bal_s.configure(text=f"赠{sym}{bal.granted:.2f} · 充{sym}{bal.topped:.2f}")

            prev = self.cfg.last_bal
            if prev and prev["total"] > bal.total:
                delta = prev["total"] - bal.total
                if delta > 0.0001:
                    est = int(delta / 2.0 * 1_000_000)
                    self.cfg.add_c(date.today().isoformat(), delta, est)
            self.cfg.last_bal = bal.total

            td = date.today().isoformat()
            c = self.cfg.get_c().get(td)
            if c:
                self.tok_v.configure(text=f"{self._fmt(c['tokens'])} T")
                self.tok_s.configure(text=f"¥{c['cost']:.2f} · {c['events']}次")

            if not self._usage: self._fetch_usage()

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
            ud.prompt_tokens += pt; ud.completion_tokens += ct; ud.requests += 1
            p = self.cfg.pricing(m)
            ud.cost += (pt/1_000_000)*p["input"] + (ct/1_000_000)*p["output"]
        self._usage = sorted(days.values(), key=lambda x: x.date)
        self.cfg.save_usage(self._usage)
        self._calc_cache()

    def _calc_cache(self):
        tp = sum(d.prompt_tokens for d in self._usage)
        tc = sum(d.completion_tokens for d in self._usage)
        if tp == 0:
            self.cache_v.configure(text="N/A"); self.cache_s.configure(text="暂无数据"); return

        pr = self.cfg.pricing("deepseek-chat")
        no_cache = (tp/1_000_000)*pr["input"] + (tc/1_000_000)*pr["output"]
        actual = sum(d.cost for d in self._usage)
        if actual >= no_cache * 0.99:
            rate = 0.0
        else:
            saved = no_cache - actual
            diff = pr["input"] - pr["cache_hit"]
            if diff > 0:
                rate = min(saved / diff * 1_000_000 / tp * 100, 99.9)
            else: rate = 0.0
        self._cache_rate = rate
        self.cache_v.configure(text=f"{rate:.1f}%")
        self.cache_s.configure(text=f"近30天 {self._fmt(tp+tc)} Token")

    # ─── 设置对话框 ───────────────────────────────────────
    def _settings(self):
        dlg = ctk.CTkToplevel(self); dlg.title("设置"); dlg.geometry("400x440")
        dlg.configure(fg_color=A["bg"]); dlg.transient(self); dlg.grab_set()

        # ─── API Key ───────────────────────────────
        ctk.CTkLabel(dlg, text="API Key", font=_f(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(12, 2), anchor="w")

        key_frame = ctk.CTkFrame(dlg, fg_color="transparent")
        key_frame.pack(fill="x", padx=16)

        kv = ctk.StringVar(value=self.cfg.key)
        ctk.CTkEntry(key_frame, width=260, height=28, placeholder_text="sk-...",
                    show="●", textvariable=kv, font=_f(10)).pack(side="left", padx=(0, 4))
        ctk.CTkButton(key_frame, text="保存", width=50, height=28,
                     command=lambda: self._save_and_test(kv, dlg), font=_f(10)
                     ).pack(side="left", padx=2)
        ctk.CTkButton(key_frame, text="测试", width=50, height=28,
                     fg_color=A["green"], hover_color="#00c853",
                     text_color="#000", command=lambda: self._test_key(kv), font=_f(10)
                     ).pack(side="left", padx=2)

        self._set_sts = ctk.CTkLabel(dlg, text="", font=_f(9), text_color=A["text2"])
        self._set_sts.pack(padx=16, pady=(2, 8), anchor="w")

        # ─── 刷新间隔 ───────────────────────────────
        ctk.CTkLabel(dlg, text="刷新间隔", font=_f(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(4, 2), anchor="w")

        int_frame = ctk.CTkFrame(dlg, fg_color="transparent")
        int_frame.pack(fill="x", padx=16, pady=(0, 8))
        iv = ctk.StringVar(value=str(self.cfg.interval))
        ctk.CTkComboBox(int_frame, width=70, height=26, values=["15","30","60","120","300"],
                       variable=iv, font=_f(10)).pack(side="left", padx=(0, 8))
        ctk.CTkLabel(int_frame, text="秒", text_color=A["text2"], font=_f(10)).pack(side="left")
        ctk.CTkButton(int_frame, text="应用", width=50, height=26,
                     command=lambda: self._apply_interval(iv), font=_f(10)).pack(side="left", padx=(12, 0))

        # ─── 定价 ──────────────────────────────────
        ctk.CTkLabel(dlg, text="模型定价 (元/百万Token)", font=_f(13, "bold"),
                    text_color=A["text"]).pack(padx=16, pady=(8, 2), anchor="w")

        def _fetch():
            fb.configure(state="disabled", text="获取中...")
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
                            prices = re.findall(r'([\d.]+)\s*元', re.sub(r'<del>.*?</del>', '', chunk))
                            if len(prices) >= 6:
                                pr = {
                                    "deepseek-v4-flash": {"input": float(prices[2]), "output": float(prices[4]), "cache_hit": float(prices[0])},
                                    "deepseek-v4-pro": {"input": float(prices[3]), "output": float(prices[5]), "cache_hit": float(prices[1])},
                                }
                                self.after(0, lambda: self._apply_pr(pr))
                except: pass
                self.after(0, lambda: fb.configure(state="normal", text="🌐 获取官方定价"))
            threading.Thread(target=_f, daemon=True).start()

        fb = ctk.CTkButton(dlg, text="🌐 获取官方定价", height=24, command=_fetch, font=_f(10))
        fb.pack(pady=(0, 4))

        scroll = ctk.CTkScrollableFrame(dlg, fg_color=A["card"], corner_radius=8, height=140)
        scroll.pack(fill="x", padx=16, pady=2)

        hdr = ctk.CTkFrame(scroll, fg_color="transparent")
        hdr.pack(fill="x")
        for t in ["模型", "输入", "输出", "缓存"]:
            ctk.CTkLabel(hdr, text=t, font=_f(9, "bold"), text_color=A["text2"]).pack(
                side="left", padx=6, expand=True)

        self._pe = {}
        ct = self.cfg.d.get("pricing", {})
        for mn in list(DEFAULT_PRICING.keys()):
            if mn == "default": continue
            row = ctk.CTkFrame(scroll, fg_color="transparent")
            row.pack(fill="x", pady=1)
            nm = mn.replace("deepseek-", "")
            ctk.CTkLabel(row, text=nm, font=_f(9), text_color=A["text"]).pack(
                side="left", padx=4, expand=True)
            i2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["input"]))
            o2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["output"]))
            ch2 = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn]).get("cache_hit",
                                      DEFAULT_PRICING[mn].get("cache_hit", 0.02))))
            self._pe[mn] = (i2, o2, ch2)
            ctk.CTkEntry(row, width=50, height=20, textvariable=i2, font=_f(9)).pack(
                side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=20, textvariable=o2, font=_f(9)).pack(
                side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=20, textvariable=ch2, font=_f(9)).pack(
                side="left", padx=2, expand=True)

        bf = ctk.CTkFrame(dlg, fg_color="transparent")
        bf.pack(fill="x", padx=16, pady=(6, 10))
        def _reset():
            for mn, (iv2, ov2, cv2) in self._pe.items():
                if mn in DEFAULT_PRICING:
                    iv2.set(str(DEFAULT_PRICING[mn]["input"]))
                    ov2.set(str(DEFAULT_PRICING[mn]["output"]))
                    cv2.set(str(DEFAULT_PRICING[mn].get("cache_hit", 0.02)))
        ctk.CTkButton(bf, text="默认", width=50, command=_reset, font=_f(10)).pack(side="left", padx=2)
        ctk.CTkButton(bf, text="保存定价", width=70, fg_color=A["cyan"],
                     command=lambda: self._save_pr(dlg), font=_f(10)).pack(side="right", padx=2)

    def _save_and_test(self, kv, dlg):
        k = kv.get().strip()
        if not k: return
        self.cfg.key = k; self.api.set_key(k)
        self._set_sts.configure(text="已保存，正在测试...", text_color=A["text2"])
        def _t():
            ok, bal = self.api.balance()
            if ok:
                self.after(0, lambda: self._set_sts.configure(
                    text="✓ 连接成功", text_color=A["green"]))
                self.after(0, lambda: self._on_bal(True, bal))
                self._start()
            else:
                self.after(0, lambda: self._set_sts.configure(
                    text="✗ 连接失败", text_color=A["pink"]))
        threading.Thread(target=_t, daemon=True).start()

    def _test_key(self, kv):
        k = kv.get().strip()
        if not k: return
        self.api.set_key(k)
        self._set_sts.configure(text="测试中...", text_color=A["text2"])
        def _t():
            ok, _ = self.api.balance()
            if ok:
                self.after(0, lambda: self._set_sts.configure(
                    text="✓ 连接成功", text_color=A["green"]))
            else:
                self.after(0, lambda: self._set_sts.configure(
                    text="✗ 连接失败", text_color=A["pink"]))
        threading.Thread(target=_t, daemon=True).start()

    def _apply_interval(self, iv):
        try:
            self.cfg.interval = int(iv.get())
            self._stop(); self._start()
            self._set_sts.configure(text="✓ 已应用", text_color=A["green"])
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
                if i >= 0 and o >= 0 and ch >= 0: np[mn] = {"input": i, "output": o, "cache_hit": ch}
            except: pass
        self.cfg.d["pricing"] = np; self.cfg.save()
        self._set_sts.configure(text="✓ 定价已保存", text_color=A["green"])

    @staticmethod
    def _fmt(n):
        if n >= 1_000_000: return f"{n/1_000_000:.2f}M"
        if n >= 1_000: return f"{n/1_000:.1f}K"
        return str(n)


def main():
    App().mainloop()

if __name__ == "__main__":
    main()
