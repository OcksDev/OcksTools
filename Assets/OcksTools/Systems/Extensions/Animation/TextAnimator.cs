using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    private TMP_Text sexy;
    public List<TextAnim> anims = new List<TextAnim>();
    private List<List<Color32>> pp = new List<List<Color32>>();
    float halfpi;
    private void Start()
    {
        halfpi = Mathf.PI / 2;
        sexy = GetComponent<TMP_Text>();
        pp = new List<List<Color32>>()
        {
            new List<Color32>()
                            {
                                new Color32(255,0,0,255),
                                new Color32(255,255,0,255),
                                new Color32(0,255,0,255),
                                new Color32(0,255,255,255),
                                new Color32(50,50,255,255),
                                new Color32(255,0,255,255),
                                new Color32(255,0,0,255),
                            },
            new List<Color32>()
                            {
                                new Color32(255, 123, 0, 255),
                                new Color32(255, 0, 0, 255),
                                new Color32(255, 123, 0, 255),
                            },
        };
    }
    private void Update()
    {
        if(anims.Count > 0)
        {
            var delt = Time.deltaTime;
            sexy.ForceMeshUpdate();
            var textinfo = sexy.textInfo;
            var sexoff = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            for (int i = 0; i < textinfo.characterCount; i++)
            {
                var charinfo = textinfo.characterInfo[i];
                if (!charinfo.isVisible) continue;
                var verts = textinfo.meshInfo[charinfo.materialReferenceIndex].vertices;
                var coilo = textinfo.meshInfo[charinfo.materialReferenceIndex].colors32;
                float sex = 0;
                List<Color32> e;
                for (int g = 0; g < anims.Count; g++)
                {
                    if (i >= anims[g].startindex && i <= anims[g].endindex)
                    {


                        //Animations that play based on the time since a character has been visable use this
                        var sp = anims[g].SpottedCharacters;
                        switch (anims[g].Type)
                        {
                            case "FadeIn":
                            case "Gravity":
                            case "ZoomIn":
                            case "ShakeIn":
                            case "FloatDown":
                                if (!sp.ContainsKey(i))
                                {
                                    sp.Add(i, 0);
                                }
                                else
                                {
                                    sp[i] += delt;
                                }
                                break;
                        }

                        //general animation behavior
                        switch (anims[g].Type)
                        {
                            // i = character #
                            // j = vert #
                            // anims[g] = current anim
                            // sp[i] = percent loaded in on type
                            case "Wave":
                                for (int j = 0; j < 4; j++)
                                {
                                    var orig = verts[charinfo.vertexIndex + j];
                                    verts[charinfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * 3f + orig.x * 0.02f) * 6f, 0);
                                }
                                break;
                            case "WaveND":
                                for (int j = 0; j < 4; j++)
                                {
                                    var orig = verts[charinfo.vertexIndex + j];
                                    verts[charinfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * 3f + i * 0.2f) * 6f, 0);
                                }
                                break;
                            case "Shake":
                                sexoff = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                                for (int j = 0; j < 4; j++)
                                {
                                    var orig = verts[charinfo.vertexIndex + j];
                                    verts[charinfo.vertexIndex + j] = orig + (sexoff);
                                }
                                break;
                            case "ShakeWhole":
                                for (int j = 0; j < 4; j++)
                                {
                                    var orig = verts[charinfo.vertexIndex + j];
                                    verts[charinfo.vertexIndex + j] = orig + (sexoff);
                                }
                                break;
                            case "ShakeIn":
                                sp[i] += delt * 2;
                                if (sp[i] <= 1)
                                {
                                    sexoff = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * 3;
                                    for (int j = 0; j < 4; j++)
                                    {
                                        var orig = verts[charinfo.vertexIndex + j];
                                        verts[charinfo.vertexIndex + j] = orig + (sexoff) * (1 - sp[i]);
                                    }
                                }
                                break;
                            case "Rainbow":
                                e = pp[0];
                                sex = (Time.time + (i * 0.1f)) % (e.Count - 1);
                                for (int j2 = 0; j2 < 4; j2++)
                                {
                                    coilo[charinfo.vertexIndex + j2] = Color32.Lerp(e[(int)sex], e[(int)sex + 1], sex % 1);
                                }
                                break;
                            case "RainbowSnap":
                                e = pp[0];
                                sex = ((Time.time + i * 0.25f) * 4) % (e.Count - 1);
                                for (int j2 = 0; j2 < 4; j2++)
                                {
                                    coilo[charinfo.vertexIndex + j2] = e[(int)sex];
                                }
                                break;
                            case "Halloween":
                                e = pp[1];
                                sex = (Time.time + (i * 0.1f)) % (e.Count - 1);
                                for (int j2 = 0; j2 < 4; j2++)
                                {
                                    coilo[charinfo.vertexIndex + j2] = Color32.Lerp(e[(int)sex], e[(int)sex + 1], sex % 1);
                                }
                                break;
                            case "FloatDown":
                                sp[i] += delt;
                                if (sp[i] <= 1)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        var orig = verts[charinfo.vertexIndex + j];
                                        verts[charinfo.vertexIndex + j] = orig + new Vector3(0, 6 * (Mathf.Cos((halfpi * sp[i]) + halfpi) + 1));
                                    }
                                }
                                break;
                            case "ZoomIn":
                                sp[i] += delt*2;
                                if (sp[i] <= 1)
                                {
                                    var avg = Vector3.Lerp(verts[charinfo.vertexIndex + 0], verts[charinfo.vertexIndex + 2], 0.5f);
                                    for (int j = 0; j < 4; j++)
                                    {
                                        var orig = verts[charinfo.vertexIndex + j];
                                        verts[charinfo.vertexIndex + j] = avg + ((orig - avg) * sp[i]);
                                    }
                                }
                                break;
                            case "FadeIn":
                                sp[i] += delt*3;
                                for (int j2 = 0; j2 < 4; j2++)
                                {
                                    var ween = (Color)coilo[charinfo.vertexIndex + j2];
                                    ween.a = sp[i];
                                    coilo[charinfo.vertexIndex + j2] = ween;
                                }
                                break;
                            case "Gravity":
                                sp[i] += delt;
                                for (int j = 0; j < 4; j++)
                                {
                                    var orig = verts[charinfo.vertexIndex + j];
                                    verts[charinfo.vertexIndex + j] = orig + new Vector3(0, -(sp[i] * sp[i])*90, 0);
                                }
                                break;
                        }
                    }
                }
            }
            for (int i = 0; i < textinfo.meshInfo.Length; i++)
            {
                var mi = textinfo.meshInfo[i];
                mi.mesh.vertices = mi.vertices;
                mi.mesh.colors32 = mi.colors32;
                sexy.UpdateGeometry(mi.mesh, i);
            }
        }
    }

}

public class TextAnim
{
    public string Type;
    public int startindex;
    public int endindex;
    public Dictionary<int, float> SpottedCharacters = new Dictionary<int, float>();
    public TextAnim() { }
    public TextAnim(string type, int start, int end)
    {
        Type = type;
        startindex = start;
        endindex = end;
    }

}