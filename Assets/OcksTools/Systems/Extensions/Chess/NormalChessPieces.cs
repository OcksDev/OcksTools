using UnityEngine;

public class Chess_Pawn : ChessPieceBase
{
    private int moves_made = 0;
    private bool has_double_push = false;
    public override void Initialize()
    {
        Name = "Pawn";
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up, Vector2Int.up, true, false));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, Vector2Int.up, false, true));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, Vector2Int.up, false, true));
    }
    public override bool CanMoveTo(BoardState state, Vector2Int pos)
    {
        if (moves_made == 0)
        {
            MoveVectors[0].Direction = Vector2Int.up * 2;
        }
        else
        {
            MoveVectors[0].Direction = Vector2Int.up;
        }
        return base.CanMoveTo(state, pos);
    }

    public override void MoveTo(Vector2Int pos)
    {
        moves_made++;
        base.MoveTo(pos);
    }

    public override void Capture(ChessPieceBase nerd, Vector2Int pos)
    {
        moves_made++;
        base.Capture(nerd, pos);
    }

    public override bool CanBeCapturedByAt(BoardState state, ChessPieceBase nerd, Vector2Int pos)
    {
        if (nerd.Name == "Pawn" && has_double_push && moves_made == 1 && nerd.Team != Team && pos == Position + TeamRotation(Vector2Int.down)) return true;
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
        MoveVectors.Add(new ChessBoardVector(Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.right, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.up + Vector2Int.right, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.left, Vector2Int.up));
        MoveVectors.Add(new ChessBoardVector(Vector2Int.down + Vector2Int.right, Vector2Int.up));
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
