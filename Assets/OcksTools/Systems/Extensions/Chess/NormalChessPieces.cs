using UnityEngine;
using static ChessEngine;

public class Chess_DefaultBoard : BoardState
{
    public override bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
}


public class Chess_Pawn : ChessPieceBase
{
    private bool has_double_push = false;
    public override void Initialize()
    {
        Name = "Pawn";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up, Vector2Int.up, true, false));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, Vector2Int.up, false, true));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, Vector2Int.up, false, true));
    }
    public override bool CanMoveToFrom(BoardState state, Vector2Int pos, Vector2Int Position)
    {
        if (MovesMade == 0 && Position == this.Position)
        {
            MoveVectors[0].Direction = Vector2Int.up * 2;
        }
        else
        {
            MoveVectors[0].Direction = Vector2Int.up;
        }
        return base.CanMoveToFrom(state, pos, Position);
    }

    public override bool CanBeCapturedByAt(BoardState state, ChessPieceBase nerd, Vector2Int pos)
    {
        if (nerd.Name == "Pawn" && has_double_push && MovesMade == 1 && nerd.Team != Team && pos == Position + TeamRotation(Vector2Int.down)) return true;
        return base.CanBeCapturedByAt(state, nerd, pos);
    }
}


public class Chess_King : ChessPieceBase
{

    public (int left, int right) GetOffsetForCastle()
    {
        switch (Team)
        {
            case ChessTeam.White: return (4, 3);
            case ChessTeam.Black: return (3, 4);
            default: return (4, 3);
        }
    }


    public override void Initialize()
    {
        Name = "King";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.left, Vector2Int.left));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.right, Vector2Int.right));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.right, Vector2Int.up));
    }

    public override bool CanMoveToFrom(BoardState state, Vector2Int pos, Vector2Int Position)
    {
        if (Position == this.Position && MovesMade == 0 && !state.IsGameUnderCheck)
        {
            var p1 = state.GetPieceAtPos(Position + TeamRotation(Vector2Int.right * GetOffsetForCastle().right));
            var p2 = state.GetPieceAtPos(Position + TeamRotation(Vector2Int.left * GetOffsetForCastle().left));
            if (p1 != null && p1.Name == "Rook" && p1.MovesMade == 0 && p1.Team == Team)
            {
                MoveVectors[2].Direction = Vector2Int.right * 2;
            }
            else
            {
                MoveVectors[2].Direction = Vector2Int.right;
            }
            if (p2 != null && p2.Name == "Rook" && p2.MovesMade == 0 && p2.Team == Team)
            {
                MoveVectors[2].Direction = Vector2Int.left * 2;
            }
            else
            {
                MoveVectors[2].Direction = Vector2Int.left;
            }
        }
        else
        {
            MoveVectors[2].Direction = Vector2Int.right;
            MoveVectors[2].Direction = Vector2Int.left;
        }
        return base.CanMoveToFrom(state, pos, Position);
    }

    public override bool CanMoveTo(BoardState state, Vector2Int pos)
    {
        if (MovesMade == 0)
        {
            if (pos == Position + TeamRotation(Vector2Int.right * 2))
            {
                state.MovePiece(state.GetPieceAtPos(Position + TeamRotation(Vector2Int.right * GetOffsetForCastle().right)), Position + TeamRotation(Vector2Int.right));
            }
            else if (pos == Position + TeamRotation(Vector2Int.left * 2))
            {
                state.MovePiece(state.GetPieceAtPos(Position + TeamRotation(Vector2Int.left * GetOffsetForCastle().left)), Position + TeamRotation(Vector2Int.left));
            }
        }
        return base.CanMoveTo(state, pos);
    }

}


public class Chess_Bishop : ChessPieceBase
{
    public override void Initialize()
    {
        Name = "Bishop";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, (Vector2Int.up + Vector2Int.left) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, (Vector2Int.up + Vector2Int.right) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.left, (Vector2Int.down + Vector2Int.left) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.right, (Vector2Int.down + Vector2Int.right) * 7));
    }
}

public class Chess_Rook : ChessPieceBase
{
    public override void Initialize()
    {
        Name = "Rook";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up, Vector2Int.up * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down, Vector2Int.down * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.left, Vector2Int.left * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.right, Vector2Int.right * 7));
    }
}

public class Chess_Queen : ChessPieceBase
{
    public override void Initialize()
    {
        Name = "Queen";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up, Vector2Int.up * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down, Vector2Int.down * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.left, Vector2Int.left * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.right, Vector2Int.right * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, (Vector2Int.up + Vector2Int.left) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, (Vector2Int.up + Vector2Int.right) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.left, (Vector2Int.down + Vector2Int.left) * 7));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.right, (Vector2Int.down + Vector2Int.right) * 7));
    }
}


public class Chess_Knight : ChessPieceBase
{
    public override void Initialize()
    {
        Name = "Knight";
        MoveVectors.Add(new ChessBoardVector((Vector2Int.up * 2) + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector((Vector2Int.up * 2) + Vector2Int.right, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector((Vector2Int.down * 2) + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector((Vector2Int.down * 2) + Vector2Int.right, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + (Vector2Int.left * 2), Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + (Vector2Int.right * 2), Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + (Vector2Int.left * 2), Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + (Vector2Int.right * 2), Vector2Int.up));
    }
}
