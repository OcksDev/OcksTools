using System.Collections.Generic;
using UnityEngine;
using static ChessEngine;

public class ChessEngine
{
    public enum ChessTeam
    {
        White,
        Black,
    }
}

public class BoardState
{
    public List<ChessPieceBase> CurerentPieces = new List<ChessPieceBase>();
    public List<MultiRef<int, string>> History = new List<MultiRef<int, string>>();
    public int CurrentTurn = 0;
    public void AddPiece(ChessPieceBase nerd, Vector2Int pos)
    {
        nerd.Position = pos;
        nerd.Initialize();
        if (nerd.Name == "<Unset>") throw new System.Exception("Bro forgot to give the piece a name");
        RecordHistory(nerd);
    }
    public void RecordHistory(ChessPieceBase nerd)
    {
        History.Add(new MultiRef<int, string>(CurrentTurn, nerd.ToString()));
    }
    public bool IsOccupied(Vector2Int pos)
    {
        foreach (var piece in CurerentPieces)
        {
            if (piece.CanBlockMovementAt(this, pos)) return true;
        }
        return false;
    }
    public bool CanPieceTakeAtPos(ChessPieceBase piece, Vector2Int pos)
    {
        foreach (var p in CurerentPieces)
        {
            if (p.CanBeCapturedByAt(this, piece, pos)) return true;
        }
        return false;
    }
}


[System.Serializable]
public abstract class ChessPieceBase
{
    public string Name = "<Unset>";
    public Vector2Int Position;
    public ChessTeam Team;
    public List<ChessBoardVector> MoveVectors = new List<ChessBoardVector>();
    public abstract void Initialize();
    public virtual List<Vector2Int> GetAllValidMoves(BoardState state)
    {
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanMoveWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                if (state.IsOccupied(p)) break;
                if (!fin.Contains(p)) fin.Add(p);
            }
        }
        return fin;
    }
    public virtual List<Vector2Int> GetAllValidCaptures(BoardState state)
    {
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanCaptureWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                if (state.CanPieceTakeAtPos(this, p) && !fin.Contains(p))
                {
                    fin.Add(p);
                    break;
                }
            }
        }
        return fin;
    }
    public virtual bool CanMoveTo(BoardState state, Vector2Int pos)
    {
        return GetAllValidMoves(state).Contains(pos);
    }
    public virtual bool CanCaptureAt(BoardState state, Vector2Int pos)
    {
        return GetAllValidCaptures(state).Contains(pos);
    }
    public virtual bool CanBlockMovementAt(BoardState state, Vector2Int pos)
    {
        return pos == Position;
    }
    public virtual bool CanBeCapturedByAt(BoardState state, ChessPieceBase nerd, Vector2Int pos)
    {
        return nerd.Team != Team && pos == Position;
    }
    public virtual void MoveTo(Vector2Int pos)
    {
        Position = pos;
    }
    public virtual void Capture(ChessPieceBase nerd, Vector2Int pos)
    {
        Position = pos;
    }
    public override string ToString()
    {
        Dictionary<string, string> b = new Dictionary<string, string>
        {
            { "n", Name.ToString() },
            { "pos", Position.ToString() },
            { "t", Team.ToString() }
        };
        return b.DictionaryToString();
    }
}

public class ChessBoardVector
{
    public Vector2Int PositionRelative;
    public Vector2Int Direction;
    public bool CanMoveWith = true;
    public bool CanCaptureWith = true;
    public ChessBoardVector(Vector2Int position_relative, Vector2Int direction, bool canMoveWith = true, bool canCaptureWith = true)
    {
        PositionRelative = position_relative;
        Direction = direction;
        CanMoveWith = canMoveWith;
        CanCaptureWith = canCaptureWith;
    }
    public List<Vector2Int> TraverseVector()
    {
        List<Vector2Int> total = new List<Vector2Int>();
        int x = Direction.x == 0 ? 0 : (Direction.x > 0 ? 1 : -1);
        int y = Direction.y == 0 ? 0 : (Direction.y > 0 ? 1 : -1);
        int max = System.Math.Max(System.Math.Abs(Direction.x), System.Math.Abs(Direction.y));
        for (int i = 0; i < max; i++)
        {
            total.Add(PositionRelative + new Vector2Int(x * i, y * i));
        }
        return total;
    }
}