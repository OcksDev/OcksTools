using UnityEngine;

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
        if (nerd.Name == "Pawn" && has_double_push && MovesMade == 1 && nerd.Team != Team && nerd.TurnLastMovedOn == state.CurrentTurn - 1 && pos == Position + TeamRotation(Vector2Int.down)) return true;
        return base.CanBeCapturedByAt(state, nerd, pos);
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

public class Chess_King : ChessPieceBase
{
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
        if (Position == this.Position && MovesMade == 0)
        {
            var p1 = state.GetPieceAtPos(Position + TeamRotation(Vector2Int.right * 3));
            var p2 = state.GetPieceAtPos(Position + TeamRotation(Vector2Int.left * 4));
            if (p1 != null && p1.Name == "Rook" && p1.MovesMade == 0)
            {
                MoveVectors[2].Direction = Vector2Int.right * 2;
            }
            else
            {
                MoveVectors[2].Direction = Vector2Int.right;
            }
            if (p2 != null && p2.Name == "Rook" && p2.MovesMade == 0)
            {
                MoveVectors[2].Direction = Vector2Int.left * 2;
            }
            else
            {
                MoveVectors[2].Direction = Vector2Int.left;
            }
        }
        return base.CanMoveToFrom(state, pos, Position);
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
