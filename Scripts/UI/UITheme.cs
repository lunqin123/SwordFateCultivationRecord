namespace SwordFateCultivationRecord;

public static class UITheme
{
    private static FontFile? _font;
    private static bool _inited;

    public static void Init()
    {
        if (_inited) return;
        _inited = true;

        // Generate textures in-memory (bypasses filesystem issues)
        UITextures.Generate();

        // Load font: try ttf then woff2
        string fontPath = "res://Resources/Fonts/MaShanZheng-Regular.ttf";
        if (!ResourceLoader.Exists(fontPath))
            fontPath = "res://Resources/Fonts/MaShanZheng-Regular.woff2";
        if (ResourceLoader.Exists(fontPath))
            _font = ResourceLoader.Load<FontFile>(fontPath);
    }

    public static void ApplyTo(Control root)
    {
        if (_font == null) return;
        var theme = root.Theme ?? new Theme();
        theme.DefaultFont = _font;
        theme.DefaultFontSize = 14;
        root.Theme = theme;
    }

    // ===== StyleBox helpers — uses in-memory textures =====

    public static StyleBox BtnStyleNormal()
    {
        return UITextures.BtnNormal != null
            ? new StyleBoxTexture { Texture = UITextures.BtnNormal, ModulateColor = new Color(0.95f, 0.90f, 0.88f) }
            : FlatStyle(0.27f, 0.17f, 0.10f); // warm amber brown
    }

    public static StyleBox BtnStyleHover()
    {
        return UITextures.BtnHover != null
            ? new StyleBoxTexture { Texture = UITextures.BtnHover }
            : FlatStyle(0.40f, 0.26f, 0.15f); // brighter amber
    }

    public static StyleBox BtnStylePressed()
    {
        return UITextures.BtnPressed != null
            ? new StyleBoxTexture { Texture = UITextures.BtnPressed }
            : FlatStyle(0.17f, 0.09f, 0.04f); // darker pressed
    }

    public static StyleBox PanelStyle()
    {
        return UITextures.Panel != null
            ? new StyleBoxTexture { Texture = UITextures.Panel, ModulateColor = new Color(0.95f, 0.92f, 0.90f) }
            : FlatStyle(0.12f, 0.08f, 0.10f); // dark panel
    }

    public static StyleBox BgStyle()
    {
        return UITextures.BgPaper != null
            ? new StyleBoxTexture { Texture = UITextures.BgPaper }
            : FlatStyle(0.20f, 0.14f, 0.09f); // warm brown parchment
    }

    static StyleBoxFlat FlatStyle(float r, float g, float b) => new()
    {
        BgColor = new Color(r, g, b),
        CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
        CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
    };

    static StyleBoxFlat FlatStyle(Color c) => new()
    {
        BgColor = c,
        CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
        CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
    };
}
