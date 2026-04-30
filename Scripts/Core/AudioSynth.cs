namespace SwordFateCultivationRecord;

/// <summary>
/// High-quality 16-bit sound synthesizer for cultivation game atmosphere.
/// </summary>
public static class AudioSynth
{
    const int SampleRate = 44100;
    static readonly Random _rng = new(42);

    public static void GenerateAll()
    {
        GenerateUI();
        GenerateActions();
        GenerateEvents();
        // BGM: generate once, don't overwrite (large file, needs Godot import)
        if (!ResourceLoader.Exists("res://Resources/Audio/bgm_ambient.wav"))
            GenerateBGM();
    }

    static void GenerateUI()
    {
        // Soft wood tap: high freq + low body
        SaveWav("ui_click",
            GenTone(1800f, 0.04f, EnvPerc, 0.4f),
            GenTone(900f, 0.02f, EnvPerc, 0.15f));

        // Gentle wind chime
        SaveWav("ui_open",
            GenTone(1200f, 0.3f, EnvChime, 0.5f),
            GenTone(1800f, 0.4f, EnvChime, 0.3f),
            GenTone(2400f, 0.35f, EnvChime, 0.15f));

        // Soft reversed chime
        SaveWav("ui_close",
            GenTone(1600f, 0.2f, EnvRev, 0.4f),
            GenTone(1100f, 0.15f, EnvRev, 0.25f));
    }

    static void GenerateActions()
    {
        // Bright C-E-G-C chord
        SaveWav("action_recruit",
            GenTone(1047f, 0.2f, EnvChime, 0.5f),
            GenTone(1319f, 0.3f, EnvChime, 0.4f),
            GenTone(1568f, 0.4f, EnvChime, 0.35f),
            GenTone(2093f, 0.35f, EnvChime, 0.2f));

        // Wooden hammer
        SaveWav("action_build",
            GenNoise(0.08f, EnvPerc, 0.5f),
            GenTone(200f, 0.06f, EnvPerc, 0.6f),
            GenTone(400f, 0.04f, EnvPerc, 0.3f));

        // Rich bell chord
        SaveWav("action_upgrade",
            GenBell(880f, 0.6f, 0.7f),
            GenBell(1320f, 0.55f, 0.3f),
            GenBell(1760f, 0.5f, 0.15f));

        // Triumphant shimmer
        SaveWav("action_breakthrough",
            GenBell(523f, 1.2f, 0.4f),
            GenBell(659f, 1.4f, 0.35f),
            GenBell(784f, 1.5f, 0.3f),
            GenBell(1047f, 1.7f, 0.2f),
            GenShimmer(2093f, 2.2f, 0.15f));

        // Sad falling minor
        SaveWav("action_fail",
            GenTone(440f, 0.35f, EnvFall, 0.4f),
            GenTone(370f, 0.45f, EnvFall, 0.3f),
            GenTone(330f, 0.55f, EnvFall, 0.2f));

        // Gentle departure
        SaveWav("action_dismiss",
            GenTone(587f, 0.25f, EnvFall, 0.35f),
            GenTone(494f, 0.4f, EnvFall, 0.25f));

        // Subtle tick
        SaveWav("action_daypass", GenTone(2400f, 0.015f, EnvPerc, 0.06f));

        // Gentle harmony
        SaveWav("action_companion",
            GenTone(659f, 0.3f, EnvChime, 0.4f),
            GenTone(784f, 0.4f, EnvChime, 0.35f),
            GenTone(1047f, 0.35f, EnvChime, 0.2f));
    }

    static void GenerateEvents()
    {
        // Mysterious long bell
        SaveWav("event_trigger",
            GenBell(392f, 1.5f, 0.5f),
            GenBell(523f, 1.8f, 0.35f),
            GenShimmer(1568f, 2.5f, 0.2f));

        // Deep gong
        SaveWav("event_gameover",
            GenGong(130f, 3f, 0.8f),
            GenGong(196f, 2.5f, 0.5f),
            GenNoise(0.3f, EnvPerc, 0.15f));
    }

    // ===== BGM: Pentatonic meditative ambient loop =====
    public static void GenerateBGM()
    {
        float dur = 30f;
        int len = (int)(SampleRate * dur);
        var buf = new float[len];

        // Pentatonic scale (D 宫调): D4 E4 G4 A4 C5 D5 E5 G5 A5
        float[] scale = { 294f, 330f, 392f, 440f, 523f, 587f, 659f, 784f, 880f };
        var rng = new Random(42);

        // Drone pad: D-A fifth with slow movement
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float pad = MathF.Sin(2f * MathF.PI * 147f * t) * 0.06f   // D3
                      + MathF.Sin(2f * MathF.PI * 220f * t) * 0.04f    // A3
                      + MathF.Sin(2f * MathF.PI * 294f * t) * 0.03f;   // D4
            float lfo = 1f + MathF.Sin(2f * MathF.PI * 0.08f * t) * 0.4f
                          + MathF.Sin(2f * MathF.PI * 0.15f * t) * 0.2f;
            buf[i] = pad * lfo;
        }

