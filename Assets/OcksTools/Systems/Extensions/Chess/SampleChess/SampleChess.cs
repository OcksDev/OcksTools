using System.Collections.Generic;
using UnityEngine;

public class SampleChess : SingleInstance<SampleChess>
{
    public CompileableDictionary<Sprite> ChessPieces = new();
    public ChessBoard b;
    public ChessTeam my_team = ChessTeam.White;
    [HideInInspector]
    public float piecescale = 69;
    private void Start()
    {
        ChessPieces.Compile();
        //set pieces
        b = Chess_DefaultBoard.MakeDefaultBoard();

        b.StartGame_2Player();
        piecescale = transform.localScale.x / 8;
        foreach (var a in b.CurrentPieces)
        {
            Debug.Log($"{a.Name}: {a.Position}");
            var c = SpawnSystem.Spawn(new SpawnData("Piece").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one));
            a.WorldObject = c;
            c.GetComponent<SpriteRenderer>().sprite = ChessPieces[a.Name + (a.Team == ChessTeam.White ? "W" : "B")];
            c.GetComponent<SampleChessPiece>().me = a;
            a.OnMoveEvent.Append((x, y) => x.WorldObject.transform.position = PosToWorld(x.Position));
            a.OnDestroyEvent.Append((x, y) => Destroy(x.WorldObject));
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
        if (nerd.Team != b.CurrentTeam) return;
        ClearMarkers();
        var m = nerd.GetLegalMoves();
        foreach (var a in m)
        {
            if (a.Piece != null) continue;
            var c = SpawnSystem.Spawn(new SpawnData("Move").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one * 0.3333f));
            markers.Add(c);
            c = SpawnSystem.Spawn(new SpawnData("MoveArea").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one));
            markers.Add(c);
            var p = c.GetComponent<SampleChess_Move>();
            p.me = nerd;
            p.Mypos = a.Position;
            p.cap = false;
        }
        foreach (var a in m)
        {
            if (a.Piece == null) continue;
            var c = SpawnSystem.Spawn(new SpawnData("Capture").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one * 0.6f).Rotation(Quaternion.Euler(0, 0, 45)));
            markers.Add(c);
            c = SpawnSystem.Spawn(new SpawnData("MoveArea").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one));
            markers.Add(c);
            var p = c.GetComponent<SampleChess_Move>();
            p.me = nerd;
            p.Mypos = a.Position;
            p.cap = true;
        }
    }
    public void DelectPiece()
    {
        ClearMarkers();
    }
    public void SelectMove(SampleChess_Move m)
    {
        var pp = m.me;
        var flags = b.MovePiece(m.me, m.Mypos);
        DelectPiece();
        if (flags.Contains("check"))
        {
            SoundSystem.Instance.PlaySound(new OXSound("Check", 1));
        }
        else if (flags.Contains("capture"))
        {
            SoundSystem.Instance.PlaySound(new OXSound("Capture", 1));
        }
        else
        {
            if (b.CurrentTeam == my_team)
            {
                SoundSystem.Instance.PlaySound(new OXSound("MoveSelf", 1));
            }
            else
            {
                SoundSystem.Instance.PlaySound(new OXSound("MoveOpponent", 1));
            }
        }

        b.AdvanceTurn();
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
