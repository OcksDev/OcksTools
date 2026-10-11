using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    public OXKeyframeAnimation anim;
    public OXKeyframeAnimation anim2;
    bool state = false;
    private void Update()
    {
        if (InputManager.IsKeyDown(KeyCode.Space))
        {
            var pp = anim.Play(gameObject);
        }
        if (InputManager.IsKeyDown(KeyCode.M))
        {
            if (state)
            {
                anim2.Play(gameObject, 0.5f);
            }
            else
            {
                anim.Play(gameObject, 0.5f);
            }
            state = !state;
        }
    }
}
