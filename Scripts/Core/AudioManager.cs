namespace SwordFateCultivationRecord;

public static class AudioManager
{
    private static Dictionary<string, AudioStreamWav> _sounds = new();
    private static List<string> _bgmPaths = new();
    private static AudioStreamPlayer? _bgmPlayer;
    private static Node? _root;
    private static bool _inited;
    private static readonly List<AudioStreamPlayer> _sfxPool = new();
    private const int MaxSfxPlayers = 4;

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

    /// <summary>Scan BGM folder for mp3 files. Falls back to procedural bgm if DirAccess fails (exports).</summary>
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
                    string name = fn.Replace(".mp3", "").Replace(".ogg", "").Replace(".wav", "");
                    names.Add(name);
                }
                fn = d.GetNext();
            }
            d.ListDirEnd();
        }

        // Fallback: if DirAccess found nothing (common in exports), try known files
        if (_bgmPaths.Count == 0)
        {
            string[] knownBgm = {
                "res://Resources/Audio/BGM/Jade Thunder Gong 1.mp3",
                "res://Resources/Audio/BGM/Jade Thunder Gong 2.mp3",
                "res://Resources/Audio/BGM/Jade Thunderwood 1.mp3",
                "res://Resources/Audio/BGM/琥珀丹火.mp3",
                "res://Resources/Audio/BGM/琥珀丹火 2.mp3",
                "res://Resources/Audio/bgm_ambient.wav",
            };
            foreach (var p in knownBgm)
            {
                if (ResourceLoader.Exists(p))
                {
                    _bgmPaths.Add(p);
                    string name = p.Replace("res://Resources/Audio/BGM/", "").Replace("res://Resources/Audio/", "").Replace(".mp3", "").Replace(".ogg", "").Replace(".wav", "");
                    names.Add(name);
                }
            }
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
            VolumeDb = Mathf.LinearToDb(GameSettings.MusicVolume * 0.9f),
        };
        _root!.CallDeferred(Node.MethodName.AddChild, _bgmPlayer);
        _bgmPlayer.Finished += () =>
        {
            switch (GameSettings.BgmMode)
            {
                case BgmLoopMode.Single:
                    PlayBgmTrack(GameSettings.BgmIndex);
                    break;
                case BgmLoopMode.Random:
                    int randIdx = Random.Shared.Next(_bgmPaths.Count);
                    GameSettings.BgmIndex = randIdx;
                    GameSettings.Save();
                    PlayBgmTrack(randIdx);
                    break;
                default:
                    int next = (GameSettings.BgmIndex + 1) % _bgmPaths.Count;
                    GameSettings.BgmIndex = next;
                    GameSettings.Save();
                    PlayBgmTrack(next);
                    break;
            }
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
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(GameSettings.MasterVolume * GameSettings.MusicVolume * 0.9f);
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
            if (t == "宗门倾覆") PlaySfx("event_gameover");
        };
    }

    public static void PlayClick() => PlaySfx("ui_click");
    public static void PlayOpen() => PlaySfx("ui_open");
    public static void PlayClose() => PlaySfx("ui_close");
    public static void PlayBuild() => PlaySfx("action_build");
    public static void PlayUpgrade() => PlaySfx("action_upgrade");
    public static void PlayCompanion() => PlaySfx("action_companion");

    public static void PauseBGM()
    {
        _bgmPlayer?.CallDeferred(AudioStreamPlayer.MethodName.Stop);
    }

    public static void ResumeBGM()
    {
        if (_bgmPlayer != null && !_bgmPlayer.Playing && _bgmPaths.Count > 0)
            _bgmPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play);
    }

    public static bool IsBGMRunning => _bgmPlayer != null && _bgmPlayer.Playing;

    /// <summary>Stop and clean up all audio before quitting.</summary>
    public static void Shutdown()
    {
        _bgmPlayer?.Stop();
        _bgmPlayer?.QueueFree();
        _bgmPlayer = null;
        foreach (var p in _sfxPool) p.QueueFree();
        _sfxPool.Clear();
        _inited = false;
        _root = null;
    }

    static void PlaySfx(string name)
    {
        if (_root == null || !_sounds.TryGetValue(name, out var stream)) return;
        float vol = Mathf.LinearToDb(GameSettings.MasterVolume * GameSettings.SfxVolume);
        // Try to get an idle player from pool
        var player = _sfxPool.FirstOrDefault(p => !p.Playing);
        if (player == null)
        {
            if (_sfxPool.Count >= MaxSfxPlayers)
            {
                // Force-reuse the oldest player (no new node)
                player = _sfxPool[0];
                _sfxPool.RemoveAt(0);
                player.Stop();
            }
            else
            {
                player = new AudioStreamPlayer();
                _root.AddChild(player);
                _sfxPool.Add(player);
                player.Finished += () => { };
            }
        }
        player.Stream = stream;
        player.VolumeDb = vol;
        player.CallDeferred(AudioStreamPlayer.MethodName.Play);
    }
}
