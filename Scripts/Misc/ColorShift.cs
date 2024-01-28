using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorShift : MonoBehaviour
{
    public byte speed = 1;
    public int style = 0;
    private SpriteRenderer boner;
    public int state = 0;
    // Start is called before the first frame update
    void Start()
    {
        boner = GetComponent<SpriteRenderer>();
        state = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var f = boner.color;
        var game = Time.time * speed;
        var g = f.a;
        switch (style)
        {
            case 0:
                f = Color.Lerp(Color.white, Color.black, Mathf.PingPong(game, 1));
                break;
            case 1:
                float time = 1f;
                float cycles = 6f;
                var b = (game + time) % time;
                var c = (game / time) % cycles;
                var c1 = Color.white;
                var c2 = Color.white;
                state = Mathf.FloorToInt(c);
                switch (state)
                {
                    case 0:
                        c1 = new Color32(255, 0, 0, 255);
                        c2 = new Color32(255, 255, 0, 255);
                        break;
                    case 1:
                        c1 = new Color32(255, 255, 0, 255);
                        c2 = new Color32(0, 255, 0, 255);
                        break;
                    case 2:
                        c1 = new Color32(0, 255, 0, 255);
                        c2 = new Color32(0, 255, 255, 255);
                        break;
                    case 3:
                        c1 = new Color32(0, 255, 255, 255);
                        c2 = new Color32(0, 0, 255, 255);
                        break;
                    case 4:
                        c1 = new Color32(0, 0, 255, 255);
                        c2 = new Color32(255, 0, 255, 255);
                        break;
                    case 5:
                        c1 = new Color32(255, 0, 255, 255);
                        c2 = new Color32(255, 0, 0, 255);
                        break;
                }
                f = Color.Lerp(c1, c2, b);
                break;
            case 2:
                float time2 = 1f;
                float cycles2 = 6f;
                var b2 = (game + time2) % time2;
                var c22 = (game / time2) % cycles2;
                var c12 = Color.white;
                state = Mathf.FloorToInt(c22);
                switch (state)
                {
                    case 0:
                        c12 = new Color32(255, 0, 0, 255);
                        break;
                    case 1:
                        c12 = new Color32(255, 255, 0, 255);
                        break;
                    case 2:
                        c12 = new Color32(0, 255, 0, 255);
                        break;
                    case 3:
                        c12 = new Color32(0, 255, 255, 255);
                        break;
                    case 4:
                        c12 = new Color32(0, 0, 255, 255);
                        break;
                    case 5:
                        c12 = new Color32(255, 0, 255, 255);
                        break;
                }
                f = Color.Lerp(c12, Color.black, b2);
                break;
        }
        f.a = g;
        boner.color = f;
    }
}
