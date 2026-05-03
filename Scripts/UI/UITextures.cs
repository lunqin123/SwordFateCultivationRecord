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
        BgPaper    = LoadOrFallback("res://Resources/Textures/bg_paper.png",    62, 44, 28);
        Panel      = LoadOrFallback("res://Resources/Textures/panel.png",        28, 18, 24);
        BtnNormal  = LoadOrFallback("res://Resources/Textures/btn_normal.png",   75, 45, 22);
        BtnHover   = LoadOrFallback("res://Resources/Textures/btn_hover.png",   110, 70, 30);
        BtnPressed = LoadOrFallback("res://Resources/Textures/btn_pressed.png",  42, 24, 12);
    }

    static ImageTexture LoadOrFallback(string path, int r, int g, int b)
    {
        if (ResourceLoader.Exists(path))
        {
            var tex = ResourceLoader.Load<Texture2D>(path);
            if (tex is ImageTexture it) return it;
            if (tex != null) return ImageTexture.CreateFromImage(tex.GetImage());
        }
        // Fallback: solid color 2x2 texture
        var img = Image.CreateEmpty(2, 2, false, Image.Format.Rgba8);
        img.Fill(Color.Color8((byte)r, (byte)g, (byte)b, 255));
        return ImageTexture.CreateFromImage(img);
    }
}
