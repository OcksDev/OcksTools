using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    public OXKeyframeAnimation anim;
    private void Update()
    {
        if (InputManager.IsKeyDown(KeyCode.Space))
        {
            var pp = anim.Play(gameObject);
        }
    }
}
