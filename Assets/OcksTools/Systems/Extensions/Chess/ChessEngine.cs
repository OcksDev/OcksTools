using System.Collections.Generic;
using UnityEngine;
using static ChessEngine;

public class ChessEngine
{
    public enum ChessTeam
    {
        White,
        Black,
        Red, // 4-player white equiv
        Blue, // 4-player left
        Yellow, // 4-player black equiv
        Green, // 4-player right
    }
}

public class BoardState
{
    public List<ChessPieceBase> CurrentPieces = new List<ChessPieceBase>();
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
        foreach (var piece in CurrentPieces)
        {
            if (piece.CanBlockMovementAt(this, pos)) return true;
        }
        return false;
    }
    public bool IsGameInCheck()
    {
        return false;
    }
    public ChessPieceBase GetPieceAtPos(Vector2Int pos)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.Position == pos) return piece;
        }
        return null;
    }
    public void SetPieceAtPos(ChessPieceBase newpiece, Vector2Int pos)
    {
        var existing = GetPieceAtPos(pos);
        if (existing != null)
        {
            CurrentPieces.Remove(existing);
        }
        AddPiece(newpiece, pos);

    }
    public void MovePiece(ChessPieceBase piece, Vector2Int newpos)
    {
        piece.MoveTo(this, newpos);
    }
    public void CapturePiece(ChessPieceBase piece, ChessPieceBase takes, Vector2Int newpos)
    {
        piece.Capture(this, takes, newpos);
    }
    public bool CanPieceTakeAtPos(ChessPieceBase piece, Vector2Int pos)
    {
        foreach (var p in CurrentPieces)
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
    public int MovesMade = 0;
    public int TurnLastMovedOn = 0;
    public Vector2Int Position;
    public ChessTeam Team;
    public List<ChessBoardVector> MoveVectors = new List<ChessBoardVector>();
    /// <summary>
    /// Sets the unique name and move vectors
    /// </summary>
    public abstract void Initialize();

    public Vector2Int TeamRotation(Vector2Int pos)
    {
        if (Team == ChessTeam.Black) return -pos;
        if (Team == ChessTeam.Yellow) return -pos;
        if (Team == ChessTeam.Blue)
        {
            var h = pos.x;
            pos.x = pos.y;
            pos.y = -h;
            return pos;
        }
        if (Team == ChessTeam.Green)
        {
            var h = pos.x;
            pos.x = -pos.y;
            pos.y = h;
            return pos;
        }
        return pos;
    }


    public virtual List<Vector2Int> GetAllValidMoves(BoardState state, Vector2Int Position)
    {
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanMoveWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                var p2 = Position + TeamRotation(p);
                if (state.IsOccupied(p2)) break;
                if (!fin.Contains(p2)) fin.Add(p2);
            }
        }
        return fin;
    }
    public virtual List<Vector2Int> GetAllValidCaptures(BoardState state, Vector2Int Position)
    {
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanCaptureWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                var p2 = Position + TeamRotation(p);
                if (state.CanPieceTakeAtPos(this, p2) && !fin.Contains(p2))
                {
                    fin.Add(p2);
                    break;
                }
            }
        }
        return fin;
    }
    public virtual bool CanMoveTo(BoardState state, Vector2Int pos)
    {
        return CanMoveToFrom(state, pos, Position);
    }
    public virtual bool CanCaptureAt(BoardState state, Vector2Int pos)
    {
        return CanCaptureAtFrom(state, pos, Position);
    }
    public virtual bool CanMoveToFrom(BoardState state, Vector2Int pos, Vector2Int Position)
    {
        return GetAllValidMoves(state, Position).Contains(pos);
    }
    public virtual bool CanCaptureAtFrom(BoardState state, Vector2Int pos, Vector2Int Position)
    {
        return GetAllValidCaptures(state, Position).Contains(pos);
    }
    public virtual bool CanBlockMovementAt(BoardState state, Vector2Int pos)
    {
        return pos == Position;
    }
    public virtual bool CanBeCapturedByAt(BoardState state, ChessPieceBase nerd, Vector2Int pos)
    {
        return nerd.Team != Team && pos == Position;
    }
    public virtual void MoveTo(BoardState state, Vector2Int pos)
    {
        MovesMade++;
        Position = pos;
    }
    public virtual void Capture(BoardState state, ChessPieceBase nerd, Vector2Int pos)
    {
        MovesMade++;
        Position = pos;
        state.CurrentPieces.Remove(nerd);
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

    public ChessPieceBase SetTeam(ChessTeam t)
    {
        Team = t;
        return this;
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