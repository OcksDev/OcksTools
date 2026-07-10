using UnityEngine;
using UnityEngine.UI;

public class InputShow : MonoBehaviour
{
    public string Input;
    public KeyCode KeyCode;
    private SpriteRenderer b;
    private Image b2;
    private void Awake()
    {
        b = GetComponent<SpriteRenderer>();
        b2 = GetComponent<Image>();
    }
    private void LateUpdate()
    {
        bool e = Input == "" ? InputManager.IsKey(KeyCode) : InputManager.IsKey(Input);
        if (e)
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
