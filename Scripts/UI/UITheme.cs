namespace SwordFateCultivationRecord;

public static class UITheme
{
    private static FontFile? _font;
    private static bool _inited;

    // ===== Color Palette =====
    public static Color DarkBg       => new(0.06f, 0.04f, 0.10f);
    public static Color CardBg       => new(0.10f, 0.07f, 0.14f);
    public static Color PanelBg      => new(0.08f, 0.06f, 0.13f);
    public static Color Gold         => new(0.91f, 0.72f, 0.29f);
    public static Color GoldDark     => new(0.65f, 0.50f, 0.18f);
    public static Color Crimson      => new(0.85f, 0.25f, 0.25f);
    public static Color TextPrimary  => new(0.90f, 0.88f, 0.85f);
    public static Color TextDim      => new(0.50f, 0.48f, 0.55f);
    public static Color TextGreen    => new(0.35f, 0.85f, 0.35f);
    public static Color TextBlue     => new(0.45f, 0.65f, 0.90f);
    public static Color TextOrange   => new(0.95f, 0.60f, 0.20f);
    public static Color SidebarBg    => new(0.05f, 0.03f, 0.08f);
    public static Color SidebarHover => new(0.15f, 0.10f, 0.22f);
    public static Color SidebarActive=> new(0.20f, 0.14f, 0.30f);
    public static Color TopBarBg     => new(0.07f, 0.05f, 0.12f);
    public static Color BottomBarBg  => new(0.07f, 0.05f, 0.12f);

    public static void Init()
    {
        if (_inited) return;
        _inited = true;

        UITextures.Generate();

        string fontPath = "res://Resources/Fonts/MaShanZheng-Regular.ttf";
        if (!ResourceLoader.Exists(fontPath))
            fontPath = "res://Resources/Fonts/MaShanZheng-Regular.woff2";
        if (ResourceLoader.Exists(fontPath))
        {
            try { _font = ResourceLoader.Load<FontFile>(fontPath); }
            catch { }
        }
    }

    public static void ApplyTo(Control root)
    {
        if (_font == null) return;
        var theme = root.Theme ?? new Theme();
        theme.DefaultFont = _font;
        theme.DefaultFontSize = 16;
        root.Theme = theme;
    }

    // ===== StyleBox Factories =====

    /// <summary>Standard card with gold-trimmed border on dark background.</summary>
    public static StyleBoxFlat CardStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = CardBg,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            BorderWidthBottom = 1, BorderWidthLeft = 1,
            BorderWidthRight = 1, BorderWidthTop = 1,
            BorderColor = GoldDark,
            ShadowSize = 4, ShadowOffset = new Vector2(0, 2),
            ShadowColor = new Color(0, 0, 0, 0.5f),
        };
    }

    /// <summary>Card hover state — brighter border.</summary>
    public static StyleBoxFlat CardHoverStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.14f, 0.10f, 0.20f),
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            BorderWidthBottom = 1, BorderWidthLeft = 1,
            BorderWidthRight = 1, BorderWidthTop = 1,
            BorderColor = Gold,
            ShadowSize = 6, ShadowOffset = new Vector2(0, 3),
            ShadowColor = new Color(0, 0, 0, 0.6f),
        };
    }

    /// <summary>Gold ornate border — thin decorative panel.</summary>
    public static StyleBoxFlat OrnateBorder()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.05f, 0.14f, 0.92f),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            BorderWidthBottom = 2, BorderWidthLeft = 2,
            BorderWidthRight = 2, BorderWidthTop = 2,
            BorderColor = GoldDark,
        };
    }

    /// <summary>Sidebar button — normal state.</summary>
    public static StyleBoxFlat SidebarBtnNormal()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0),
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
        };
    }

    /// <summary>Sidebar button — hover state.</summary>
    public static StyleBoxFlat SidebarBtnHover()
    {
        return new StyleBoxFlat
        {
            BgColor = SidebarHover,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
            BorderWidthLeft = 3, BorderWidthTop = 0,
            BorderWidthRight = 0, BorderWidthBottom = 0,
            BorderColor = Gold,
        };
    }

    /// <summary>Sidebar button — active/selected state.</summary>
    public static StyleBoxFlat SidebarBtnActive()
    {
        return new StyleBoxFlat
        {
            BgColor = SidebarActive,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
            BorderWidthLeft = 3, BorderWidthTop = 0,
            BorderWidthRight = 0, BorderWidthBottom = 0,
            BorderColor = Gold,
        };
    }

    /// <summary>Top / bottom bar background.</summary>
    public static StyleBoxFlat BarStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = TopBarBg,
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
            BorderWidthBottom = 1, BorderWidthTop = 0,
            BorderWidthLeft = 0, BorderWidthRight = 0,
            BorderColor = GoldDark,
        };
    }

    /// <summary>Top bar style (border at bottom).</summary>
    public static StyleBoxFlat TopBarStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = TopBarBg,
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
            BorderWidthBottom = 2, BorderWidthTop = 0,
            BorderWidthLeft = 0, BorderWidthRight = 0,
            BorderColor = Gold,
        };
    }

    /// <summary>Bottom bar style (border at top).</summary>
    public static StyleBoxFlat BottomBarStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = BottomBarBg,
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
            BorderWidthTop = 2, BorderWidthBottom = 0,
            BorderWidthLeft = 0, BorderWidthRight = 0,
            BorderColor = Gold,
        };
    }

    /// <summary>Sidebar panel background.</summary>
    public static StyleBoxFlat SidebarPanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = SidebarBg,
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
            BorderWidthRight = 1, BorderWidthTop = 0,
            BorderWidthLeft = 0, BorderWidthBottom = 0,
            BorderColor = GoldDark,
        };
    }

    /// <summary>Content area background.</summary>
    public static StyleBoxFlat ContentAreaStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.04f, 0.03f, 0.08f, 0.70f),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
        };
    }

    // ===== Button StyleBoxes (with textures or fallback) =====

    public static StyleBox BtnStyleNormal()
    {
        return UITextures.BtnNormal != null
            ? new StyleBoxTexture { Texture = UITextures.BtnNormal, ModulateColor = new Color(0.92f, 0.85f, 0.78f) }
            : new StyleBoxFlat { BgColor = new Color(0.22f, 0.15f, 0.08f), CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = GoldDark };
    }

    public static StyleBox BtnStyleHover()
    {
        return UITextures.BtnHover != null
            ? new StyleBoxTexture { Texture = UITextures.BtnHover }
            : new StyleBoxFlat { BgColor = new Color(0.35f, 0.23f, 0.12f), CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = Gold };
    }

    public static StyleBox BtnStylePressed()
    {
        return UITextures.BtnPressed != null
            ? new StyleBoxTexture { Texture = UITextures.BtnPressed }
            : new StyleBoxFlat { BgColor = new Color(0.14f, 0.08f, 0.03f), CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderColor = GoldDark };
    }

    public static StyleBox PanelStyle()
    {
        return UITextures.Panel != null
            ? new StyleBoxTexture { Texture = UITextures.Panel, ModulateColor = new Color(0.92f, 0.88f, 0.85f) }
            : new StyleBoxFlat { BgColor = new Color(0.10f, 0.07f, 0.08f), CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4 };
    }

    public static StyleBox BgStyle()
    {
        return UITextures.BgPaper != null
            ? new StyleBoxTexture { Texture = UITextures.BgPaper }
            : new StyleBoxFlat { BgColor = new Color(0.18f, 0.12f, 0.08f), CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4, CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4 };
    }
}
