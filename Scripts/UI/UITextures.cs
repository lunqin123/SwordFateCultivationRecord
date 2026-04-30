namespace SwordFateCultivationRecord;

public static class UITextures
{
    public static ImageTexture? BgPaper;
    public static ImageTexture? Panel;
    public static ImageTexture? BtnNormal;
    public static ImageTexture? BtnHover;
    public static ImageTexture? BtnPressed;

    public static void Generate()
    {
        BgPaper = MakeTex(512, 512, GenBgPaper);
        Panel = MakeTex(256, 256, GenPanel);
        BtnNormal = MakeTex(192, 48, GenBtnNormal);
        BtnHover = MakeTex(192, 48, GenBtnHover);
        BtnPressed = MakeTex(192, 48, GenBtnPressed);

        // Also save so Godot editor caches them
        SaveTex(BgPaper, "res://Resources/Textures/bg_paper.png");
        SaveTex(Panel, "res://Resources/Textures/panel.png");
        SaveTex(BtnNormal, "res://Resources/Textures/btn_normal.png");
        SaveTex(BtnHover, "res://Resources/Textures/btn_hover.png");
        SaveTex(BtnPressed, "res://Resources/Textures/btn_pressed.png");
    }

    static ImageTexture MakeTex(int w, int h, Action<Image> fill)
    {
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        fill(img);
        return ImageTexture.CreateFromImage(img);
    }

    static void SaveTex(ImageTexture? tex, string path)
    {
        tex?.GetImage().SavePng(path);
    }

    // ===== Warm parchment paper background =====
    static void GenBgPaper(Image img)
    {
        int s = 512;
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            double n1 = Fbm(x, y, 0.005, 4, 42) * 28;
            double n2 = Fbm(x, y, 0.025, 3, 99) * 14;
            double n3 = Fbm(x, y, 0.07, 2, 73) * 8;
            double fiber = Math.Sin(y * 0.35 + Fbm(x, y, 0.08, 1, 37) * 4) * 5;

            int r = C(55 + (int)(n1 + n2 + n3 + fiber));
            int g = C(37 + (int)(n1 * 0.7 + n2 * 0.7 + n3 * 0.5 + fiber * 0.7));
            int b = C(24 + (int)(n1 * 0.4 + n2 * 0.4 + n3 * 0.3 + fiber * 0.4));

            double dx = (x - s / 2.0) / (s / 2.0);
            double dy = (y - s / 2.0) / (s / 2.0);
            double v = 1.0 - (dx * dx + dy * dy) * 0.35;
            r = C((int)(r * v)); g = C((int)(g * v)); b = C((int)(b * v));

            img.SetPixel(x, y, Color.Color8((byte)r, (byte)g, (byte)b, 255));
        }
    }

    // ===== Darker panel with texture =====
    static void GenPanel(Image img)
    {
        int s = 256;
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            double n = Fbm(x, y, 0.04, 3, 55) * 10;
            int r = C(32 + (int)n);
            int g = C(20 + (int)(n * 0.7));
            int b = C(28 + (int)(n * 0.5));
            double edge = EdgeDarken(x, y, s, 0.18);
            r = C((int)(r * edge)); g = C((int)(g * edge)); b = C((int)(b * edge));
            img.SetPixel(x, y, Color.Color8((byte)r, (byte)g, (byte)b, 235));
        }
    }

    // ===== Button normal: warm amber brown =====
    static void GenBtnNormal(Image img)
    {
        int w = 192, h = 48;
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            double n = Fbm(x, y, 0.06, 2, 33) * 7;
            int r = C(68 + (int)n);
            int g = C(40 + (int)(n * 0.7));
            int b = C(22 + (int)(n * 0.4));
            if (y < 4) { r = C(r + 32); g = C(g + 22); b = C(b + 14); }
            if (y >= h - 3) { r = C(r - 12); g = C(g - 8); b = C(b - 5); }
            double lr = 1.0 - Math.Abs(x - w / 2.0) / (w / 2.0) * 0.12;
            r = C((int)(r * lr)); g = C((int)(g * lr)); b = C((int)(b * lr));
            img.SetPixel(x, y, Color.Color8((byte)r, (byte)g, (byte)b, 250));
        }
    }

    // ===== Button hover =====
    static void GenBtnHover(Image img)
    {
        int w = 192, h = 48;
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            double n = Fbm(x, y, 0.06, 2, 33) * 7;
            int r = C(95 + (int)n);
            int g = C(58 + (int)(n * 0.7));
            int b = C(32 + (int)(n * 0.4));
            if (y < 4) { r = C(r + 42); g = C(g + 30); b = C(b + 20); }
            if (y >= h - 3) { r = C(r - 7); g = C(g - 4); b = C(b - 2); }
            double lr = 1.0 - Math.Abs(x - w / 2.0) / (w / 2.0) * 0.08;
            r = C((int)(r * lr)); g = C((int)(g * lr)); b = C((int)(b * lr));
            img.SetPixel(x, y, Color.Color8((byte)r, (byte)g, (byte)b, 250));
        }
    }

    // ===== Button pressed =====
    static void GenBtnPressed(Image img)
    {
        int w = 192, h = 48;
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            double n = Fbm(x, y, 0.06, 2, 33) * 5;
            int r = C(42 + (int)n);
            int g = C(24 + (int)(n * 0.6));
            int b = C(14 + (int)(n * 0.3));
            if (y < 4) { r = C(r - 10); g = C(g - 6); b = C(b - 4); }
            if (y >= h - 3) { r = C(r + 10); g = C(g + 6); b = C(b + 4); }
            img.SetPixel(x, y, Color.Color8((byte)r, (byte)g, (byte)b, 250));
        }
    }

    // ===== Fbm noise =====
    static double Fbm(int x, int y, double scale, int octaves, int seed)
    {
        double val = 0, amp = 1, freq = 1, maxV = 0;
        for (int i = 0; i < octaves; i++)
        {
            val += NoiseVal(x * scale * freq, y * scale * freq, seed + i * 17) * amp;
            maxV += amp; amp *= 0.5; freq *= 2;
        }
        return val / maxV;
    }

    static double NoiseVal(double x, double y, int seed)
    {
        int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
        double fx = x - xi, fy = y - yi;
        double sx = fx * fx * (3 - 2 * fx);
        double sy = fy * fy * (3 - 2 * fy);
        return Lerp(
            Lerp(Grad(xi, yi, seed), Grad(xi + 1, yi, seed), sx),
            Lerp(Grad(xi, yi + 1, seed), Grad(xi + 1, yi + 1, seed), sx), sy);
    }

    static double Grad(int x, int y, int seed)
    {
        uint h = (uint)((x * 374761393 + y * 668265263 + seed * 1274126177) & 0x7FFFFFFF);
        h = (h ^ (h >> 13)) * 1274126177;
        h ^= h >> 16;
        return (h % 65536) / 32768.0 - 1.0;
    }

    static double Lerp(double a, double b, double t) => a + (b - a) * t;

    static double EdgeDarken(int x, int y, int s, double strength)
    {
        double dx = Math.Abs(x - s / 2.0) / (s / 2.0);
        double dy = Math.Abs(y - s / 2.0) / (s / 2.0);
        return 1.0 - Math.Max(dx, dy) * strength;
    }

    static int C(int v) => v < 0 ? 0 : v > 255 ? 255 : v;
}