        // Bell notes — much denser now
        for (int note = 0; note < 60; note++)
        {
            float onset = (float)rng.NextDouble() * dur;
            float noteDur = 1.2f + (float)rng.NextDouble() * 2.5f;
            float freq = scale[rng.Next(scale.Length)];
            float amp = 0.08f + (float)rng.NextDouble() * 0.1f;
            var bell = GenBell(freq, noteDur, amp);
            int start = (int)(onset * SampleRate);
            for (int i = 0; i < bell.Length && start + i < len; i++)
                buf[start + i] += bell[i];
        }

        // Subtle wind
        float lpfW = 0f;
        for (int i = 0; i < len; i++)
        {
            float n = (float)(rng.NextDouble() * 2f - 1f);
            lpfW += (n - lpfW) * 0.1f;
            buf[i] += lpfW * 0.01f;
        }

        // Crossfade for seamless loop
        int fadeLen = SampleRate * 2;
        for (int i = 0; i < fadeLen; i++)
        {
            float f = (float)i / fadeLen;
            int outIdx = len - fadeLen + i;
            buf[outIdx] = buf[outIdx] * (1f - f) + buf[i] * f;
        }

        // Normalize and boost
        float max = 0.001f;
        for (int i = 0; i < len; i++) { float a = MathF.Abs(buf[i]); if (a > max) max = a; }
        float gain = 0.5f / max;
        for (int i = 0; i < len; i++) buf[i] = Math.Clamp(buf[i] * gain, -0.95f, 0.95f);

