using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ChessEngineV2
{

    public static Vector2Int TeamRotation(ChessTeam Team, Vector2Int pos)
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

public abstract class BoardState2
{
    public List<ChessPieceBase2> CurrentPieces = new();
    public int CurrentTurn = 0;
    public ChessTeam CurrentTeam = ChessTeam.White;

    public void StartGame_2Player()
    {
        CurrentTeam = ChessTeam.White;
        StartGame();
    }

    public void StartGame_4Player()
    {
        CurrentTeam = ChessTeam.Red;
        StartGame();
    }

    private void StartGame()
    {
        CurrentTurn = 0;
    }

    public void AdvanceTurn()
    {
        if (CurrentTeam == ChessTeam.Black || CurrentTeam == ChessTeam.Green)
        {
            CurrentTurn++;
        }
        CurrentTeam = NextTeam(CurrentTeam);
    }

    public static ChessTeam NextTeam(ChessTeam CurrentTeam)
    {
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
        return CurrentTeam;
    }

    public void AddPiece(ChessPieceBase2 piece, Vector2Int Position, ChessTeam Team)
    {
        if (!piece.IgnoreBounds && !IsSpaceInBounds(Position))
        {
            throw new System.Exception($"Position {Position} is out of bounds for this board.");
        }
        piece.Position = Position;
        piece.Team = Team;
        piece.CurrentBoard = this;
        piece.Name = piece.GetName();
        CurrentPieces.Add(piece);
        piece.OnAddedToBoard();
    }

    public abstract bool IsSpaceInBounds(Vector2Int pos);

    public ChessPieceBase2 GetPieceAtPos(Vector2Int pos)
    {
        foreach (var a in CurrentPieces)
        {
            if (a.Position == pos) return a;
        }
        return null;
    }
    public void MovePiece(ChessPieceBase2 piece, Vector2Int NewPosition)
    {
        if (!piece.IgnoreBounds && !IsSpaceInBounds(NewPosition))
        {
            throw new System.Exception($"Position {NewPosition} is out of bounds for this board.");
        }
        var pieceAtNewPos = GetPieceAtPos(NewPosition);
        CurrentPieces.Remove(pieceAtNewPos);
        piece.Position = NewPosition;
    }

    public (bool valid, ChessPieceBase2 king) IsTeamInCheck(ChessTeam team)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.Name == "King" && piece.Team == team) // finds the king of the team
            {
                foreach (var p in CurrentPieces)
                {
                    if (p.Team != piece.Team && p.GetAllPossibleMoves().Any(m => m.Position == piece.Position))
                    {
                        return (true, piece);
                    }
                }
            }
        }
        return (false, null);
    }
    public BoardState2 Clone()
    {
        var clone = MemberwiseClone() as BoardState2;
        clone.CurrentPieces = new List<ChessPieceBase2>();
        foreach (var piece in CurrentPieces)
        {
            var pieceClone = piece.Clone();
            pieceClone.CurrentBoard = clone;
            clone.CurrentPieces.Add(pieceClone);
        }
        return clone;
    }
}
[System.Serializable]
public abstract class ChessPieceBase2
{
    public string Name = "";
    public abstract string GetName();
    public BoardState2 CurrentBoard;
    public ChessTeam Team;
    public Vector2Int Position;
    public Vector2Int TeamRotation(Vector2Int Pos) => ChessEngineV2.TeamRotation(Team, Pos);
    public List<ChessBoardVector2> BoardVectors = new();
    public virtual bool IgnoreBounds => false;
    public virtual void OnAddedToBoard() { }

    public List<(Vector2Int Position, ChessPieceBase2 Piece)> GetAllPossibleMoves()
    {
        List<(Vector2Int Position, ChessPieceBase2 Piece)> validMoves = new();
        List<(Vector2Int Position, ChessPieceBase2 Piece)> validSpaces = new();
        foreach (var vector in BoardVectors)
        {
            var pp = vector.GetSpaces().Select(x => TeamRotation(x) + Position).ToList();
            validSpaces.Clear();
            for (int i = 0; i < pp.Count; i++)
            {
                var p = CurrentBoard.GetPieceAtPos(pp[i]);
                if (p == null && vector.MustCapture) continue;
                if (p != null && p.Team == Team) break;
                validSpaces.Add((pp[i], p));
                if (p != null) break;
            }
            validMoves.AddRange(validSpaces);
        }
        return validMoves;
    }
    public List<(Vector2Int Position, ChessPieceBase2 Piece)> GetLegalMoves(ChessPieceBase2 piece)
    {
        // track the piece by its index in the list, since Clone() preserves list order
        int pieceIndex = CurrentBoard.CurrentPieces.IndexOf(piece);
        var legalMoves = new List<(Vector2Int Position, ChessPieceBase2 Piece)>();

        foreach (var move in piece.GetAllPossibleMoves())
        {
            var clone = CurrentBoard.Clone();
            var clonedPiece = clone.CurrentPieces[pieceIndex];

            clone.MovePiece(clonedPiece, move.Position);

            var (inCheck, _) = clone.IsTeamInCheck(piece.Team);
            if (!inCheck)
                legalMoves.Add(move);
        }

        return legalMoves;
    }

    public ChessPieceBase2 Clone() { return MemberwiseClone() as ChessPieceBase2; }

}


public struct ChessBoardVector2
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public int Length;
    public bool MustCapture;
    public bool IncludeStartSpace;
    public ChessBoardVector2((short x, short y) pos, (short x, short y) direction, int length, bool mustCapture = false, bool includeStartSpace = true)
    {
        Position = new Vector2Int(pos.x, pos.y);
        Direction = new Vector2Int(direction.x, direction.y);
        Length = length;
        MustCapture = mustCapture;
        IncludeStartSpace = includeStartSpace;
    }

    public ChessBoardVector2(Vector2Int pos, Vector2Int direction, int length, bool mustCapture = false, bool includeStartSpace = true)
    {
        Position = pos;
        Direction = direction;
        Length = length;
        MustCapture = mustCapture;
        IncludeStartSpace = includeStartSpace;
    }

    public List<Vector2Int> GetSpaces()
    {
        List<Vector2Int> a = new List<Vector2Int>();
        for (int i = (IncludeStartSpace ? 0 : 1); i <= Length; i++)
        {
            a.Add(Position + new Vector2Int(Direction.x * i, Direction.y * i));
        }
        return a;
    }
}