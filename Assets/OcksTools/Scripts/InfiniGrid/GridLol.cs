using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using UnityEngine.XR;

public class GridLol : MonoBehaviour
{
    public float GeneralScale = 1f;
    public int XPlaceSize = 1;
    public int YPlaceSize = 1;
    public static GridLol Instance;
    public GameObject Highligher;
    public Color32[] HighligherColors;
    public Dictionary<Vector3Int, OcksTileData> Tiles = new Dictionary<Vector3Int, OcksTileData>();
    public GameObject TileToSpawn;
    public List<Color32> colors = new List<Color32>();
    private void Awake()
    {
        Instance = this;
        SaveSystem.SaveAllData += SaveAllTiles;
        SaveSystem.LoadAllData += LoadAllTiles;
        highlighrt = Instantiate(Highligher, transform).GetComponent<SpriteRenderer>();
    }
    private SpriteRenderer highlighrt;
    public void Update()
    {
        var z = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        z /= GeneralScale;
        var zi = new Vector3Int(Mathf.RoundToInt(z.x - ((XPlaceSize - 1) / 2f)), Mathf.RoundToInt(z.y - ((YPlaceSize - 1) / 2f)), 0);

        var zi2 = new Vector3Int(Mathf.RoundToInt(z.x), Mathf.RoundToInt(z.y), 0);
        bool destroya = false;
        bool antiplace = false;
        OcksTileData x = GetTile(zi2);
        var dummythicccheeks = new OcksTileData(zi, new Vector2Int(XPlaceSize, YPlaceSize), 0);
        //Debug.Log(x);
        if (x != null)
        {
            destroya = true;
        }
        else
        {
            foreach (var t in dummythicccheeks.GetUsedTiles())
            {
                if (t == zi2) continue;
                x = GetTile(t);
                if (x != null)
                {
                    antiplace = true;
                    break;
                }
            }
        }
        if (InputManager.IsKeyDown(InputManager.gamekeys["shoot"]))
        {
            if (!antiplace)
            {
                if (destroya)
                {
                    foreach (var b in x.GetUsedTiles())
                    {
                        Tiles.Remove(b);
                    }
                    Destroy(x.tob.gameObject);
                }
                else
                {
                    var d = new OcksTileData(zi, new Vector2Int(XPlaceSize, YPlaceSize), Random.Range(0, colors.Count));
                    SpawnTile(d);
                }
            }
        }
        highlighrt.color = HighligherColors[(antiplace||destroya)?0:1];
        highlighrt.transform.position = GetPosOfScaled(dummythicccheeks);
        highlighrt.transform.localScale = GetScaleOfScaled(dummythicccheeks);

    }

    public OcksTileData GetTile(Vector3Int pos)
    {
        if (Tiles.ContainsKey(pos))
        {
            return Tiles[pos];
        }
        else
        {
            return null;
        }
    }

    Vector3 GetPosOfScaled(OcksTileData zi)
    {
        return (zi.pos + new Vector3((zi.size.x - 1) / 2f, (zi.size.y - 1) / 2f, 1)) * GeneralScale;
    }
    Vector3 GetScaleOfScaled(OcksTileData zi)
    {
        return new Vector3(zi.size.x, zi.size.y, 1) * GeneralScale;
    }

    public TileObject SpawnTile(OcksTileData zi)
    {
        var a = Instantiate(TileToSpawn, GetPosOfScaled(zi), Quaternion.identity, transform).GetComponent<TileObject>();
        a.data = zi;
        a.data.tob = a;
        foreach(var b in zi.GetUsedTiles())
        {
            Tiles.Add(b, zi);
        }
        a.spriteRenderer.color = colors[zi.TileType];
        a.transform.localScale = GetScaleOfScaled(zi);
        return a;
    }

    public void SaveAllTiles()
    {
        Dictionary<OcksTileData, int> fuckyou = new Dictionary<OcksTileData, int>();
        List<string> boners = new List<string>();
        foreach (var tile in Tiles)
        {
            if (fuckyou.ContainsKey(tile.Value)) continue;
            fuckyou.Add(tile.Value, 0);
            boners.Add(tile.Value.TileToString());
        }
        SaveSystem.Instance.SetString("InfiniGrid", Converter.ListToString(boners, "-=-"));
    }
    public void LoadAllTiles()
    {
        var a = Converter.StringToList(SaveSystem.Instance.GetString("InfiniGrid", ""),"-=-");
        foreach(var b in a)
        {
            if (b == "") continue;
            var sex = new OcksTileData();
            sex.StringToTile(b);
            SpawnTile(sex);
        }
    }


}


public class OcksTileData
{
    public Vector3Int pos;
    public Vector2Int size = Vector2Int.one;
    public int TileType;
    public TileObject tob;
    public OcksTileData()
    {

    }
    public OcksTileData(Vector3Int pos, Vector2Int size, int tileType)
    {
        this.pos = pos;
        TileType = tileType;
        this.size = size;
    }

    public Vector3Int[] GetUsedTiles()
    {
        var e = new Vector3Int[size.x*size.y];
        for(int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                e[(i * size.y) + j] = new Vector3Int(pos.x + i, pos.y + j, 0);
            }
        }
        return e;
    }

    public string TileToString()
    {
        List<string> strings = new List<string>();
        strings.Add(pos.ToString());
        strings.Add(size.ToString());
        strings.Add(TileType.ToString());

        return Converter.ListToString(strings, "|=|");
    }
    public void StringToTile(string e)
    {
        var c = Converter.StringToList(e, "|=|");
        pos = SpecializedStringToVector3Int(c[0]);
        size = Converter.StringToVector2Int(c[1]);
        TileType = int.Parse(c[2]);
    }


    public Vector3Int SpecializedStringToVector3Int(string e)
    {
        //dont mind this method
        var s = Converter.StringToList(e.Substring(1, e.Length - 2));
        return new Vector3Int(int.Parse(s[0]), int.Parse(s[1]), 0);
    }


}
