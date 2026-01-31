using System.Collections.Generic;
using UnityEngine;
using static ChessEngine;

public class ChessEngine
{
    public static BoardState SetUpDefaultMatch()
    {
        BoardState board = new Chess_DefaultBoard();

        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(0, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(1, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(2, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(3, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(4, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(5, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(6, 1));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.White), new Vector2Int(7, 1));
        board.AddPiece(new Chess_King().SetTeam(ChessTeam.White), new Vector2Int(4, 0));
        board.AddPiece(new Chess_Queen().SetTeam(ChessTeam.White), new Vector2Int(3, 0));
        board.AddPiece(new Chess_Bishop().SetTeam(ChessTeam.White), new Vector2Int(2, 0));
        board.AddPiece(new Chess_Bishop().SetTeam(ChessTeam.White), new Vector2Int(5, 0));
        board.AddPiece(new Chess_Knight().SetTeam(ChessTeam.White), new Vector2Int(1, 0));
        board.AddPiece(new Chess_Knight().SetTeam(ChessTeam.White), new Vector2Int(6, 0));
        board.AddPiece(new Chess_Rook().SetTeam(ChessTeam.White), new Vector2Int(0, 0));
        board.AddPiece(new Chess_Rook().SetTeam(ChessTeam.White), new Vector2Int(7, 0));

        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(0, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(1, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(2, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(3, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(4, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(5, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(6, 6));
        board.AddPiece(new Chess_Pawn().SetTeam(ChessTeam.Black), new Vector2Int(7, 6));
        board.AddPiece(new Chess_King().SetTeam(ChessTeam.Black), new Vector2Int(4, 7));
        board.AddPiece(new Chess_Queen().SetTeam(ChessTeam.Black), new Vector2Int(3, 7));
        board.AddPiece(new Chess_Bishop().SetTeam(ChessTeam.Black), new Vector2Int(2, 7));
        board.AddPiece(new Chess_Bishop().SetTeam(ChessTeam.Black), new Vector2Int(5, 7));
        board.AddPiece(new Chess_Knight().SetTeam(ChessTeam.Black), new Vector2Int(1, 7));
        board.AddPiece(new Chess_Knight().SetTeam(ChessTeam.Black), new Vector2Int(6, 7));
        board.AddPiece(new Chess_Rook().SetTeam(ChessTeam.Black), new Vector2Int(0, 7));
        board.AddPiece(new Chess_Rook().SetTeam(ChessTeam.Black), new Vector2Int(7, 7));

        return board;
    }


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

public abstract class BoardState
{
    public List<ChessPieceBase> CurrentPieces = new();
    public List<MultiRef<int, string>> History = new();
    public int CurrentTurn = 0;
    public ChessTeam CurrentTeam = ChessTeam.White;
    public bool IsGameUnderCheck = false;
    private List<ChessPieceBase> ChangedThisTurn = new();

    public void StartGame()
    {
        CurrentTurn = 0;
        CurrentTeam = ChessTeam.Black;
        AdvanceTurn();
    }

    public void AdvanceTurn()
    {
        foreach (var a in ChangedThisTurn)
        {
            a.CalculateValids(this);
            RecordHistory(a);
        }
        ChangedThisTurn.Clear();
        switch (CurrentTeam)
        {
            case ChessTeam.White:
                CurrentTeam = ChessTeam.Black;
                break;
            case ChessTeam.Black:
                CurrentTeam = ChessTeam.White;
                break;
            case ChessTeam.Red:
                CurrentTeam = ChessTeam.Blue;
                break;
            case ChessTeam.Blue:
                CurrentTeam = ChessTeam.Yellow;
                break;
            case ChessTeam.Yellow:
                CurrentTeam = ChessTeam.Green;
                break;
            case ChessTeam.Green:
                CurrentTeam = ChessTeam.Red;
                break;

        }
        CurrentTurn++;
        IsGameUnderCheck = IsTeamInCheck(CurrentTeam).valid;
    }


    public void AddPiece(ChessPieceBase nerd, Vector2Int pos)
    {
        nerd.Position = pos;
        nerd.Initialize();
        if (nerd.Name == "<Unset>") throw new System.Exception("Bro forgot to give the piece a name");
        ChangedThisTurn.Add(nerd);
    }
    public void RecordHistory(ChessPieceBase nerd)
    {
        History.Add(new MultiRef<int, string>(CurrentTurn, nerd.ToString()));
    }

    public abstract bool IsWithinBounds(Vector2Int pos);

    public bool IsOccupied(Vector2Int pos)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.CanBlockMovementAt(this, pos)) return true;
        }
        return false;
    }
    public virtual ChessPieceBase GetPieceAtPos(Vector2Int pos)
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
    /*public bool WouldMoveCauseCheck(ChessPieceBase piece, Vector2Int movepos)
    {
        ChessPieceBase king = null;
        foreach (var p in CurrentPieces)
        {
            if (p.Team != piece.Team && p.Name == "King")
            {
                king = p;
                break;
            }
        }
        if (king != null)
        {
            return piece.GetAllValidCaptures(this, movepos).Contains(movepos);
        }
        else
        {
            Debug.LogError("No king found for team " + piece.Team.ToString());
        }
        return false;
    }*/
    public (bool valid, ChessPieceBase king) IsTeamInCheck(ChessTeam team)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.Name == "King" && piece.Team == team)
            {
                foreach (var p in CurrentPieces)
                {
                    if (p.Team != piece.Team && p.CanCaptureAt(this, piece.Position))
                    {
                        return (true, piece);
                    }
                }
            }
        }
        return (false, null);
    }

    public bool WouldBeInCheckWithMove(ChessPieceBase piece, Vector2Int movepos)
    {
        var originalpos = piece.Position;
        piece.Position = movepos;
        var (incheck, _) = IsTeamInCheck(piece.Team);
        piece.Position = originalpos;
        return incheck;
    }

    public void MovePiece(ChessPieceBase piece, Vector2Int newpos)
    {
        piece.MoveTo(this, newpos);
        ChangedThisTurn.Add(piece);
    }

    public void CapturePiece(ChessPieceBase piece, ChessPieceBase takes, Vector2Int newpos)
    {
        piece.Capture(this, takes, newpos);
        ChangedThisTurn.Add(piece);
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
    public List<Vector2Int> ValidMoves = new();
    public List<Vector2Int> ValidCaptures = new();
    /// <summary>
    /// Sets the unique name and move vectors
    /// </summary>
    public abstract void Initialize();
    private bool calced_valids = false;
    public void CalculateValids(BoardState state)
    {
        calced_valids = false;
        ValidMoves = GetAllValidMoves(state, Position);
        ValidCaptures = GetAllValidCaptures(state, Position);
        calced_valids = true;
    }


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
        if (Position == this.Position && calced_valids) return ValidMoves;
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanMoveWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                var p2 = Position + TeamRotation(p);
                if (!state.IsWithinBounds(p2)) break;
                if (state.IsOccupied(p2)) break;
                if (state.WouldBeInCheckWithMove(this, p2)) break;
                if (!fin.Contains(p2)) fin.Add(p2);
            }
        }
        return fin;
    }
    public virtual List<Vector2Int> GetAllValidCaptures(BoardState state, Vector2Int Position)
    {
        if (Position == this.Position && calced_valids) return ValidCaptures;
        List<Vector2Int> fin = new List<Vector2Int>();
        foreach (ChessBoardVector v in MoveVectors)
        {
            if (!v.CanCaptureWith) continue;
            var d = v.TraverseVector();
            foreach (var p in d)
            {
                var p2 = Position + TeamRotation(p);
                if (!state.IsWithinBounds(p2)) break;
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