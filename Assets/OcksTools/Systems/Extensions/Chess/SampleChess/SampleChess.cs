using System.Collections.Generic;
using UnityEngine;

public class SampleChess : SingleInstance<SampleChess>
{
    public CompileableDictionary<string, Sprite> ChessPieces = new();
    public BoardState b;
    public float piecescale = 69;
    private void Start()
    {
        ChessPieces.Compile();
        b = ChessEngine.SetUpDefaultMatch();
        piecescale = transform.localScale.x / 8;
        foreach (var a in b.CurrentPieces)
        {
            Debug.Log($"{a.Name}: {a.Position}");
            var c = SpawnSystem.Spawn(new SpawnData("Piece").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one));
            a._object = c;
            c.GetComponent<SpriteRenderer>().sprite = ChessPieces[a.Name + (a.Team == ChessEngine.ChessTeam.White ? "W" : "B")];
            c.GetComponent<SampleChessPiece>().me = a;
        }
    }
    private List<GameObject> markers = new();
    public void ClearMarkers()
    {
        foreach (var a in markers)
        {
            Destroy(a);
        }
        markers.Clear();
    }
    public void SelectPiece(ChessPieceBase nerd)
    {
        ClearMarkers();
        var m = nerd.GetAllValidMoves(b);
        foreach (var a in m)
        {
            var c = SpawnSystem.Spawn(new SpawnData("Move").Position(PosToWorld(a)).Scale(piecescale * Vector3.one * 0.3333f));
            markers.Add(c);
        }
        m = nerd.GetAllValidCaptures(b);
        foreach (var a in m)
        {
            var c = SpawnSystem.Spawn(new SpawnData("Capture").Position(PosToWorld(a)).Scale(piecescale * Vector3.one * 0.6f).Rotation(Quaternion.Euler(0, 0, 45)));
            markers.Add(c);
        }
    }

    public Vector3 PosToWorld(Vector2Int pos)
    {
        Vector3 sp = transform.position - transform.localScale / 2;
        sp += (Vector3)(piecescale / 2 * Vector2.one);
        sp.x += pos.x * piecescale;
        sp.y += pos.y * piecescale;
        return sp;
    }
}
