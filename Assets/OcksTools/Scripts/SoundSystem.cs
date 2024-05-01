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
    public List<AudioClip> AudioClips = new List<AudioClip>();
    private List<AudioSource> AudioSources = new List<AudioSource>();
    public static SoundSystem Instance
    {
        get { return instance; }
    }   

    private void Awake()
    {
        if (Instance == null) instance = this;
    }
    private void Start()
    {

    }

    public void ModSound(int sound, bool findexisting = false)
    {
        int k = 0;
        pvolume = 1;
        int the_sound = sound;
        switch (sound)
        {
            case 0:
                pvolume = SFXVolume;
                /*
                k = Random.Range(0, 2);
                switch (k)
                {
                    case 0:
                        pvolume *= 1.2f;
                        the_sound = 0;
                        break;
                    case 1:
                        the_sound = 1;
                        break;
                    case 2:
                        the_sound = 2;
                        break;
                }*/
                break;
            case 3:
                pvolume = SFXVolume * 1.2f;
                break;
            case 4:
                pvolume = SFXVolume * 1.5f;
                break;
        }

        psource = FindOpenSource(the_sound, findexisting);
    }

    public AudioSource FindOpenSource(int index, bool findexisting = false)
    {
        if (findexisting)
        {
            foreach (var penis in AudioSources)
            {
                if (penis.clip == AudioClips[index])
                {
                    penis.clip = AudioClips[index];
                    return penis;
                }
            }
        }
        foreach (var penis in AudioSources)
        {
            if (!penis.isPlaying)
            {
                penis.clip = AudioClips[index];
                return penis;
            }
        }
        var sex = gameObject.AddComponent<AudioSource>();
        sex.clip = AudioClips[index];
        AudioSources.Add(sex);
        return sex;
    }


    private AudioSource psource;
    private float pvolume;

    public void PlaySound(int sound, bool randompitch = false, float volumes = 1f, float pitchmod = 1f)
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

    public void PlaySoundWithClipping(int sound, bool randompitch = false, float volumes = 1f, float pitchmod = 1f)
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
    public void PlaySound(int sound, Vector3 pos, bool randompitch = false, float volume = 1f, float pitchmod = 1f)
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
