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

    }

    public void ModSound(string sound, bool findexisting = false)
    {
        pvolume = 1;
        string the_sound = sound;
        switch (sound)
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
                        the_sound = "new sound";
                        break;
                    case 2:
                        the_sound = "new sound AGAIN";
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

        psource = FindOpenSource(the_sound, findexisting);
    }

    public AudioSource FindOpenSource(string index, bool findexisting = false)
    {
        if (findexisting)
        {
            foreach (var penis in AudioSources)
            {
                if (penis.clip == AudioClipDict[index].Clip)
                {
                    penis.clip = AudioClipDict[index].Clip;
                    return penis;
                }
            }
        }
        foreach (var penis in AudioSources)
        {
            if (!penis.isPlaying)
            {
                penis.clip = AudioClipDict[index].Clip;
                return penis;
            }
        }
        var sex = gameObject.AddComponent<AudioSource>();
        sex.clip = AudioClipDict[index].Clip;
        AudioSources.Add(sex);
        return sex;
    }


    private AudioSource psource;
    private float pvolume;

    public void PlaySound(string sound, bool randompitch = false, float volumes = 1f, float pitchmod = 1f)
    {
        ModSound(sound);
        var volume = pvolume;
        var p = psource;
        p.pitch = 1f;
        p.pitch *= pitchmod;
        if (randompitch)
        {
            p.pitch *= Random.Range(.7f, 1.3f);
        }
        volume *= MasterVolume;
        volume *= volumes;
        p.volume = volume;
        p.Play();
    }

    public void PlaySoundWithClipping(string sound, bool randompitch = false, float volumes = 1f, float pitchmod = 1f)
    {
        ModSound(sound, true);
        var volume = pvolume;
        var p = psource;
        p.pitch = 1f;
        p.pitch *= pitchmod;
        if (randompitch)
        {
            p.pitch *= Random.Range(.7f, 1.3f);
        }
        volume *= MasterVolume;
        volume *= volumes;
        p.volume = volume;
        p.Play();
    }
    public void PlaySound(string sound, Vector3 pos, bool randompitch = false, float volume = 1f, float pitchmod = 1f)
    {
        //for 2d games MAKE SURE THE [z] CORDINATE IS SET TO THE SAME AS THE CAMERA
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
    }

}
[System.Serializable]
public class OXSoundData
{
    public string Name;
    public AudioClip Clip;
}

