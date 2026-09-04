using System.Collections.Generic;
using UnityEngine;
public static class ChessEngine
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
    [RuntimeInitializeOnLoadMethod]
    public static void InitPieces()
    {
        OXFactory.DefineForInheritorsOf<ChessPieceBase>((x) => x.GetName());
        OXFactory.DefineForInheritorsOf<ChessBoard>((x) => x.GetName());
    }

    public static void SaveBoard(ChessBoard board, SaveProfile dict, string key)
    {
        key = "Chess_" + key;
        List<string> PieceData = new List<string>(board.CurrentPieces.Count);
        foreach (var a in board.CurrentPieces)
        {
            PieceData.Add(a.SaveToString());
        }
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"Board", board.GetName()},
            {"Team", board.CurrentTeam.ToString()},
            {"Turn", board.CurrentTurn.ToString()},
            {"Pieces", PieceData.ListToString("<>")},
        };
        dict.SetDict(key, data);
    }

    public static ChessBoard LoadBoard(SaveProfile dict, string key)
    {
        key = "Chess_" + key;
        var data = dict.GetDict(key, new());
        var board = OXFactory.Create<ChessBoard>(data["Board"]);
        board.CurrentTeam = System.Enum.Parse<ChessTeam>(data["Team"]);
        board.CurrentTurn = int.Parse(data["Turn"]);
        List<string> PieceData = data["Turn"].StringToList("<>");
        foreach (var a in PieceData)
        {
            var p = LoadPiece(a);
            board.AddPiece(p, p.Position, p.Team);
        }
        return board;
    }

    public static ChessPieceBase LoadPiece(string data)
    {
        List<string> real_data = data.StringToList("|");
        var p = OXFactory.Create<ChessPieceBase>(real_data[0]);
        p.Name = real_data[0];
        p.Team = System.Enum.Parse<ChessTeam>(real_data[1]);
        p.Position = real_data[2].StringToVector2Int();
        p.MoveTurn = int.Parse(real_data[3]);
        string ed = "";
        if (real_data.Count == 5) ed = real_data[4];
        p.LoadExtraData(ed);
        return p;
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

public abstract class ChessBoard
{
    public abstract string GetName();
    public List<ChessPieceBase> CurrentPieces = new();
    private Dictionary<Vector2Int, ChessPieceBase> _positionLookup = new();
    public int CurrentTurn = 0;
    public ChessTeam CurrentTeam = ChessTeam.White;
    public bool Simulation = false;
    public HashSet<string> MoveFlags = new HashSet<string>();

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
        foreach (var item in CurrentPieces)
        {
            if (item.Team != CurrentTeam) continue;
            item.OnUpdate();
        }
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

    public void AddPiece(ChessPieceBase piece, (int, int) Position, ChessTeam Team)
    {
        AddPiece(piece, new Vector2Int(Position.Item1, Position.Item2), Team);
    }
    public OXEvent<ChessPieceBase> OnPieceAddedEvent = new();
    public void AddPiece(ChessPieceBase piece, Vector2Int Position, ChessTeam Team)
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
        piece.HasMoved = piece.MoveTurn >= 0;
        CurrentPieces.Add(piece);
        _positionLookup.Add(Position, piece);
        piece.OnAddedToBoard();
        OnPieceAddedEvent?.Invoke(piece);
    }

    public abstract bool IsSpaceInBounds(Vector2Int pos);
    public virtual HashSet<string> GetSpaceFlags(ChessTeam Team, Vector2Int pos)
    {
        var h = new HashSet<string>();
        if (IsSpaceInBounds(pos))
        {
            h.Add("inbounds");
        }
        return h;
    }
    public ChessPieceBase GetPieceAtPos(Vector2Int pos)
    {
        return _positionLookup.TryGetValue(pos, out var piece) ? piece : null;
    }

    public HashSet<string> MovePiece(ChessPieceBase piece, Vector2Int NewPosition)
    {
        MoveFlags.Clear();
        MovePieceInternal(piece, NewPosition);
        if (IsTeamInCheck(NextTeam(CurrentTeam)).valid)
        {
            MoveFlags.Add("check");
        }
        return MoveFlags;
    }

    public HashSet<string> MovePieceInternal(ChessPieceBase piece, Vector2Int NewPosition)
    {
        if (piece.Position == NewPosition) { return new(); }
        if (!piece.IgnoreBounds && !IsSpaceInBounds(NewPosition))
        {
            throw new System.Exception($"Position {NewPosition} is out of bounds for this board.");
        }
        var pieceAtNewPos = GetPieceAtPos(NewPosition);
        if (pieceAtNewPos != null)
        {
            CapturePiece(piece, pieceAtNewPos);
            MoveFlags.Add("capture");
        }
        piece.MoveTurn = CurrentTurn;
        var oldpos = piece.Position;
        _positionLookup.Remove(piece.Position);
        piece.Position = NewPosition;
        _positionLookup.Add(NewPosition, piece);
        piece.OnMove(oldpos);
        piece.OnMoveEvent?.Invoke(piece, oldpos);
        piece.HasMoved = true;
        return MoveFlags;
    }
    public void CapturePiece(ChessPieceBase piece, ChessPieceBase cap_piece)
    {
        piece.OnCapture(cap_piece);
        piece.OnCaptureEvent?.Invoke(piece, cap_piece);
        cap_piece.OnDestroy(piece);
        cap_piece.OnDestroyEvent?.Invoke(cap_piece, piece);
        MoveFlags.Add("capture");
        RemovePieceFast(cap_piece);
    }

    public void RemovePieceFast(ChessPieceBase piece, bool destroy = true)
    {
        int idx = piece.BoardIndex;
        int lastIdx = CurrentPieces.Count - 1;

        if (idx != lastIdx)
        {
            var lastPiece = CurrentPieces[lastIdx];
            CurrentPieces[idx] = lastPiece;
            lastPiece.BoardIndex = idx;
        }

        if (destroy)
        {
            piece.OnDestroy(piece);
            piece.OnDestroyEvent?.Invoke(piece, piece);
        }

        CurrentPieces.RemoveAt(lastIdx);
        _positionLookup.Remove(piece.Position);
    }

    public (bool valid, ChessPieceBase king) IsTeamInCheck(ChessTeam team)
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
    public ChessBoard Clone()
    {
        var clone = MemberwiseClone() as ChessBoard;
        clone.Simulation = true;
        clone.CurrentPieces = new List<ChessPieceBase>(CurrentPieces.Count);
        clone._positionLookup = new Dictionary<Vector2Int, ChessPieceBase>(CurrentPieces.Count);
        clone.OnPieceAddedEvent = null;
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
public abstract class ChessPieceBase
{
    public string Name = "";
    public int BoardIndex = -1;
    public abstract string GetName();
    public ChessBoard CurrentBoard;
    public ChessTeam Team;
    public Vector2Int Position;
    public int MoveTurn = -1;
    public bool HasMoved = false;
    public Vector2Int TeamRotation(Vector2Int Pos) => ChessEngine.TeamRotation(Team, Pos);
    public List<ChessBoardVector> BoardVectors = new();
    public virtual bool IgnoreBounds => false;
    public OXEvent<ChessPieceBase, Vector2Int> OnMoveEvent = new();
    public OXEvent<ChessPieceBase, ChessPieceBase> OnCaptureEvent = new();
    public OXEvent<ChessPieceBase, ChessPieceBase> OnDestroyEvent = new();
    public virtual void OnAddedToBoard() { }
    public virtual void OnUpdate() { }
    public virtual void OnMove(Vector2Int OldPosition) { }
    public virtual void OnCapture(ChessPieceBase CapturedPiece) { }
    public virtual void OnDestroy(ChessPieceBase Killer) { }
    public GameObject WorldObject;
    public List<(Vector2Int Position, ChessPieceBase Piece)> GetAllPossibleMoves()
    {
        var validMoves = new List<(Vector2Int Position, ChessPieceBase Piece)>();
        HashSet<Vector2Int> visitedPositions = new();
        foreach (var vector in BoardVectors)
        {
            var spaces = vector.GetSpaces();
            for (int i = 0; i < spaces.Length; i++)
            {
                var pp = TeamRotation(spaces[i]) + Position;
                if (visitedPositions.Contains(pp)) continue;
                if (!IgnoreBounds && !CurrentBoard.IsSpaceInBounds(pp)) break;
                var p = CurrentBoard.GetPieceAtPos(pp);
                if (p == null && vector.MoveReq == ChessMoveRequirement.RequireCapture) continue;
                if (p != null && vector.MoveReq == ChessMoveRequirement.RequireEmptySpace) break;
                if (p != null && p.Team == Team) break;
                validMoves.Add((pp, p));
                visitedPositions.Add(pp);
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
                if (p == null && vector.MoveReq == ChessMoveRequirement.RequireCapture) continue;
                if (p != null && vector.MoveReq == ChessMoveRequirement.RequireEmptySpace) continue;
                if (p != null && p.Team == Team) break;

                if (pp == target) return true;

                if (p != null) break;
            }
        }
        return false;
    }

    public List<(Vector2Int Position, ChessPieceBase Piece, HashSet<string> Flags)> GetLegalMoves()
    {
        var legalMoves = new List<(Vector2Int Position, ChessPieceBase Piece, HashSet<string> Flags)>();

        foreach (var move in GetAllPossibleMoves())
        {
            var clone = CurrentBoard.Clone();
            var clonedPiece = clone.CurrentPieces[BoardIndex];

            var f = clone.MovePiece(clonedPiece, move.Position);

            var (inCheck, _) = clone.IsTeamInCheck(Team);
            if (!inCheck)
                legalMoves.Add((move.Position, move.Piece, new(f)));
        }

        return legalMoves;
    }
    public ChessPieceBase Clone()
    {
        var c = MemberwiseClone() as ChessPieceBase;
        c.OnCaptureEvent = null;
        c.OnMoveEvent = null;
        c.OnDestroyEvent = null;
        return c;
    }
    public string SaveToString()
    {
        List<string> d = new()
        {
            Name,
            Team.ToString(),
            Position.ToString(),
            MoveTurn.ToString(),
        };
        string s = GetExtraData();
        if (s != null && s != "") d.Add(s);
        return d.ListToString("|");
    }
    public virtual string GetExtraData() { return ""; }
    public virtual void LoadExtraData(string a) { }
}

public struct ChessBoardVector
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public int Length;
    public ChessMoveRequirement MoveReq;
    public bool IncludeStartSpace;
    private Vector2Int[] _cachedSpaces;
    public ChessBoardVector((short x, short y) pos, (short x, short y) direction, int length, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal, bool includeStartSpace = false)
    {
        Position = new Vector2Int(pos.x, pos.y);
        Direction = new Vector2Int(direction.x, direction.y);
        Length = length;
        MoveReq = mustCapture;
        IncludeStartSpace = includeStartSpace;
        _cachedSpaces = null;
    }
    public ChessBoardVector((short x, short y) direction, int length, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal)
    {
        Position = new Vector2Int(0, 0);
        Direction = new Vector2Int(direction.x, direction.y);
        Length = length;
        MoveReq = mustCapture;
        IncludeStartSpace = false;
        _cachedSpaces = null;
    }
    public ChessBoardVector(Vector2Int direction, int length, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal)
    {
        Position = new Vector2Int(0, 0);
        Direction = direction;
        Length = length;
        MoveReq = mustCapture;
        IncludeStartSpace = false;
        _cachedSpaces = null;
    }
    public ChessBoardVector((short x, short y) pos, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal)
    {
        Position = new Vector2Int(pos.x, pos.y);
        Direction = new Vector2Int(0, 0);
        Length = 0;
        MoveReq = mustCapture;
        IncludeStartSpace = true;
        _cachedSpaces = null;
    }
    public ChessBoardVector(Vector2Int pos, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal)
    {
        Position = pos;
        Direction = new Vector2Int(0, 0);
        Length = 0;
        MoveReq = mustCapture;
        IncludeStartSpace = true;
        _cachedSpaces = null;
    }

    public ChessBoardVector(Vector2Int pos, Vector2Int direction, int length, ChessMoveRequirement mustCapture = ChessMoveRequirement.Normal, bool includeStartSpace = false)
    {
        Position = pos;
        Direction = direction;
        Length = length;
        MoveReq = mustCapture;
        IncludeStartSpace = includeStartSpace;
        _cachedSpaces = null;
    }
    public Vector2Int[] GetSpaces()
    {
        if (_cachedSpaces == null)
        {
            if (Length == 0)
            {
                _cachedSpaces = new Vector2Int[1] { Position };
                return _cachedSpaces;
            }
            int start = IncludeStartSpace ? 0 : 1;
            _cachedSpaces = new Vector2Int[Length - start + 1];
            for (int i = start; i <= Length; i++)
                _cachedSpaces[i - start] = Position + new Vector2Int(Direction.x * i, Direction.y * i);
        }
        return _cachedSpaces;
    }

    public ChessBoardVector SetReq(ChessMoveRequirement req)
    {
        MoveReq = req;
        return this;
    }
}

public enum ChessMoveRequirement
{
    Normal,
    RequireCapture,
    RequireEmptySpace,
}