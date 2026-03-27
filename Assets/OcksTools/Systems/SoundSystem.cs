using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSystem : SingleInstance<SoundSystem>
{
#if UNITY_EDITOR
    public float DebugVolumeMult = 1.0f;
#endif
    public Dictionary<string, float> Volumes = new Dictionary<string, float>();
    public List<OXSoundData> AudioClips = new();
    public List<AudioLibrary> AudioLibraries = new();
    public List<OXMixerData> AudioMixers = new();
    public Dictionary<string, OXSoundData> AudioClipDict = new();
    public Dictionary<string, AudioMixerGroup> AudioMixerDict = new();
    private Dictionary<Transform, List<AudioSource>> AudioSources = new();


    public OXEvent<OXSound> SoundMod = new();
    public OXEvent SoundDictCompile = new();

    public override void Awake2()
    {
#if UNITY_EDITOR
        DebugVolumeMult = 1.0f;
#endif
        foreach (var l in AudioLibraries)
        {
            foreach (var s in l.Datas)
            {
                AddSound(s);
            }
            foreach (var s in l.Mixers)
            {
                AddMixer(s);
            }
        }
        foreach (var s in AudioClips)
        {
            AddSound(s);
        }
        foreach (var s in AudioMixers)
        {
            AddMixer(s);
        }
        SaveSystem.SaveAllData.Append(-1, SaveVolumes);
        SaveSystem.LoadAllData.Append(-1, LoadVolumes);


        ConsoleCommandBuilder.Build(() =>
        {
            ConsoleLol.Instance.Add(new OXCommand("volume").Append(new OXCommand(OXCommand.ExpectedInputType.String).Append(new OXCommand(OXCommand.ExpectedInputType.Double).Action(VolumeCommand))));
        });
    }
    public void SetVolume(string v, float x)
    {
        Volumes.AddOrUpdate(v, x);
    }
    public float GetVolume(string v, float x)
    {
        return Volumes.ContainsKey(v) ? Volumes[v] : 1;
    }
    public void SaveVolumes(SaveProfile dict)
    {
        dict.SetDict("Volumes", Volumes.ABDictionaryToStringDictionary());
    }

    public void LoadVolumes(SaveProfile dict)
    {
        Volumes = dict.GetDict("Volumes", new()).StringDictionaryToABDictionary<string, float>();
    }
    public void VolumeCommand(OXCommandData dat)
    {
        SetVolume(dat.com_caps[1], float.Parse(dat.com[2]));
    }

    private void Start()
    {
        SoundDictCompile.Invoke();
    }
    public void AddSound(OXSoundData s)
    {
        AudioClipDict.AddOrUpdate(s.Name, s);
    }
    public void AddMixer(OXMixerData s)
    {
        AudioMixerDict.AddOrUpdate(s.Name, s.Mixer);
    }
    public void ModSound(OXSound sound, bool findexisting = false)
    {
        float pvolume = 1;
        switch (sound.name)
        {
            case "sound":
                /*
                var k = Random.Range(0, 2);
                switch (k)
                {
                    case 0:
                        pvolume *= 1.2f;
                        break;
                    case 1:
                        sound.name = "new sound";
                        break;
                    case 2:
                        sound.name = "new sound AGAIN";
                        break;
                }*/
                break;
            case "A":
                pvolume = 1.2f;
                break;
            case "B":
                pvolume = 1.5f;
                break;
        }
        SoundMod.Invoke(sound);
        sound._volume *= pvolume;
        sound.psource = FindOpenSource(sound, findexisting);
    }

    public AudioSource FindOpenSource(OXSound sound, bool findexisting = false)
    {
        if (sound._parent == null)
        {
            sound._parent = transform;
        }
        if (sound._pos != null)
        {
            var d = new GameObject("OXBetterOneShot");
            d.transform.position = sound._pos.Value;
            d.transform.parent = sound._parent;
            var sex2 = d.AddComponent<AudioSource>();
            sex2.clip = AudioClipDict[sound.name].Clip;
            var sex3 = d.AddComponent<OneShotAudioKiller>();
            sex3.nb = sex2;
            return sex2;
        }
        var keysToRemove = new List<Transform>();

        foreach (var kvp in AudioSources)
        {
            if (kvp.Key == null) // catches destroyed Unity objects
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            AudioSources.Remove(key);
        }
        if (findexisting)
        {
            foreach (var penis in AudioSources.GetOrDefine(sound._parent, new()))
            {
                if (penis.clip == AudioClipDict[sound.name].Clip)
                {
                    penis.clip = AudioClipDict[sound.name].Clip;
                    return penis;
                }
            }
        }
        foreach (var penis in AudioSources.GetOrDefine(sound._parent, new()))
        {
            if (!penis.isPlaying)
            {
                penis.clip = AudioClipDict[sound.name].Clip;
                return penis;
            }
        }
        var sex = gameObject.AddComponent<AudioSource>();
        sex.clip = AudioClipDict[sound.name].Clip;
        AudioSources.GetOrDefine(sound._parent, new()).Add(sex);
        return sex;
    }

    public float ChannelMult(string specific = "SFX")
    {
        float master = Volumes.GetOrDefine("Master", 1);
#if UNITY_EDITOR
        master *= DebugVolumeMult;
#endif
        if (specific == "") return master;
        return master * Volumes.GetValueOrDefault(specific, 1);
    }
    public OXSound PlaySound(OXSound sound)
    {
        ModSound(sound, sound._clipping);
        var p = sound.psource;
        p.outputAudioMixerGroup = sound._mixer;
        p.pitch = sound._pitch;
        p.panStereo = sound._pan;
        p.bypassEffects = sound._bypass;
        p.loop = sound._loop;
        p.priority = sound._priority;
        p.spatialBlend = sound._spaceblend;
        p.rolloffMode = sound._rolloffMode;
        p.minDistance = sound._rollofffactor;
        if (sound._rand_min != null)
        {
            p.pitch *= Random.Range(sound._rand_min.Value, sound._rand_max.Value);
        }
        sound.SetVolume(sound._volume);
        p.Play();
        return sound;
    }

}
[System.Serializable]
public class OXSoundData
{
    public string Name;
    public AudioClip Clip;
}
[System.Serializable]
public class OXMixerData
{
    public string Name;
    public AudioMixerGroup Mixer;
}
public class OXSound
{
    public string name;
    public AudioClip clip;
    public AudioSource psource;
    public AudioMixerGroup _mixer = null;
    public string _channel = "SFX";
    public float _pitch = 1;
    public float? _rand_min = null;
    public float? _rand_max = null;
    public Transform _parent = null;
    public float _volume = 1;
    public float _pan = 0;
    public float _spaceblend = 0;
    public byte _priority = 128;
    public bool _clipping = false;
    public bool _bypass = false;
    public bool _loop = false;
    public float _maxd = 0;
    public AudioRolloffMode _rolloffMode = AudioRolloffMode.Linear;
    public float _rollofffactor = 0;
    public Vector3? _pos = null;
    public OXSound(string name, float volume)
    {
        this.name = name;
        this._volume = volume;
    }
    public OXSound Pitch(float v)
    {
        _pitch = v;
        return this;
    }
    public OXSound Pan(float v)
    {
        _pan = v;
        return this;
    }
    public OXSound RandomPitch()
    {
        _rand_min = 0.95f;
        _rand_max = 1.05f;
        return this;
    }
    public OXSound RandomPitch(float min, float max)
    {
        _rand_min = min;
        _rand_max = max;
        return this;
    }
    public OXSound Channel(string s)
    {
        _channel = s;
        return this;
    }
    public OXSound Position(Vector3 v)
    {
        //for 2d games MAKE SURE THE [z] CORDINATE IS SET TO THE SAME AS THE CAMERA
        _pos = v;
        _spaceblend = 1;
        return this;
    }
    public OXSound Position(bool req, Vector3 v)
    {
        if (!req) return this;
        //for 2d games MAKE SURE THE [z] CORDINATE IS SET TO THE SAME AS THE CAMERA
        _pos = v;
        _spaceblend = 1;
        return this;
    }
    public OXSound Clipping()
    {
        _clipping = true;
        return this;
    }
    public OXSound BypassEffects()
    {
        _bypass = true;
        return this;
    }
    public OXSound Loop()
    {
        _loop = true;
        return this;
    }
    public OXSound MaxDistance(float x)
    {
        _maxd = x;
        return this;
    }
    public OXSound SpacialBlend(float x)
    {
        _spaceblend = x;
        return this;
    }

    public OXSound Rolloff(float factor, bool isLinear = false)
    {
        _rolloffMode = isLinear ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
        _rollofffactor = factor;
        return this;
    }
    public OXSound Priority(byte x)
    {
        _priority = x;
        return this;
    }
    public OXSound Parent(Transform x)
    {
        _parent = x;
        return this;
    }
    public OXSound Mixer(string x, bool AllowOverride = false)
    {
        if (!AllowOverride && _mixer != null) return this;
        _mixer = SoundSystem.Instance.AudioMixerDict[x];
        return this;
    }
    public OXSound Mixer(AudioMixerGroup x, bool AllowOverride = false)
    {
        if (!AllowOverride && _mixer != null) return this;
        _mixer = x;
        return this;
    }

    public void Stop()
    {
        psource.Stop();
    }

    public void SetVolume(float volume)
    {
        var vol = 1f;
        vol *= SoundSystem.Instance.ChannelMult();
        vol *= volume;
        psource.volume = vol;
    }
}

