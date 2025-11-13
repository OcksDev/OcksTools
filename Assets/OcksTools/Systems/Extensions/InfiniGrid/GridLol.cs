using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridLol : MonoBehaviour
{
    public bool UseTilemapExtension = false;
    public float GeneralScale = 1f;
    public int XPlaceSize = 1;
    public int YPlaceSize = 1;
    public int LayerPlace = 0;
    public static GridLol Instance;
    public GameObject Highligher;
    public Color32[] HighligherColors;
    public Dictionary<Vector3Int, OcksTileData> Tiles = new Dictionary<Vector3Int, OcksTileData>();
    public GameObject TileToSpawn;
    public List<Color32> colors = new List<Color32>();
    private GridTileMapExtension tileextend;
    private void Awake()
    {
        Instance = this;
        SaveSystem.SaveAllData.Append("SaveAllTiles", SaveAllTiles);
        SaveSystem.LoadAllData.Append("LoadAllTiles", LoadAllTiles);
        highlighrt = Instantiate(Highligher, transform).GetComponent<SpriteRenderer>();
        if(UseTilemapExtension) tileextend = GetComponent<GridTileMapExtension>();
    }
    private SpriteRenderer highlighrt;
    public void Update()
    {
        var z = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        z /= GeneralScale;
        var zi = new Vector3Int(Mathf.RoundToInt(z.x - ((XPlaceSize - 1) / 2f)), Mathf.RoundToInt(z.y - ((YPlaceSize - 1) / 2f)), LayerPlace);

        var zi2 = new Vector3Int(Mathf.RoundToInt(z.x), Mathf.RoundToInt(z.y), LayerPlace);
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
        if (InputManager.IsKeyDown("shoot"))
        {
            if (!antiplace)
            {
                if (destroya)
                {
                    foreach (var b in x.GetUsedTiles())
                    {
                        Tiles.Remove(b);
                    }
                    if (UseTilemapExtension)
                    {

                        tileextend.TileMap.SetTile(x.pos, null);
                    }
                    else
                    {
                        Destroy(x.tob.gameObject);
                    }
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
        var aa = (zi.pos + new Vector3((zi.size.x - 1) / 2f, (zi.size.y - 1) / 2f, 1)) * GeneralScale;
        aa.z *= -1;
        return aa;
    }
    Vector3 GetScaleOfScaled(OcksTileData zi)
    {
        return new Vector3(zi.size.x, zi.size.y, 1) * GeneralScale;
    }

    public void SpawnTile(OcksTileData zi)
    {
        if (UseTilemapExtension)
        {
            tileextend.TileMap.SetTile(zi.pos, tileextend.TileToSpawn);
            tileextend.TileMap.SetTileFlags(zi.pos, UnityEngine.Tilemaps.TileFlags.None);
            tileextend.TileMap.SetColor(zi.pos, colors[zi.TileType]);
            foreach (var b in zi.GetUsedTiles())
            {
                Tiles.Add(b, zi);
            }
        }
        else
        {
            var a = Instantiate(TileToSpawn, GetPosOfScaled(zi), Quaternion.identity, transform).GetComponent<TileObject>();
            a.data = zi;
            a.data.tob = a;
            foreach (var b in zi.GetUsedTiles())
            {
                Tiles.Add(b, zi);
            }
            a.spriteRenderer.color = colors[zi.TileType];
            a.transform.localScale = GetScaleOfScaled(zi);
        }
    }

    public void SaveAllTiles(string dict)
    {
        Dictionary<OcksTileData, int> fuckyou = new Dictionary<OcksTileData, int>();
        List<string> boners = new List<string>();
        foreach (var tile in Tiles)
        {
            if (fuckyou.ContainsKey(tile.Value)) continue;
            fuckyou.Add(tile.Value, 0);
            boners.Add(tile.Value.TileToString());
        }
        SaveSystem.Instance.SetList("InfiniGrid", boners, dict);
    }
    public void LoadAllTiles(string dict)
    {
        var a = SaveSystem.Instance.GetList("InfiniGrid", new List<string>(), dict);
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
    Dictionary<string,string> data = new Dictionary<string,string>();
    public OcksTileData()
    {
        data = GetDataDict();
    }
    public OcksTileData(Vector3Int pos, Vector2Int size, int tileType)
    {
        data = GetDataDict();
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
                e[(i * size.y) + j] = new Vector3Int(pos.x + i, pos.y + j, pos.z);
            }
        }
        return e;
    }

    public string TileToString()
    {
        Dictionary<string, string> boner = new Dictionary<string, string>();
        var defaul = GetDataDict();
        data["pos"] = pos.ToString();
        data["size"] = size.ToString();
        data["type"] = TileType.ToString();
        foreach(var a in data)
        {
            if ((!defaul.ContainsKey(a.Key)) || (defaul[a.Key] != a.Value))
            {
                boner.Add(a.Key, a.Value);
            }
        }
        return Converter.DictionaryToString(boner);
    }
    public void StringToTile(string e)
    {
        data = GetDataDict();
        var c = Converter.StringToDictionary(e);
        foreach(var k in c)
        {
            if (data.ContainsKey(k.Key))
            {
                data[k.Key] = k.Value;
            }
            else
            {
                data.Add(k.Key,k.Value);
            }
        }

        pos = Converter.StringToVector3Int(data["pos"]);
        size = Converter.StringToVector2Int(data["size"]);
        TileType = int.Parse(data["type"]);
    }

    public Dictionary<string,string> GetDataDict()
    {
        return new Dictionary<string, string>()
        {
            {"pos","(0, 0, 0)"},
            {"size","(1, 1)"},
            {"type","0"},
        };
    }


}
