namespace SwordFateCultivationRecord;

public static class UITheme
{
    private static FontFile? _titleFont; // MaShanZheng - calligraphy, for headings
    private static FontFile? _bodyFont;  // Zpix - pixel, for body text
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

    /// <summary>Calligraphy font for titles/headings (磅礴大气).</summary>
    public static FontFile? TitleFont => _titleFont;
    /// <summary>Pixel font for body text and buttons (像素风格).</summary>
    public static FontFile? BodyFont => _bodyFont;

    public static void Init()
    {
        if (_inited) return;
        _inited = true;

        UITextures.Generate();

        // Title font: MaShanZheng calligraphy
        if (ResourceLoader.Exists("res://Resources/Fonts/MaShanZheng-Regular.ttf"))
            try { _titleFont = ResourceLoader.Load<FontFile>("res://Resources/Fonts/MaShanZheng-Regular.ttf"); } catch { }
        if (_titleFont == null && ResourceLoader.Exists("res://Resources/Fonts/MaShanZheng-Regular.woff2"))
            try { _titleFont = ResourceLoader.Load<FontFile>("res://Resources/Fonts/MaShanZheng-Regular.woff2"); } catch { }

        // Body font: Zpix pixel font
        if (ResourceLoader.Exists("res://Resources/Fonts/zpix.ttf"))
            try { _bodyFont = ResourceLoader.Load<FontFile>("res://Resources/Fonts/zpix.ttf"); } catch { }
    }

    /// <summary>Apply default theme (title font) to root control.</summary>
    public static void ApplyTo(Control root)
    {
        if (_titleFont == null && _bodyFont == null) return;
        var theme = root.Theme ?? new Theme();
        theme.DefaultFont = _bodyFont ?? _titleFont;
        theme.DefaultFontSize = 14;
        root.Theme = theme;
    }

    /// <summary>Apply pixel body font to a specific control.</summary>
    public static void ApplyBodyFont(Control control)
    {
        if (_bodyFont == null) return;
        control.AddThemeFontOverride("font", _bodyFont);
        control.AddThemeFontSizeOverride("font_size", 13);
    }

    /// <summary>Apply calligraphy title font to a label, with given size.</summary>
    public static void ApplyTitleFont(Label label, int size)
    {
        if (_titleFont != null)
        {
            label.AddThemeFontOverride("font", _titleFont);
            label.AddThemeFontSizeOverride("font_size", size);
        }
    }

    // ===== StyleBox Factories =====

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

    public static StyleBoxFlat SidebarBtnNormal()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0),
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
        };
    }

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

    public static StyleBoxFlat ContentAreaStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.04f, 0.03f, 0.08f, 0.70f),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
        };
    }

    // ===== Button StyleBoxes =====

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
