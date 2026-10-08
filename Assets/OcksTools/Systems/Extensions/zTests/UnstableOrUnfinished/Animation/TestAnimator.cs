using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    public OXKeyframeAnimation anim;
    private void Update()
    {
        if (InputManager.IsKeyDown(KeyCode.Space))
        {
            anim.Play(gameObject);
        }
        if (InputManager.IsKeyDown(KeyCode.P))
        {
            anim.Keyframes.Clear();
            var frame1 = new OXKeyframe() { Data = new() };
            frame1.Data.Add((0, new OXTransformWithScale() { Position = new Vector3(0, 0, 0) }));
            frame1.Time = 0;
            anim.Keyframes.Add(frame1);

            var frame2 = new OXKeyframe() { Data = new() };
            frame2.Data.Add((0, new OXTransformWithScale() { Position = new Vector3(0, 2, 0) }));
            frame2.Time = 1;
            anim.Keyframes.Add(frame2);

            var frame3 = new OXKeyframe() { Data = new() };
            frame3.Data.Add((0, new OXTransformWithScale() { Position = new Vector3(0, 0, 0) }));
            frame3.InterpMode = OXKeyframeInterpolationMode.Elastic;
            frame3.Time = 1.4f;
            anim.Keyframes.Add(frame3);
        }
    }
}
