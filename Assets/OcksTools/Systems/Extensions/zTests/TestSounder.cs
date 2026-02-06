using UnityEngine;

public class TestSounder : MonoBehaviour
{
    public void LeftSound()
    {
        SoundSystem.Instance.PlaySound(new OXSound("VB", 1f).Position(new Vector3(-8, 0, -10)));
    }
    public void RightSound()
    {
        SoundSystem.Instance.PlaySound(new OXSound("VB", 1f).Position(new Vector3(8, 0, -10)));
    }
}
