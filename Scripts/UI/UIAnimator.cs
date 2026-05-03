namespace SwordFateCultivationRecord;

public static class UIAnimator
{
    public static void WindowOpen(Control control)
    {
        control.Modulate = new Color(1, 1, 1, 0);
        control.Scale = new Vector2(0.85f, 0.85f);
        var t = control.CreateTween().SetParallel();
        t.TweenProperty(control, "scale", Vector2.One, 0.35f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        t.TweenProperty(control, "modulate", new Color(1, 1, 1, 1), 0.25f)
            .SetEase(Tween.EaseType.Out);
    }

    public static void CardAppear(Control control)
    {
        control.Modulate = new Color(1, 1, 1, 0);
        control.Position += new Vector2(0, 30);
        var t = control.CreateTween().SetParallel();
        t.TweenProperty(control, "position", control.Position - new Vector2(0, 30), 0.3f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        t.TweenProperty(control, "modulate", new Color(1, 1, 1, 1), 0.3f)
            .SetEase(Tween.EaseType.Out);
    }

    public static void ButtonPress(Control control)
    {
        var t = control.CreateTween();
        t.TweenProperty(control, "scale", new Vector2(0.92f, 0.92f), 0.06f)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(control, "scale", Vector2.One, 0.15f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
    }

    public static void NotificationSlide(Control control)
    {
        control.Modulate = new Color(1, 1, 1, 0);
        var startX = control.Position.X + 100;
        var targetX = control.Position.X;
        control.Position = new Vector2(startX, control.Position.Y);
        var t = control.CreateTween().SetParallel();
        t.TweenProperty(control, "position:x", targetX, 0.4f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
        t.TweenProperty(control, "modulate", new Color(1, 1, 1, 1), 0.3f)
            .SetEase(Tween.EaseType.Out);
    }

    public static void Pulse(Control control)
    {
        var t = control.CreateTween();
        t.TweenProperty(control, "scale", new Vector2(1.04f, 1.04f), 0.2f)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(control, "scale", Vector2.One, 0.3f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
    }

    public static void Shake(Control control)
    {
        var origX = control.Position.X;
        var t = control.CreateTween();
        t.TweenProperty(control, "position:x", origX - 8, 0.05f);
        t.TweenProperty(control, "position:x", origX + 8, 0.05f);
        t.TweenProperty(control, "position:x", origX - 5, 0.05f);
        t.TweenProperty(control, "position:x", origX + 5, 0.05f);
        t.TweenProperty(control, "position:x", origX, 0.06f);
    }

    /// <summary>Card hover: gentle scale up with gold glow.</summary>
    public static void CardHoverEnter(Control control)
    {
        var t = control.CreateTween();
        t.TweenProperty(control, "scale", new Vector2(1.03f, 1.03f), 0.15f)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>Card hover exit: scale back to normal.</summary>
    public static void CardHoverExit(Control control)
    {
        var t = control.CreateTween();
        t.TweenProperty(control, "scale", Vector2.One, 0.15f)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>Tab content switch: quick fade transition.</summary>
    public static void TabSwitch(Control control)
    {
        control.Modulate = new Color(1, 1, 1, 0.5f);
        var t = control.CreateTween();
        t.TweenProperty(control, "modulate", new Color(1, 1, 1, 1), 0.2f)
            .SetEase(Tween.EaseType.Out);
    }
}
