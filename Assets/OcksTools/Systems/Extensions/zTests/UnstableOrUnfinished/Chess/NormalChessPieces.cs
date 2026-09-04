using UnityEngine;

public class Chess_DefaultBoard : BoardState2
{
    public override bool IsSpaceInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
}