        SaveRaw("bgm_ambient", buf);
    }

    // ===== Envelope presets [start, attack, peak, decay, sustain, release] =====
    static readonly float[] EnvPerc  = { 0f, 0.003f, 1f, 0.01f, 0f, 0.1f };
    static readonly float[] EnvChime = { 0f, 0.01f, 1f, 0.05f, 0f, 0.5f };
    static readonly float[] EnvRev   = { 0f, 0.1f, 0.8f, 0.05f, 0f, 0.1f };
    static readonly float[] EnvFall  = { 1f, 0.01f, 0.5f, 0.25f, 0f, 0.6f };

    // ===== Generators =====

    static float[] GenTone(float freq, float dur, float[] env, float amp)
    {
        int len = (int)(SampleRate * dur);
        var buf = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float phase = 2f * MathF.PI * freq * t;
            float sample = MathF.Sin(phase) * 0.7f + MathF.Sin(phase * 2f) * 0.2f + MathF.Sin(phase * 3f) * 0.1f;
            buf[i] = sample * amp * Envelope(t, dur, env);
        }
        return buf;
    }

    static float[] GenBell(float freq, float dur, float amp)
    {
        int len = (int)(SampleRate * dur);
        var buf = new float[len];
        float[] partials = { 1f, 2.76f, 5.4f, 8.9f, 13.3f };
        float[] pAmp = { 0.6f, 0.4f, 0.25f, 0.12f, 0.06f };
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float sample = 0f;
            for (int p = 0; p < partials.Length; p++)
                sample += MathF.Sin(2f * MathF.PI * freq * partials[p] * t) * pAmp[p];
            float env = MathF.Exp(-t * 5f / dur) * (1f - MathF.Exp(-t * 50f));
            buf[i] = sample * amp * env;
        }
        return buf;
    }

    static float[] GenGong(float freq, float dur, float amp)
    {
        int len = (int)(SampleRate * dur);
        var buf = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float sample = MathF.Sin(2f * MathF.PI * freq * t) * 0.5f
                + MathF.Sin(2f * MathF.PI * freq * 2.01f * t) * 0.3f
                + MathF.Sin(2f * MathF.PI * freq * 3.02f * t) * 0.15f
                + MathF.Sin(2f * MathF.PI * freq * 5.1f * t) * 0.05f;
            float env = MathF.Exp(-t * 2.5f / dur);
            float tremolo = 1f + MathF.Sin(2f * MathF.PI * 4f * t) * 0.15f;
            buf[i] = sample * amp * env * tremolo;
        }
        return buf;
    }

    static float[] GenShimmer(float freq, float dur, float amp)
    {
        int len = (int)(SampleRate * dur);
        var buf = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float sample = MathF.Sin(2f * MathF.PI * freq * t) * 0.4f
                + MathF.Sin(2f * MathF.PI * freq * 1.5f * t) * 0.25f
                + MathF.Sin(2f * MathF.PI * freq * 2.3f * t) * 0.15f;
            float env = MathF.Exp(-t * 4f / dur);
            float shimmer = 1f + MathF.Sin(2f * MathF.PI * 7f * t) * 0.3f;
            buf[i] = sample * amp * env * shimmer;
        }
        return buf;
    }

    static float[] GenNoise(float dur, float[] env, float amp)
    {
        int len = (int)(SampleRate * dur);
        var buf = new float[len];
        float lpf = 0f;
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SampleRate;
            float noise = (float)(_rng.NextDouble() * 2f - 1f);
            lpf += (noise - lpf) * 0.15f;
            buf[i] = lpf * amp * Envelope(t, dur, env);
        }
        return buf;
    }

    // ===== Envelope =====
    static float Envelope(float t, float dur, float[] env)
    {
        float a = env[0], at = env[1], p = env[2], dt = env[3], s = env[4], rt = env[5];
        float total = at + dt + rt;
        float scale = dur / total;
        float tt = t * scale;
        if (tt < at) return a + (p - a) * (tt / at);
        if (tt < at + dt) return p + (s - p) * ((tt - at) / dt);
        if (tt < at + dt + rt) return s * (1f - (tt - at - dt) / rt);
        return 0f;
    }

    // ===== Mix & Save =====
    static float[] Mix(params float[][] tracks)
    {
        int maxLen = 0;
        foreach (var t in tracks) if (t.Length > maxLen) maxLen = t.Length;
        var buf = new float[maxLen];
        foreach (var t in tracks)
            for (int i = 0; i < t.Length; i++)
                buf[i] += t[i];
        return buf;
    }

    static void SaveRaw(string name, float[] buf)
    {
        var samples = new short[buf.Length];
        for (int i = 0; i < buf.Length; i++)
            samples[i] = (short)(Math.Clamp(buf[i], -0.99f, 0.99f) * 32767f);
        WriteWavFile(name, samples);
    }

    static void SaveWav(string name, params float[][] tracks)
    {
        var mix = Mix(tracks);
        var samples = new short[mix.Length];
        for (int i = 0; i < mix.Length; i++)
            samples[i] = (short)(Math.Clamp(mix[i], -0.99f, 0.99f) * 32767f);
        WriteWavFile(name, samples);
    }

    static void WriteWavFile(string name, short[] samples)
    {

        int dataSize = samples.Length * 2;
        var bytes = new byte[44 + dataSize];

        // RIFF
        bytes[0] = (byte)'R'; bytes[1] = (byte)'I'; bytes[2] = (byte)'F'; bytes[3] = (byte)'F';
        WriteI32(bytes, 4, 36 + dataSize);
        bytes[8] = (byte)'W'; bytes[9] = (byte)'A'; bytes[10] = (byte)'V'; bytes[11] = (byte)'E';
        // fmt
        bytes[12] = (byte)'f'; bytes[13] = (byte)'m'; bytes[14] = (byte)'t'; bytes[15] = (byte)' ';
        WriteI32(bytes, 16, 16); WriteI16(bytes, 20, 1); WriteI16(bytes, 22, 1);
        WriteI32(bytes, 24, SampleRate); WriteI32(bytes, 28, SampleRate * 2);
        WriteI16(bytes, 32, 2); WriteI16(bytes, 34, 16);
        // data
        bytes[36] = (byte)'d'; bytes[37] = (byte)'a'; bytes[38] = (byte)'t'; bytes[39] = (byte)'a';
        WriteI32(bytes, 40, dataSize);

        for (int i = 0; i < samples.Length; i++)
        {
            bytes[44 + i * 2] = (byte)(samples[i] & 0xFF);
            bytes[44 + i * 2 + 1] = (byte)((samples[i] >> 8) & 0xFF);
        }

        string path = $"res://Resources/Audio/{name}.wav";
        if (ResourceLoader.Exists(path))
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
        string ip = path + ".import";
        if (ResourceLoader.Exists(ip))
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(ip));

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file?.StoreBuffer(bytes);
    }

    static void WriteI32(byte[] buf, int off, int v)
    {
        buf[off] = (byte)v; buf[off + 1] = (byte)(v >> 8);
        buf[off + 2] = (byte)(v >> 16); buf[off + 3] = (byte)(v >> 24);
    }
    static void WriteI16(byte[] buf, int off, int v)
    {
        buf[off] = (byte)v; buf[off + 1] = (byte)(v >> 8);
    }
}
