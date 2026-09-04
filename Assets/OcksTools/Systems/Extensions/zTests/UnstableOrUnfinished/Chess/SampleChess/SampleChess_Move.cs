using UnityEngine;

public class SampleChess_Move : MonoBehaviour
{
    public Vector2Int Mypos;
    public bool cap;
    public ChessPieceBase2 me;
    private void Update()
    {
        if (Hover.IsHovering(gameObject))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                SampleChess.Instance.SelectMove(this);
            }
        }
    }
}
