using UnityEngine;

public class TestScene : MonoBehaviour
{
    public async void SceneChange()
    {
        if (OXSceneManager.Instance == null)
        {
            Debug.LogError("How?");
        }
        await OXSceneManager.Instance.LoadSceneInstant("GrapTest");
    }
    public async void SceneChangeKeep() => await OXSceneManager.Instance.LoadSceneInstant("GrapTest", true);
    public async void SceneBGLPA() => await OXSceneManager.Instance.LoadSceneBackground("GrapTest", true, true, true);
    public async void SceneBG() => await OXSceneManager.Instance.LoadSceneBackground("GrapTest", true);
}
