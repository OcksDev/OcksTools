using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    public OXKeyframeAnimation anim;
    public OXKeyframeAnimation anim2;
    private float spd = 1;
    private bool state = false;
    private void Update()
    {
        if (InputManager.IsKeyDown(KeyCode.Space))
        {
            var pp = anim.Play(gameObject, spd);
        }
        if (InputManager.IsKeyDown(KeyCode.M))
        {
            if (state)
            {
                anim2.Play(gameObject, 0.5f, spd);
            }
            else
            {
                anim.Play(gameObject, 0.5f, spd);
            }
            state = !state;
        }
        if (InputManager.IsKeyDown(KeyCode.N))
        {
            spd *= -1;
            GetComponent<OXKeyframeAnimator>().CurrentAnim.PlaybackSpeed = spd;
        }
    }
}
