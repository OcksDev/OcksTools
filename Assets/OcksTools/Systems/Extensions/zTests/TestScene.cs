using UnityEngine;

public class TestScene : MonoBehaviour
{
    public void SceneChange()
    {
        if (OXSceneManager.Instance == null)
        {
            Debug.LogError("How?");
        }
        OXSceneManager.Instance.LoadSceneInstant("GrapTest");
    }
    public void SceneChangeKeep() => OXSceneManager.Instance.LoadSceneInstant("GrapTest", true);
    public async void SceneBGLPA() => await OXSceneManager.Instance.LoadSceneBackground("GrapTest", true, true, true);
    public async void SceneBG() => await OXSceneManager.Instance.LoadSceneBackground("GrapTest", true);
}
