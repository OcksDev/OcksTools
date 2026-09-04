using System.Collections.Generic;
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
    private Dictionary<Vector2Int, ChessPieceBase2> _positionLookup = new();
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
        piece.BoardIndex = CurrentPieces.Count;
        CurrentPieces.Add(piece);
        _positionLookup.Add(Position, piece);
        piece.OnAddedToBoard();
    }

    public abstract bool IsSpaceInBounds(Vector2Int pos);
    public ChessPieceBase2 GetPieceAtPos(Vector2Int pos)
    {
        return _positionLookup.TryGetValue(pos, out var piece) ? piece : null;
    }
    public void MovePiece(ChessPieceBase2 piece, Vector2Int NewPosition)
    {
        if (!piece.IgnoreBounds && !IsSpaceInBounds(NewPosition))
        {
            throw new System.Exception($"Position {NewPosition} is out of bounds for this board.");
        }
        var pieceAtNewPos = GetPieceAtPos(NewPosition);
        if (pieceAtNewPos != null)
        {
            CapturePiece(piece, pieceAtNewPos);
        }
        var oldpos = piece.Position;
        _positionLookup.Remove(piece.Position);
        piece.Position = NewPosition;
        _positionLookup.Add(NewPosition, piece);
        piece.OnMove(oldpos);
        piece.OnMoveEvent?.Invoke(piece, oldpos);
    }
    public void CapturePiece(ChessPieceBase2 piece, ChessPieceBase2 cap_piece)
    {
        var l = new List<ChessPieceBase2>() { cap_piece };
        piece.OnCapture(l);
        piece.OnCaptureEvent?.Invoke(piece, l);
        RemovePieceFast(cap_piece);
    }

    private void RemovePieceFast(ChessPieceBase2 piece)
    {
        int idx = piece.BoardIndex;
        int lastIdx = CurrentPieces.Count - 1;

        if (idx != lastIdx)
        {
            var lastPiece = CurrentPieces[lastIdx];
            CurrentPieces[idx] = lastPiece;
            lastPiece.BoardIndex = idx;
        }
        CurrentPieces.RemoveAt(lastIdx);
        _positionLookup.Remove(piece.Position);
    }

    public (bool valid, ChessPieceBase2 king) IsTeamInCheck(ChessTeam team)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.Name == "King" && piece.Team == team) // finds the king of the team
            {
                foreach (var p in CurrentPieces)
                {
                    if (p.Team != piece.Team && p.AttacksSquare(piece.Position))
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
        clone.CurrentPieces = new List<ChessPieceBase2>(CurrentPieces.Count);
        clone._positionLookup = new Dictionary<Vector2Int, ChessPieceBase2>(CurrentPieces.Count);
        foreach (var piece in CurrentPieces)
        {
            var pieceClone = piece.Clone();
            pieceClone.CurrentBoard = clone;
            clone.CurrentPieces.Add(pieceClone);
            clone._positionLookup.Add(pieceClone.Position, pieceClone);
        }
        return clone;
    }
    public bool HasAnyLegalMoves(ChessTeam team)
    {
        foreach (var piece in CurrentPieces)
        {
            if (piece.Team == team && piece.GetLegalMoves().Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public ChessGameStatus GetGameStatus(ChessTeam team)
    {
        var (inCheck, _) = IsTeamInCheck(team);
        bool hasMoves = HasAnyLegalMoves(team);

        if (inCheck && !hasMoves) return ChessGameStatus.Checkmate;
        if (!inCheck && !hasMoves) return ChessGameStatus.Stalemate;
        if (inCheck) return ChessGameStatus.Check;
        return ChessGameStatus.Normal;
    }
}
public enum ChessGameStatus { Normal, Check, Checkmate, Stalemate }
[System.Serializable]
public abstract class ChessPieceBase2
{
    public string Name = "";
    public int BoardIndex = -1;
    public abstract string GetName();
    public BoardState2 CurrentBoard;
    public ChessTeam Team;
    public Vector2Int Position;
    public Vector2Int TeamRotation(Vector2Int Pos) => ChessEngineV2.TeamRotation(Team, Pos);
    public List<ChessBoardVector2> BoardVectors = new();
    public virtual bool IgnoreBounds => false;
    public virtual void OnAddedToBoard() { }
    public OXEvent<ChessPieceBase2, Vector2Int> OnMoveEvent = new();
    public virtual void OnMove(Vector2Int OldPosition) { }
    public OXEvent<ChessPieceBase2, List<ChessPieceBase2>> OnCaptureEvent = new();
    public virtual void OnCapture(List<ChessPieceBase2> CapturedPieces) { }
    public OXEvent<ChessPieceBase2> OnDestroyEvent = new();
    public GameObject WorldObject;
    public List<(Vector2Int Position, ChessPieceBase2 Piece)> GetAllPossibleMoves()
    {
        var validMoves = new List<(Vector2Int Position, ChessPieceBase2 Piece)>();

        foreach (var vector in BoardVectors)
        {
            var spaces = vector.GetSpaces();
            for (int i = 0; i < spaces.Length; i++)
            {
                var pp = TeamRotation(spaces[i]) + Position;
                var p = CurrentBoard.GetPieceAtPos(pp);
                if (p == null && vector.MustCapture) continue;
                if (p != null && p.Team == Team) break;
                validMoves.Add((pp, p));
                if (p != null) break;
            }
        }

        return validMoves;
    }
    public bool AttacksSquare(Vector2Int target)
    {
        foreach (var vector in BoardVectors)
        {
            var spaces = vector.GetSpaces();
            for (int i = 0; i < spaces.Length; i++)
            {
                var pp = TeamRotation(spaces[i]) + Position;
                var p = CurrentBoard.GetPieceAtPos(pp);
                if (p == null && vector.MustCapture) continue;
                if (p != null && p.Team == Team) break;

                if (pp == target) return true;

                if (p != null) break;
            }
        }
        return false;
    }

    public List<(Vector2Int Position, ChessPieceBase2 Piece)> GetLegalMoves()
    {
        var legalMoves = new List<(Vector2Int Position, ChessPieceBase2 Piece)>();

        foreach (var move in GetAllPossibleMoves())
        {
            var clone = CurrentBoard.Clone();
            var clonedPiece = clone.CurrentPieces[BoardIndex];

            clone.MovePiece(clonedPiece, move.Position);

            var (inCheck, _) = clone.IsTeamInCheck(Team);
            if (!inCheck)
                legalMoves.Add(move);
        }

        return legalMoves;
    }
    public ChessPieceBase2 Clone()
    {
        var c = MemberwiseClone() as ChessPieceBase2;
        c.OnCaptureEvent = null;
        c.OnMoveEvent = null;
        c.OnDestroyEvent = null;
        return c;
    }

}

public struct ChessBoardVector2
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public int Length;
    public bool MustCapture;
    public bool IncludeStartSpace;
    private Vector2Int[] _cachedSpaces;
    public ChessBoardVector2((short x, short y) pos, (short x, short y) direction, int length, bool mustCapture = false, bool includeStartSpace = true)
    {
        Position = new Vector2Int(pos.x, pos.y);
        Direction = new Vector2Int(direction.x, direction.y);
        Length = length;
        MustCapture = mustCapture;
        IncludeStartSpace = includeStartSpace;
        _cachedSpaces = null;
    }

    public ChessBoardVector2(Vector2Int pos, Vector2Int direction, int length, bool mustCapture = false, bool includeStartSpace = true)
    {
        Position = pos;
        Direction = direction;
        Length = length;
        MustCapture = mustCapture;
        IncludeStartSpace = includeStartSpace;
        _cachedSpaces = null;
    }
    public Vector2Int[] GetSpaces()
    {
        if (_cachedSpaces == null)
        {
            int start = IncludeStartSpace ? 0 : 1;
            _cachedSpaces = new Vector2Int[Length - start + 1];
            for (int i = start; i <= Length; i++)
                _cachedSpaces[i - start] = Position + new Vector2Int(Direction.x * i, Direction.y * i);
        }
        return _cachedSpaces;
    }
}