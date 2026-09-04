using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleChess : SingleInstance<SampleChess>
{
    public CompileableDictionary<Sprite> ChessPieces = new();
    public ChessBoard_Default b;
    public ChessTeam my_team = ChessTeam.White;
    [HideInInspector]
    public float piecescale = 69;
    private void Start()
    {
        ChessPieces.Compile();
        //set pieces
        b = new ChessBoard_Default();
        piecescale = transform.localScale.x / 8;
        b.OnPieceAddedEvent.Append("m", (a) =>
        {
            var c = SpawnSystem.Spawn(new SpawnData("Piece").Position(PosToWorld(a.Position)).Scale(piecescale * Vector3.one));
            a.WorldObject = c;
            c.GetComponent<SpriteRenderer>().sprite = ChessPieces[a.Name + (a.Team == ChessTeam.White ? "W" : "B")];
            c.GetComponent<SampleChessPiece>().me = a;
            a.OnMoveEvent.Append((x, y) => StartCoroutine(MovePieceAnimation(x, PosToWorld(y), PosToWorld(x.Position))));
            a.OnDestroyEvent.Append((x, y) => Destroy(x.WorldObject));
        });

        b.Make();
        b.StartGame_2Player();
    }

    public IEnumerator MovePieceAnimation(ChessPieceBase piece, Vector3 oldpos, Vector3 newpos)
    {
        var m = Mathf.Pow((oldpos - newpos).magnitude, 0.33f);
        yield return OXLerp.Frame.Linear((x) =>
        {
            if (piece.WorldObject == null) return;
            x = Ease.In(x);
            piece.WorldObject.transform.position = Vector3.Lerp(oldpos, newpos, x);
        }, 0.15f * m);
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
            if (a.Flags.Contains("capture")) continue;
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
            if (!a.Flags.Contains("capture")) continue;
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
    ChessPieceBase oldking = null;
    public void SelectMove(SampleChess_Move m)
    {
        if (oldking != null)
        {
            oldking.WorldObject.GetComponent<SampleChessPiece>().cr.gameObject.SetActive(false);
            oldking = null;
        }
        var pp = m.me;
        var flags = b.MovePiece(m.me, m.Mypos);
        DelectPiece();
        if (flags.Contains("check"))
        {
            SoundSystem.Instance.PlaySound(new OXSound("Check", 1));
        }
        else if (flags.Contains("promotion"))
        {
            SoundSystem.Instance.PlaySound(new OXSound("Promote", 1));
        }
        else if (flags.Contains("castle"))
        {
            SoundSystem.Instance.PlaySound(new OXSound("Castle", 1));
        }
        if (flags.Contains("capture"))
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

        var d = b.IsTeamInCheck(b.CurrentTeam);
        if (d.valid)
        {
            d.king.WorldObject.GetComponent<SampleChessPiece>().cr.gameObject.SetActive(true);
            oldking = d.king;
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
