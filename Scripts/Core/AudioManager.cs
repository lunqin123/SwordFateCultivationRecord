namespace SwordFateCultivationRecord;

public static class AudioManager
{
    private static Dictionary<string, AudioStreamWav> _sounds = new();
    private static List<string> _bgmPaths = new();
    private static AudioStreamPlayer? _bgmPlayer;
    private static Node? _root;
    private static bool _inited;

    public static IReadOnlyList<string> BgmNames { get; private set; } = Array.Empty<string>();
    public static int CurrentBgmIndex => GameSettings.BgmIndex;

    public static void Init(Node anyNode)
    {
        if (_inited) return;
        _inited = true;
        _root = anyNode.GetTree().Root;
        AudioSynth.GenerateAll();
        LoadSounds();
        ScanBGM();
        StartBGM();
        ConnectEvents();
    }

    static void LoadSounds()
    {
        string[] names = {
            "ui_click", "ui_open", "ui_close",
            "action_recruit", "action_build", "action_upgrade",
            "action_breakthrough", "action_fail", "action_dismiss",
            "action_daypass", "action_companion",
            "event_trigger", "event_gameover"
        };
        foreach (var name in names)
        {
            string path = $"res://Resources/Audio/{name}.wav";
            if (!ResourceLoader.Exists(path)) continue;
            var res = ResourceLoader.Load<AudioStreamWav>(path);
            if (res != null) _sounds[name] = res;
        }
    }

    /// <summary>Scan BGM folder for mp3 files.</summary>
    static void ScanBGM()
    {
        _bgmPaths.Clear();
        var names = new List<string>();
        string dir = "res://Resources/Audio/BGM/";
        using var d = DirAccess.Open(dir);
        if (d != null)
        {
            d.ListDirBegin();
            string fn = d.GetNext();
            while (!string.IsNullOrEmpty(fn))
            {
                if (!d.CurrentIsDir() && (fn.EndsWith(".mp3") || fn.EndsWith(".ogg") || fn.EndsWith(".wav")))
                {
                    _bgmPaths.Add(dir + fn);
                    // Extract display name: remove extension
                    string name = fn.Replace(".mp3", "").Replace(".ogg", "").Replace(".wav", "");
                    names.Add(name);
                }
                fn = d.GetNext();
            }
            d.ListDirEnd();
        }
        BgmNames = names;
        if (GameSettings.BgmIndex >= _bgmPaths.Count)
            GameSettings.BgmIndex = 0;
    }

    static void StartBGM()
    {
        if (_root == null || _bgmPaths.Count == 0) return;
        PlayBgmTrack(GameSettings.BgmIndex);
    }

    static void PlayBgmTrack(int index)
    {
        _bgmPlayer?.QueueFree();
        _bgmPlayer = null;
        if (index < 0 || index >= _bgmPaths.Count) return;

        string path = _bgmPaths[index];
        if (!ResourceLoader.Exists(path)) return;

        // MP3 loads as AudioStreamMP3, WAV as AudioStreamWav — use base class
        var stream = ResourceLoader.Load<AudioStream>(path);
        if (stream == null) return;

        _bgmPlayer = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = Mathf.LinearToDb(GameSettings.MusicVolume * 0.6f),
        };
        _root!.CallDeferred(Node.MethodName.AddChild, _bgmPlayer);
        _bgmPlayer.Finished += () =>
        {
            int next = (GameSettings.BgmIndex + 1) % _bgmPaths.Count;
            GameSettings.BgmIndex = next;
            PlayBgmTrack(next);
        };
        _bgmPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play);
    }

    /// <summary>Switch to a different BGM track.</summary>
    public static void SetBgm(int index)
    {
        GameSettings.BgmIndex = index;
        GameSettings.Save();
        if (_inited)
            PlayBgmTrack(index);
    }

    public static void UpdateBGMVolume()
    {
        if (_bgmPlayer != null)
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(GameSettings.MusicVolume * 0.6f);
    }

    public static void ConnectEvents()
    {
        EventBus.DiscipleRecruited += _ => PlaySfx("action_recruit");
        EventBus.DiscipleDeparted += _ => PlaySfx("action_dismiss");
        EventBus.BreakthroughSuccess += _ => PlaySfx("action_breakthrough");
        EventBus.BreakthroughFailed += _ => PlaySfx("action_fail");
        EventBus.EventChoiceRequired += _ => PlaySfx("event_trigger");
        EventBus.GameNotification += (t, _) =>
        {
            if (t == "宗门覆灭") PlaySfx("event_gameover");
        };
    }

    public static void PlayClick() => PlaySfx("ui_click");
    public static void PlayOpen() => PlaySfx("ui_open");
    public static void PlayClose() => PlaySfx("ui_close");
    public static void PlayBuild() => PlaySfx("action_build");
    public static void PlayUpgrade() => PlaySfx("action_upgrade");
    public static void PlayCompanion() => PlaySfx("action_companion");

    static void PlaySfx(string name)
    {
        if (_root == null || !_sounds.TryGetValue(name, out var stream)) return;
        var player = new AudioStreamPlayer { Stream = stream, VolumeDb = 0 };
        _root.AddChild(player);
        player.Finished += () => player.QueueFree();
        player.CallDeferred(AudioStreamPlayer.MethodName.Play);
    }
}
