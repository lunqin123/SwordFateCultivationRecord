#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DeepSeek · 悬浮用量看板
实时余额 · Token消耗 · 缓存命中率
"""

import json
import math
import os
import re
import sys
import threading
from collections import defaultdict
from dataclasses import dataclass, field
from datetime import datetime, timedelta, date
from typing import Optional

import requests
import customtkinter as ctk

# ─── 配置 ─────────────────────────────────────────────────
if getattr(sys, 'frozen', False):
    CDIR = os.path.dirname(os.path.abspath(sys.executable))
else:
    CDIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_FILE = os.path.join(CDIR, ".deepseek_config.json")

BALANCE_API = "https://api.deepseek.com/user/balance"
USAGE_API   = "https://api.deepseek.com/v1/usage"
PRICING_URL = "https://api-docs.deepseek.com/zh-cn/quick_start/pricing"

# 默认定价 (元/百万Token)
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

def f(s=13, w="normal"): return ctk.CTkFont(family=F, size=s, weight=w)

C = {
    "bg":      "#080910",
    "card":    "#111322",
    "border":  "#1c1f3a",
    "cyan":    "#00e5ff",
    "green":   "#00e676",
    "pink":    "#ff4081",
    "purple":  "#b388ff",
    "text":    "#e0e0e0",
    "text2":   "#78909c",
}


# ═══════════════════════════════════════════════════════════
#  数据模型
# ═══════════════════════════════════════════════════════════
@dataclass
class Balance:
    total: float = 0.0
    granted: float = 0.0
    topped: float = 0.0
    currency: str = "CNY"


@dataclass
class UsageDay:
    date: date
    prompt_tokens: int = 0
    completion_tokens: int = 0
    cost: float = 0.0
    requests: int = 0

    @property
    def total_tokens(self): return self.prompt_tokens + self.completion_tokens


# ═══════════════════════════════════════════════════════════
#  配置
# ═══════════════════════════════════════════════════════════
class Config:
    def __init__(self):
        self.d = self._load()

    def _load(self):
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                    return json.load(f)
            except: pass
        return {"api_key": "", "pricing": {}, "interval": 30,
                "consumption": {}, "last_balance": None, "usage_cache": []}

    def save(self):
        with open(CONFIG_FILE, "w", encoding="utf-8") as f:
            json.dump(self.d, f, indent=2, ensure_ascii=False)

    @property
    def key(self): return self.d.get("api_key", "")
    @key.setter
    def key(self, v): self.d["api_key"] = v.strip(); self.save()

    @property
    def interval(self): return self.d.get("interval", 30)
    @interval.setter
    def interval(self, v): self.d["interval"] = max(10, min(3600, v)); self.save()

    def pricing(self, model):
        c = self.d.get("pricing", {})
        return c.get(model) or DEFAULT_PRICING.get(model, DEFAULT_PRICING["default"])

    def get_consume(self): return self.d.get("consumption", {})
    def add_consume(self, day, cost, tokens):
        cd = self.d.setdefault("consumption", {})
        cd.setdefault(day, {"cost": 0.0, "tokens": 0, "events": 0})
        cd[day]["cost"] += cost
        cd[day]["tokens"] += tokens
        cd[day]["events"] += 1
        self.d["consumption"] = dict(sorted(cd.items())[-90:])
        self.save()

    @property
    def last_bal(self): return self.d.get("last_balance")
    @last_bal.setter
    def last_bal(self, total):
        self.d["last_balance"] = {"total": total, "time": datetime.now().isoformat()}
        self.save()

    def load_usage(self):
        raw = self.d.get("usage_cache", [])
        out = []
        for x in raw:
            d = UsageDay(date=date.fromisoformat(x["date"]),
                         prompt_tokens=x["p"], completion_tokens=x["c"],
                         cost=x["cost"], requests=x["n"])
            out.append(d)
        return out

    def save_usage(self, data):
        self.d["usage_cache"] = [
            {"date": d.date.isoformat(), "p": d.prompt_tokens, "c": d.completion_tokens,
             "cost": d.cost, "n": d.requests} for d in data
        ]
        self.save()


# ═══════════════════════════════════════════════════════════
#  API
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
#  主应用
# ═══════════════════════════════════════════════════════════
class App(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.cfg = Config()
        self.api = API(self.cfg.key)
        self._tid = None
        self._busy = False
        self._bal = None
        self._cache_rate = 0.0
        self._drx = 0; self._dry = 0
        self._pa = 0.0
        self._usage = self.cfg.load_usage()

        # 悬浮窗
        self.overrideredirect(True)
        self.attributes("-topmost", True)
        self.configure(fg_color=C["bg"])
        self.geometry(f"340x320+{self.winfo_screenwidth()-360}+{20}")
        self.minsize(280, 280)

        self.bind("<Button-1>", self._ds)
        self.bind("<B1-Motion>", self._dm)
        self.bind("<Button-3>", self._rc)

        self._ui()
        self._anim()
        if self.cfg.key:
            self.after(500, self._refresh)
            self._start()

    def _ds(self, e): self._drx = e.x; self._dry = e.y
    def _dm(self, e): self.geometry(f"+{e.x_root-self._drx}+{e.y_root-self._dry}")

    def _rc(self, e):
        m = ctk.CTkToplevel(self); m.overrideredirect(True)
        m.configure(fg_color=C["card"])
        m.geometry(f"100x60+{e.x_root}+{e.y_root}")
        ctk.CTkButton(m, text="关闭", font=f(11), fg_color="transparent",
                     command=self._close).pack(fill="x", padx=2, pady=2)
        ctk.CTkButton(m, text="⚙ 定价", font=f(11), fg_color="transparent",
                     command=self._settings).pack(fill="x", padx=2, pady=2)
        m.focus_set(); m.bind("<FocusOut>", lambda _: m.destroy())

    # ─── UI ───────────────────────────────────────────────
    def _ui(self):
        # 顶栏
        top = ctk.CTkFrame(self, fg_color="transparent", height=36)
        top.pack(fill="x", padx=12, pady=(10, 0)); top.pack_propagate(False)

        self.tl = ctk.CTkLabel(top, text="⚡ DeepSeek", font=f(20, "bold"), text_color=C["cyan"])
        self.tl.pack(side="left")
        ctk.CTkButton(top, text="✕", width=24, height=24, font=f(14),
                     fg_color="transparent", hover_color="#331111",
                     text_color=C["text2"], command=self._close).pack(side="right")

        # 余额
        self.bal_val = ctk.CTkLabel(self, text="—", font=f(36, "bold"), text_color=C["text"])
        self.bal_val.pack(pady=(14, 0))
        self.bal_sub = ctk.CTkLabel(self, text="总余额", font=f(11), text_color=C["text2"])
        self.bal_sub.pack()

        # 分隔线
        ctk.CTkFrame(self, height=1, fg_color=C["border"]).pack(fill="x", padx=20, pady=(8, 4))

        # Token + 缓存命中率
        info = ctk.CTkFrame(self, fg_color="transparent")
        info.pack(fill="x", padx=20, pady=4)

        self.tok_val = ctk.CTkLabel(info, text="—", font=f(18, "bold"), text_color=C["green"])
        self.tok_val.pack()
        self.tok_sub = ctk.CTkLabel(info, text="今日 Token 消耗", font=f(10), text_color=C["text2"])
        self.tok_sub.pack()

        ctk.CTkFrame(self, height=1, fg_color=C["border"]).pack(fill="x", padx=40, pady=(2, 4))

        self.cache_val = ctk.CTkLabel(info, text="—", font=f(18, "bold"), text_color=C["purple"])
        self.cache_val.pack()
        self.cache_sub = ctk.CTkLabel(info, text="缓存命中率", font=f(10), text_color=C["text2"])
        self.cache_sub.pack()

        # 底栏
        bottom = ctk.CTkFrame(self, fg_color="transparent")
        bottom.pack(fill="x", padx=12, pady=(8, 6))

        self.key_var = ctk.StringVar(value=self.cfg.key)
        ctk.CTkEntry(bottom, width=200, height=26, placeholder_text="sk-...",
                    show="●", textvariable=self.key_var, font=f(10)).pack(side="left", padx=(0, 4))
        ctk.CTkButton(bottom, text="保存", width=44, height=26,
                     command=self._save_key, font=f(10)).pack(side="left", padx=2)
        ctk.CTkButton(bottom, text="测试", width=44, height=26,
                     fg_color=C["green"], hover_color="#00c853", text_color="#000",
                     command=self._test, font=f(10)).pack(side="left", padx=2)

        self.sts = ctk.CTkLabel(self, text="", font=f(9), text_color=C["text2"])
        self.sts.pack(pady=(0, 2))

        ctrl = ctk.CTkFrame(self, fg_color="transparent")
        ctrl.pack(fill="x", padx=12, pady=(0, 6))

        self.auto_var = ctk.BooleanVar(value=True)
        ctk.CTkCheckBox(ctrl, text="自动", variable=self.auto_var,
                       command=self._tgl_auto, font=f(10)).pack(side="left", padx=2)

        self.int_var = ctk.StringVar(value=str(self.cfg.interval))
        ctk.CTkComboBox(ctrl, width=50, height=22, values=["15","30","60","120"],
                       variable=self.int_var, command=self._chg_int, font=f(10)).pack(side="left", padx=2)

        ctk.CTkButton(ctrl, text="⟳", width=28, height=22, font=f(12),
                     command=self._refresh).pack(side="left", padx=(4, 0))
        ctk.CTkButton(ctrl, text="⚙", width=28, height=22, font=f(10),
                     command=self._settings).pack(side="right", padx=2)

    # ─── 动画 ─────────────────────────────────────────────
    def _anim(self):
        self._pa += 0.04
        # 标题呼吸
        r = int(120 + 80*(math.sin(self._pa)+1)/2)
        g = int(200 + 40*(math.sin(self._pa+1.5)+1)/2)
        b = int(230 + 25*(math.sin(self._pa-1)+1)/2)
        self.tl.configure(text_color=f"#{min(255,r):02x}{min(255,g):02x}{min(255,b):02x}")
        self._aid = self.after(40, self._anim)

    # ─── 事件 ─────────────────────────────────────────────
    def _save_key(self):
        k = self.key_var.get().strip()
        if k:
            self.cfg.key = k; self.api.set_key(k)
            self._sts("已保存 ✓"); self._refresh()

    def _test(self):
        k = self.key_var.get().strip()
        if not k: self._sts("先输入Key"); return
        self.api.set_key(k)
        self._sts("测试中...")
        def _t():
            ok, bal = self.api.balance()
            if ok:
                self.cfg.key = k
                self.after(0, lambda: self._sts("连接成功 ✓"))
                self.after(0, lambda: self._on_bal(True, bal))
            else: self.after(0, lambda: self._sts("失败"))
        threading.Thread(target=_t, daemon=True).start()

    def _sts(self, m): self.sts.configure(text=m)

    def _tgl_auto(self):
        if self.auto_var.get(): self._start()
        else: self._stop()

    def _chg_int(self, _=None):
        try:
            self.cfg.interval = int(self.int_var.get())
            if self.auto_var.get(): self._stop(); self._start()
        except: pass

    def _close(self):
        self._stop()
        if hasattr(self, '_aid'): self.after_cancel(self._aid)
        self.destroy()

    # ─── 自动刷新 ─────────────────────────────────────────
    def _start(self):
        self._stop()
        self._tid = self.after(self.cfg.interval * 1000, self._tick)

    def _stop(self):
        if self._tid: self.after_cancel(self._tid); self._tid = None

    def _tick(self):
        if self.auto_var.get() and not self._busy: self._refresh()
        if self.auto_var.get(): self._tid = self.after(self.cfg.interval * 1000, self._tick)

    # ─── 数据 ─────────────────────────────────────────────
    def _refresh(self):
        if self._busy: return
        self._busy = True
        def _f():
            ok, bal = self.api.balance()
            self.after(0, lambda: self._on_bal(ok, bal))
        threading.Thread(target=_f, daemon=True).start()

    def _on_bal(self, ok, bal):
        self._busy = False
        if ok and bal:
            old = self._bal
            self._bal = bal

            # 更新余额
            sym = "¥" if bal.currency == "CNY" else "$"
            self.bal_val.configure(text=f"{sym}{bal.total:.2f}")
            self.bal_sub.configure(text=f"赠{sym}{bal.granted:.2f} · 充{sym}{bal.topped:.2f}")

            # 检测消费
            prev = self.cfg.last_bal
            if prev and prev["total"] > bal.total:
                delta = prev["total"] - bal.total
                if delta > 0.0001:
                    est = int(delta / 2.0 * 1_000_000)
                    self.cfg.add_consume(date.today().isoformat(), delta, est)
            self.cfg.last_bal = bal.total

            # 今日统计
            td = date.today().isoformat()
            c = self.cfg.get_consume().get(td)
            if c:
                self.tok_val.configure(text=f"{self._fmt(c['tokens'])} Token")
                self.tok_sub.configure(text=f"¥{c['cost']:.2f} · {c['events']}次")

            # 首次拉官方用量
            if not self._usage:
                self._refresh_usage()

    def _refresh_usage(self):
        k = self.key_var.get().strip()
        if not k: return
        end = date.today(); start = end - timedelta(29)
        def _f():
            self.api.set_key(k)
            ok, recs = self.api.usage(start, end)
            if ok and recs:
                self.after(0, lambda: self._merge(recs))
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

        # 算缓存命中率
        self._calc_cache_rate()
        self._update_today()

    def _calc_cache_rate(self):
        """根据实际费用反推缓存命中率"""
        total_pt = sum(d.prompt_tokens for d in self._usage)
        total_ct = sum(d.completion_tokens for d in self._usage)
        if total_pt == 0:
            self._cache_rate = 0
            self.cache_val.configure(text="N/A")
            self.cache_sub.configure(text="暂无请求数据")
            return

        # 用 V4-Flash (默认模型) 价格估算
        pr = self.cfg.pricing("deepseek-chat")
        miss_price = pr["input"]    # ¥1.0/M
        hit_price = pr["cache_hit"] # ¥0.02/M
        output_price = pr["output"] # ¥2.0/M

        # 若无缓存命中，总费用应是多少
        no_cache_cost = (total_pt/1_000_000)*miss_price + (total_ct/1_000_000)*output_price
        actual_cost = sum(d.cost for d in self._usage)

        if actual_cost >= no_cache_cost * 0.99:
            rate = 0.0
        else:
            # 节省来自缓存命中token
            saved = no_cache_cost - actual_cost
            if miss_price - hit_price > 0:
                cache_hit_tokens = saved / (miss_price - hit_price) * 1_000_000
                rate = min(cache_hit_tokens / total_pt * 100, 99.9)

        self._cache_rate = rate
        self.cache_val.configure(text=f"{rate:.1f}%")
        self.cache_sub.configure(
            text=f"预估缓存命中 · 总 {self._fmt(total_pt+total_ct)} Token")

    def _update_today(self):
        td = date.today().isoformat()
        c = self.cfg.get_consume().get(td)
        if c:
            self.tok_val.configure(text=f"{self._fmt(c['tokens'])} Token")
            self.tok_sub.configure(text=f"¥{c['cost']:.2f} · {c['events']}次")

    # ─── 定价设置 ─────────────────────────────────────────
    def _settings(self):
        dlg = ctk.CTkToplevel(self); dlg.title("定价")
        dlg.geometry("400x320"); dlg.configure(fg_color=C["bg"])
        dlg.transient(self); dlg.grab_set()

        ctk.CTkLabel(dlg, text="模型定价 (元/百万Token)", font=f(13, "bold"),
                    text_color=C["text"]).pack(pady=(10, 2))

        def _fetch():
            btn2.configure(state="disabled", text="获取中...")
            def _f():
                try:
                    r = self.api.s.get(PRICING_URL, timeout=15,
                        headers={"User-Agent": "Mozilla/5.0"})
                    if r.status_code == 200:
                        r.encoding = "utf-8"
                        idx = r.text.find("deepseek-v4-flash")
                        if idx > 0:
                            import re
                            chunk = r.text[idx:idx+3000]
                            cleaned = re.sub(r'<del>.*?</del>', '', chunk)
                            prices = re.findall(r'([\d.]+)\s*元', cleaned)
                            if len(prices) >= 6:
                                pr = {
                                    "deepseek-v4-flash": {"input": float(prices[2]), "output": float(prices[4]), "cache_hit": float(prices[0])},
                                    "deepseek-v4-pro": {"input": float(prices[3]), "output": float(prices[5]), "cache_hit": float(prices[1])},
                                }
                                self.after(0, lambda: self._apply_pr(pr))
                except: pass
                self.after(0, lambda: btn2.configure(state="normal", text="🌐 获取官方定价"))
            threading.Thread(target=_f, daemon=True).start()

        btn2 = ctk.CTkButton(dlg, text="🌐 获取官方定价", height=26, command=_fetch, font=f(10))
        btn2.pack(pady=4)

        scroll = ctk.CTkScrollableFrame(dlg, fg_color=C["card"], corner_radius=8, height=180)
        scroll.pack(fill="x", padx=14, pady=4)

        hdr = ctk.CTkFrame(scroll, fg_color="transparent")
        hdr.pack(fill="x")
        for t in ["模型", "输入", "输出", "缓存"]:
            ctk.CTkLabel(hdr, text=t, font=f(9, "bold"), text_color=C["text2"]).pack(
                side="left", padx=6, expand=True)

        self._pe = {}
        ct = self.cfg.d.get("pricing", {})
        for mn in list(DEFAULT_PRICING.keys()):
            if mn == "default": continue
            row = ctk.CTkFrame(scroll, fg_color="transparent")
            row.pack(fill="x", pady=1)
            nm = mn.replace("deepseek-", "")
            ctk.CTkLabel(row, text=nm, font=f(9), text_color=C["text"]).pack(
                side="left", padx=4, expand=True)
            iv = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["input"]))
            ov = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn])["output"]))
            cv = ctk.StringVar(value=str(ct.get(mn, DEFAULT_PRICING[mn]).get("cache_hit",
                                     DEFAULT_PRICING[mn].get("cache_hit", 0.02))))
            self._pe[mn] = (iv, ov, cv)
            ctk.CTkEntry(row, width=50, height=22, textvariable=iv, font=f(9)).pack(
                side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=22, textvariable=ov, font=f(9)).pack(
                side="left", padx=2, expand=True)
            ctk.CTkEntry(row, width=50, height=22, textvariable=cv, font=f(9)).pack(
                side="left", padx=2, expand=True)

        bf = ctk.CTkFrame(dlg, fg_color="transparent")
        bf.pack(fill="x", padx=14, pady=6)

        def _reset():
            for mn, (iv, ov, cv) in self._pe.items():
                if mn in DEFAULT_PRICING:
                    iv.set(str(DEFAULT_PRICING[mn]["input"]))
                    ov.set(str(DEFAULT_PRICING[mn]["output"]))
                    cv.set(str(DEFAULT_PRICING[mn].get("cache_hit", 0.02)))

        ctk.CTkButton(bf, text="默认", width=50, command=_reset, font=f(10)).pack(side="left", padx=2)
        ctk.CTkButton(bf, text="保存", width=50, fg_color=C["cyan"],
                     command=lambda: self._save_pr(dlg), font=f(10)).pack(side="right", padx=2)

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
        dlg.destroy()

    @staticmethod
    def _fmt(n):
        if n >= 1_000_000: return f"{n/1_000_000:.2f}M"
        if n >= 1_000: return f"{n/1_000:.1f}K"
        return str(n)


def main():
    App().mainloop()


if __name__ == "__main__":
    main()
