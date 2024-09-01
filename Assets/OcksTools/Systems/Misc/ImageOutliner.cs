using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageOutliner : MonoBehaviour
{
    public Screenshot ss;
    public Texture2D tex2;
    public Sprite sprit;
    public SpriteRenderer gaming;
    public Image gaming2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScreenshotImage();
        }
    }


    public void ScreenshotImage()
    {
        var tex = tex2;
        
        var colors = tex.GetPixels();
        for(int i = 0; i < colors.Length; i++)
        {
            if (colors[i].a >= 0.1f)
            {
                colors[i] = new Color(1,1,1,1);
            }
        }
        tex.SetPixels(colors);
        gaming2.sprite = Converter.Texture2DToSprite(tex2);
        float w = ((float)tex2.width) / 100f;
        float h = ((float)tex2.height) / 100f;
        gaming2.transform.localScale = new Vector3(w, h, 1);
        var e = new ScreenshotData();
        e.Camera = Camera.main;
        e.Camera.Reset();
        e.Height_PX = tex.height;
        e.Width_PX = tex.width;
        e.Name = "OutlinedImage";
        ss.TakeScreenshot(e);
    }
}
