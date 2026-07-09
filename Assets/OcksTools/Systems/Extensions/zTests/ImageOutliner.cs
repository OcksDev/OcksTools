using UnityEngine;
using UnityEngine.UI;

public class ImageOutliner : MonoBehaviour
{
    /*
     * VERY experimental thing going on here
     * the current issue is that it modifies the source file when changing pixels, which is not ideal.
     * Also despite the name, it currently just makes things entirely white, because I am working on the original problem first.
     * 
     * Images must have "read/writeable" be enabled and be in one of unity's supported formats (which you set in the image instead of automatic)
     * I already provided a preset for how images should be prepared.
     */



    public Screenshot ss;
    public Texture2D tex2;
    public Image gaming2;
    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScreenshotImage();
        }
    }


    public void ScreenshotImage()
    {
        var tex = tex2;
        float w = ((float)tex2.width) / 100f;
        w *= 600f / tex2.width;
        float h = ((float)tex2.height) / 100f;
        h *= 600f / tex2.height;
        gaming2.transform.localScale = new Vector3(w, h, 1);
        var e = new ScreenshotData("OriginalImage")
            .WidthPX(tex.width)
            .HeightPX(tex.height)
            .Camera(Camera.main);

        gaming2.sprite = Converter.Texture2DToSprite(tex2);
        ss.TakeScreenshot(e);

        var colors = tex.GetPixels32();

        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].a >= 200)
            {
                colors[i] = new Color32(255, 255, 255, 255);
            }
        }
        tex.SetPixels32(colors);
        tex.Apply();
        gaming2.sprite = Converter.Texture2DToSprite(tex);
        e._Name = "OutlinedImage";
        ss.TakeScreenshot(e);
    }
}
