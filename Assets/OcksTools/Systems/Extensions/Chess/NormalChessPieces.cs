using System.Collections.Generic;
using UnityEngine;

public class ChessBoard_Default : ChessBoard
{
    public override string GetName() => "Default";
    public override bool IsSpaceInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    public override HashSet<string> GetSpaceFlags(ChessTeam Team, Vector2Int pos)
    {
        var h = base.GetSpaceFlags(Team, pos);
        if (Team == ChessTeam.White && pos.y == 7) h.Add("promotion");
        else if (Team == ChessTeam.Black && pos.y == 0) h.Add("promotion");
        return h;
    }

    public ChessBoard Make()
    {
        for (int i = 0; i < 8; i++)
        {
            AddPiece(new ChessPiece_Pawn(), (i, 1), ChessTeam.White);
            AddPiece(new ChessPiece_Pawn(), (i, 6), ChessTeam.Black);
        }
        AddPiece(new ChessPiece_Rook(), (0, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Rook(), (7, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Rook(), (0, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Rook(), (7, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Knight(), (1, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Knight(), (6, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Knight(), (1, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Knight(), (6, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Bishop(), (2, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Bishop(), (5, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Bishop(), (2, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Bishop(), (5, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_Queen(), (3, 0), ChessTeam.White);
        AddPiece(new ChessPiece_Queen(), (3, 7), ChessTeam.Black);
        AddPiece(new ChessPiece_King(), (4, 0), ChessTeam.White);
        AddPiece(new ChessPiece_King(), (4, 7), ChessTeam.Black);
        return this;
    }

}


public class ChessPiece_Pawn : ChessPieceBase
{
    public override string GetName() => "Pawn";
    public bool DoublePushed = false;
    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((0,1), 2, ChessMoveRequirement.RequireEmptySpace),
            new ChessBoardVector((1,1), ChessMoveRequirement.RequireCapture),
            new ChessBoardVector((-1,1), ChessMoveRequirement.RequireCapture)
        };
        DoublePushed = false;
    }
    public override void OnMove(Vector2Int OldPosition)
    {
        if (!CurrentBoard.Simulation)
        {
            BoardVectors[0] = new ChessBoardVector((0, 1), ChessMoveRequirement.RequireEmptySpace);
            DoublePushed = Mathf.Abs(OldPosition.y - Position.y) == 2;
        }
        if (OldPosition.x != Position.x)
        {
            var p = CurrentBoard.GetPieceAtPos(Position + TeamRotation(new Vector2Int(0, -1)));
            if (IsGoodPissPawn(p))
            {
                CurrentBoard.CapturePiece(this, p);
                CurrentBoard.MoveFlags.Add("en passant");
            }
        }
        if (CurrentBoard.GetSpaceFlags(Team, Position).Contains("promotion"))
        {
            CurrentBoard.RemovePieceFast(this, true);
            CurrentBoard.AddPiece(new ChessPiece_Queen(), Position, Team);
            CurrentBoard.MoveFlags.Add("promotion");
        }
    }
    public override void OnUpdate()
    {
        EnPassCheck(1);
        EnPassCheck(2);
        DoublePushed = false;
    }

    private void EnPassCheck(int i)
    {
        BoardVectors[i] = BoardVectors[i].SetReq(ChessMoveRequirement.RequireCapture);
        var p = CurrentBoard.GetPieceAtPos(Position + TeamRotation(new Vector2Int(BoardVectors[i].Position.x, 0)));
        if (IsGoodPissPawn(p))
        {
            BoardVectors[i] = BoardVectors[i].SetReq(ChessMoveRequirement.Normal);
        }
    }

    private bool IsGoodPissPawn(ChessPieceBase p)
    {
        if (p == null)
        {
            return false;
        }
        if (p.Team != Team && p.Name == "Pawn")
        {
            var pp = p as ChessPiece_Pawn;
            if (pp.DoublePushed)
                return true;
        }
        return false;
    }
}
public class ChessPiece_King : ChessPieceBase
{
    public override string GetName() => "King";

    private List<ChessBoardVector> _baseVectors;

    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((0,1)),
            new ChessBoardVector((1,1)),
            new ChessBoardVector((-1,1)),
            new ChessBoardVector((1,0)),
            new ChessBoardVector((0,-1)),
            new ChessBoardVector((1,-1)),
            new ChessBoardVector((-1,-1)),
            new ChessBoardVector((-1,0))
        };
        _baseVectors = new List<ChessBoardVector>(BoardVectors);
    }

    public override void OnUpdate()
    {
        // start fresh from the normal 8 king moves each turn
        BoardVectors = new List<ChessBoardVector>(_baseVectors);

        if (MoveTurn != -1) return; // king has moved before -> castling permanently disabled

        TryAddCastleVector(step: 1);   // kingside (toward local +x)
        TryAddCastleVector(step: -1);  // queenside (toward local -x)
    }

    public override void OnMove(Vector2Int OldPosition)
    {
        if (CurrentBoard.Simulation) return;
        if (HasMoved) return;

        int localDx = Mathf.Abs(OldPosition.x - Position.x);
        if (localDx == 2)
        {
            Vector2Int worldStep = TeamRotation(new Vector2Int(1, 0));
            int flip = 1;
            if (Team == ChessTeam.Black) flip = -1;
            Vector2Int worldStep2 = TeamRotation(new Vector2Int(flip, 0));
            if (OldPosition.x > Position.x)
            {
                //moved left, so move the rook to the right
                CurrentBoard.MovePieceInternal(CurrentBoard.GetPieceAtPos(Position + worldStep * -2 * flip), Position + worldStep2);
            }
            else
            {
                //moved right, so move the rook to the left
                CurrentBoard.MovePieceInternal(CurrentBoard.GetPieceAtPos(Position + worldStep * 1 * flip), Position - worldStep2);
            }
            CurrentBoard.MoveFlags.Add("castle");
        }
    }

    private void TryAddCastleVector(int step)
    {
        Vector2Int worldStep = TeamRotation(new Vector2Int(1, 0));
        ChessPieceBase rook = null;
        int flip = 1;
        if (Team == ChessTeam.Black) flip = -1;
        if (step == -1)
            rook = CurrentBoard.GetPieceAtPos(Position + worldStep * -4 * flip);
        else
            rook = CurrentBoard.GetPieceAtPos(Position + worldStep * 3 * flip);

        if (rook == null || rook.HasMoved) return;


        if (IsSquareAttacked(Position)) return;              // king currently in check
        if (IsSquareAttacked(Position + worldStep)) return;   // passes through attack
        if (IsSquareAttacked(Position + worldStep * 2)) return; // lands on attack

        BoardVectors.Add(new ChessBoardVector(
            new Vector2Int(step * 2, 0), ChessMoveRequirement.RequireEmptySpace));
    }

    private bool IsSquareAttacked(Vector2Int square)
    {
        foreach (var p in CurrentBoard.CurrentPieces)
        {
            if (p.Team != Team && p.AttacksSquare(square)) return true;
        }
        return false;
    }
}

public class ChessPiece_Queen : ChessPieceBase
{
    public override string GetName() => "Queen";
    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((0,1), 7),
            new ChessBoardVector((1,1), 7),
            new ChessBoardVector((-1,1), 7),
            new ChessBoardVector((1,0), 7),
            new ChessBoardVector((0,-1), 7),
            new ChessBoardVector((1,-1), 7),
            new ChessBoardVector((-1,-1), 7),
            new ChessBoardVector((-1,0), 7)
        };
    }
}

public class ChessPiece_Bishop : ChessPieceBase
{
    public override string GetName() => "Bishop";
    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((1,1), 7),
            new ChessBoardVector((-1,1), 7),
            new ChessBoardVector((1,-1), 7),
            new ChessBoardVector((-1,-1), 7)
        };
    }
}
public class ChessPiece_Rook : ChessPieceBase
{
    public override string GetName() => "Rook";
    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((0,1), 7),
            new ChessBoardVector((1,0), 7),
            new ChessBoardVector((0,-1), 7),
            new ChessBoardVector((-1,0), 7)
        };
    }
}

public class ChessPiece_Knight : ChessPieceBase
{
    public override string GetName() => "Knight";
    public override void OnAddedToBoard()
    {
        BoardVectors = new()
        {
            new ChessBoardVector((1,2)),
            new ChessBoardVector((-1,2)),
            new ChessBoardVector((1,-2)),
            new ChessBoardVector((-1,-2)),
            new ChessBoardVector((2,1)),
            new ChessBoardVector((-2,1)),
            new ChessBoardVector((2,-1)),
            new ChessBoardVector((-2,-1))
        };
    }
}
