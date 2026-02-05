using UnityEngine;
using UnityEngine.UI;

public class InputShow : MonoBehaviour
{
    public string Input;
    private SpriteRenderer b;
    private Image b2;
    private void Awake()
    {
        b = GetComponent<SpriteRenderer>();
        b2 = GetComponent<Image>();
    }
    private void LateUpdate()
    {
        if (InputManager.IsKey(Input))
        {
            if (b != null) b.color = Color.white;
            if (b2 != null) b2.color = Color.white;
        }
        else
        {
            if (b != null) b.color = Color.black;
            if (b2 != null) b2.color = Color.black;
        }
    }
}
