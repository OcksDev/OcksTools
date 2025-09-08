using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{



    //Will be fixed/changed in the future



    private static SoundSystem instance;

    public float MasterVolume = 1;
    public float SFXVolume = 1;
    public float MusicVolume = 1;
    public List<OXSoundData> AudioClips = new List<OXSoundData>();
    public Dictionary<string, OXSoundData> AudioClipDict = new Dictionary<string, OXSoundData>();
    private List<AudioSource> AudioSources = new List<AudioSource>();
    public static SoundSystem Instance
    {
        get { return instance; }
    }   

    public OXEvent SoundMod = new OXEvent();
    public OXEvent SoundDictCompile = new OXEvent();

    private void Awake()
    {
        foreach (var s in AudioClips)
        {
            AudioClipDict.Add(s.Name, s);
        }
        if (Instance == null) instance = this;
    }
    private void Start()
    {
        SoundDictCompile.Invoke();
    }
    public void AddSound(OXSoundData s)
    {
        AudioClipDict.Add(s.Name, s);
    }
    public void ModSound(OXSound sound, bool findexisting = false)
    {
        float pvolume = 1;
        switch (sound.name)
        {
            case "sound":
                pvolume = SFXVolume;
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
                pvolume = SFXVolume * 1.2f;
                break;
            case "B":
                pvolume = SFXVolume * 1.5f;
                break;
        }
        SoundMod.Invoke();
        sound.volume *= pvolume;
        if(sound.pos == null) sound.psource = FindOpenSource(sound, findexisting);
    }

    public AudioSource FindOpenSource(OXSound sound, bool findexisting = false)
    {
        if (findexisting)
        {
            foreach (var penis in AudioSources)
            {
                if (penis.clip == AudioClipDict[sound.name].Clip)
                {
                    penis.clip = AudioClipDict[sound.name].Clip;
                    return penis;
                }
            }
        }
        foreach (var penis in AudioSources)
        {
            if (!penis.isPlaying)
            {
                penis.clip = AudioClipDict[sound.name].Clip;
                return penis;
            }
        }
        var sex = gameObject.AddComponent<AudioSource>();
        sex.clip = AudioClipDict[sound.name].Clip;
        AudioSources.Add(sex);
        return sex;
    }


    public OXSound PlaySound(OXSound sound)
    {
        ModSound(sound,sound.clipping);
        var volume = 1f;
        var p = sound.psource;
        p.pitch = sound.pitch;
        p.panStereo = sound.pan;
        p.bypassEffects = sound.bypass;
        p.loop = sound.loop;
        if (sound.rand_min != null)
        {
            p.pitch *= Random.Range(sound.rand_min.Value, sound.rand_max.Value);
        }
        volume *= MasterVolume;
        volume *= sound.volume;
        p.volume = volume;
        if(sound.pos != null)
        {
            AudioSource.PlayClipAtPoint(p.clip, sound.pos.Value, volume);
        }
        else
        {
            p.Play();
        }
        return sound;
    }

    /*public void PlaySound(string sound, Vector3 pos, bool randompitch = false, float volume = 1f, float pitchmod = 1f)
    {
        ModSound(sound);
        var p = psource;
        p.pitch = 1f;
        p.pitch *= pitchmod;
        if (randompitch)
        {
            p.pitch *= Random.Range(.7f, 1.3f);
        }
        pvolume *= MasterVolume;
        pvolume *= volume;
        AudioSource.PlayClipAtPoint(p.clip, pos, pvolume);
    }*/

}
[System.Serializable]
public class OXSoundData
{
    public string Name;
    public AudioClip Clip;
}
public class OXSound
{
    public string name;
    public AudioClip clip;
    public AudioSource psource;
    public float pitch = 1;
    public float? rand_min;
    public float? rand_max;
    public float volume = 1;
    public float pan = 0;
    public bool clipping = false;
    public bool bypass = false;
    public bool loop = false;
    public float maxd = 0;
    public Vector3? pos;
    public OXSound(string name, float volume)
    {
        this.name = name;
        this.volume = volume;
    }
    public OXSound Pitch(float v)
    {
        pitch = v;
        return this;
    }
    public OXSound Pan(float v)
    {
        pan = v;
        return this;
    }
    public OXSound RandomPitch(float min,float max)
    {
        rand_min = min;
        rand_max = max;
        return this;
    }
    public OXSound Position(Vector3 v)
    {
        //for 2d games MAKE SURE THE [z] CORDINATE IS SET TO THE SAME AS THE CAMERA
        pos = v;
        return this;
    }
    public OXSound Clipping()
    {
        clipping = true;
        return this;
    }
    public OXSound BypassEffects()
    {
        bypass = true;
        return this;
    }
    public OXSound Loop()
    {
        loop = true;
        return this;
    }
    public OXSound MaxDistance(float x)
    {
        maxd = x;
        return this;
    }

}

