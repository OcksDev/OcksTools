using UnityEngine;

public class Chess_DefaultBoard : ChessBoard
{
    public override bool IsSpaceInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }


    public static ChessBoard MakeDefaultBoard()
    {
        var board = new Chess_DefaultBoard();
        for (int i = 0; i < 8; i++)
        {
            board.AddPiece(new ChessPiece_Pawn(), (i, 1), ChessTeam.White);
            board.AddPiece(new ChessPiece_Pawn(), (i, 6), ChessTeam.Black);
        }
        board.AddPiece(new ChessPiece_Rook(), (0, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Rook(), (7, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Rook(), (0, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Rook(), (7, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Knight(), (1, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Knight(), (6, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Knight(), (1, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Knight(), (6, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Bishop(), (2, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Bishop(), (5, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Bishop(), (2, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Bishop(), (5, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_Queen(), (3, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_Queen(), (3, 7), ChessTeam.Black);
        board.AddPiece(new ChessPiece_King(), (4, 0), ChessTeam.White);
        board.AddPiece(new ChessPiece_King(), (4, 7), ChessTeam.Black);
        return board;
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
            if (MoveTurn == -1) BoardVectors[0] = new ChessBoardVector((0, 1), ChessMoveRequirement.RequireEmptySpace);
            MoveTurn = CurrentBoard.CurrentTurn;
            DoublePushed = Mathf.Abs(OldPosition.y - Position.y) == 2;
        }
        if (OldPosition.x != Position.x)
        {
            var p = CurrentBoard.GetPieceAtPos(Position + TeamRotation(new Vector2Int(0, -1)));
            if (IsGoodPissPawn(p))
            {
                CurrentBoard.CapturePiece(this, p);
            }
        }
    }
    public override void OnUpdate()
    {
        EnPassCheck(1);
        EnPassCheck(2);
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
            if ((pp.MoveTurn == CurrentBoard.CurrentTurn - 1 || pp.MoveTurn == CurrentBoard.CurrentTurn) && pp.DoublePushed)
                return true;
        }
        return false;
    }
}

public class ChessPiece_King : ChessPieceBase
{
    public override string GetName() => "King";
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
