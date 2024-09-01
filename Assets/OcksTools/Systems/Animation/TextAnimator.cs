using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    private TMP_Text sex;
    public List<TextAnim> anims = new List<TextAnim>();
    private List<List<Color32>> pp = new List<List<Color32>>();
    private void Start()
    {
        sex = GetComponent<TMP_Text>();
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
            sex.ForceMeshUpdate();
            var textinfo = sex.textInfo;
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
                        switch (anims[g].Type)
                        {
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
                        }
                    }
                }
            }
            for (int i = 0; i < textinfo.meshInfo.Length; i++)
            {
                var mi = textinfo.meshInfo[i];
                mi.mesh.vertices = mi.vertices;
                mi.mesh.colors32 = mi.colors32;
                sex.UpdateGeometry(mi.mesh, i);
            }
        }
    }

}

public class TextAnim
{
    public string Type;
    public int startindex;
    public int endindex;
    public TextAnim() { }
    public TextAnim(string type, int start, int end)
    {
        Type = type;
        startindex = start;
        endindex = end;
    }

}