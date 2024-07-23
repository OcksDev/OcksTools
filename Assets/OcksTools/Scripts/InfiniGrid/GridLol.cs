using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.XR;

public class GridLol : MonoBehaviour
{
    public static GridLol Instance;
    public Tilemap Collidermap;
    public Tile collidertile;
    public List<TileObject> Tiles = new List<TileObject>();
    public GameObject TileToSpawn;
    public List<Color32> colors = new List<Color32>();
    private void Awake()
    {
        Instance = this;
        SaveSystem.SaveAllData += SaveAllTiles;
        SaveSystem.LoadAllData += LoadAllTiles;
    }

    public void Update()
    {
        if (InputManager.IsKeyDown(InputManager.gamekeys["shoot"]))
        {
            var z = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var zi = new Vector3Int(Mathf.RoundToInt(z.x), Mathf.RoundToInt(z.y), 0);
            var x = Collidermap.GetTile(zi);
            if(x != null)
            {
                for(int i = 0; i < Tiles.Count; i++)
                {
                    var a = Tiles[i];
                    if (a.data.pos == zi)
                    {
                        Tiles.Remove(a);
                        Destroy(a.gameObject);
                        break;
                    }
                }
                Collidermap.SetTile(zi, null);
            }
            else
            {
                var d = new OcksTileData(zi, Random.Range(0, colors.Count));
                SpawnTile(d);
            }
        }
    }
    public TileObject SpawnTile(OcksTileData zi)
    {
        var a = Instantiate(TileToSpawn, zi.pos, Quaternion.identity).GetComponent<TileObject>();
        Collidermap.SetTile(zi.pos, collidertile);
        Tiles.Add(a);
        a.data = zi;
        a.spriteRenderer.color = colors[zi.TileType];
        Debug.Log(zi.ToString());
        return a;
    }

    public void SaveAllTiles()
    {
        List<string> boners = new List<string>();
        foreach (var tile in Tiles)
        {
            boners.Add(tile.data.pos.ToString() + "|=|" + tile.data.TileType.ToString());
        }
        SaveSystem.Instance.SetString("InfiniGrid", Converter.ListToString(boners, "-=-"));
    }
    public void LoadAllTiles()
    {
        var a = Converter.StringToList(SaveSystem.Instance.GetString("InfiniGrid", ""),"-=-");
        foreach(var b in a)
        {
            if (b == "") continue;
            var c = Converter.StringToList(b, "|=|");
            var d = new OcksTileData(SpecializedStringToVector3Int(c[0]), int.Parse(c[1]));
            SpawnTile(d);
        }
    }

    public Vector3Int SpecializedStringToVector3Int(string e)
    {
        var s = Converter.StringToList(e.Substring(1, e.Length - 3));
        return new Vector3Int(int.Parse(s[0]), int.Parse(s[1]), 0);
    }


}


public class OcksTileData
{
    public Vector3Int pos;
    public int TileType;
    public OcksTileData()
    {

    }
    public OcksTileData(Vector3Int pos, int tileType)
    {
        this.pos = pos;
        TileType = tileType;
    }
}
