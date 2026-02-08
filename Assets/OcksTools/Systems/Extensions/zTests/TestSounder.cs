using UnityEngine;

public class TestSounder : MonoBehaviour
{
    public void LeftSound()
    {
        SoundSystem.Instance.PlaySound(new OXSound("Vine Boom", 1f).Position(new Vector3(-8, 0, -10)));
    }
    public void RightSound()
    {
        SoundSystem.Instance.PlaySound(new OXSound("Vine Boom", 1f).Position(new Vector3(8, 0, -10)));
    }
    public void RightSoundLoad()
    {
        SoundSystem.Instance.PlaySound(new OXSound("Vine Boom", 1000f).Position(new Vector3(8, 0, -10)).Mixer("SFX"));
    }
}
