
using System.Collections;
using UnityEngine;

public class TextOXFileStuff : MonoBehaviour
{
    public Sprite CoolImage;
    public AudioClip CoolSound;
    private AudioSource a;
    private SpriteRenderer b;
    private IEnumerator Start()
    {
        a = gameObject.AddComponent<AudioSource>();
        b = GetComponent<SpriteRenderer>();
        var x = new OXFile();
        x.Data.Add("img", CoolImage);
        x.Data.Add("snd", CoolSound);
        string path = FileSystem.Instance.GameDirectory + "/ImgAndAudio.ox";
        x.WriteFile(path, true);

        yield return new WaitForSeconds(1.5f);
        var ox = new OXFile();
        ox.ReadFile(path);
        b.sprite = ox.Data["img"].DataSprite;
        a.clip = ox.Data["snd"].DataSound;
        a.Play();
    }
}
