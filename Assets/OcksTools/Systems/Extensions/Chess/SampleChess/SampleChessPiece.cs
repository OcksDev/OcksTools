using UnityEngine;

public class SampleChessPiece : MonoBehaviour
{
    public ChessPieceBase me;
    private SpriteRenderer sr;
    public SpriteRenderer cr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cr.gameObject.SetActive(false);
    }
    private void Update()
    {
        var w = Color.white;
        if (Hover.IsHovering(gameObject))
        {
            w = Color.Lerp(w, Color.black, 0.1f);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                SampleChess.Instance.SelectPiece(me);
            }
        }
        sr.color = w;
    }
}
