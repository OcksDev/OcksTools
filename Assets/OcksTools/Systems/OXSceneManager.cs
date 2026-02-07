using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class OXSceneManager : SingleInstance<OXSceneManager>
{
    public Dictionary<string, SceneData> Datas = new Dictionary<string, SceneData>();
    public override void Awake2()
    {
        var d = SceneManager.sceneCount;
        var d2 = SceneManager.GetActiveScene();
        for (int i = 0; i < d; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            var x = GetScene(s.name);
            if (s.name == d2.name)
            {
                x.IsCurrentlyActive.SetValue(true);
            }
            x.IsCurrentlyLoaded.SetValue(true);
        }
    }
    public SceneData GetScene(string a)
    {
        if (!Datas.ContainsKey(a))
        {
            Datas[a] = new SceneData(a);
        }
        return Datas[a];
    }
    public bool IsSceneActive(string a) => GetScene(a).IsCurrentlyActive;
    public async Task<SceneData> LoadSceneInstant(string a, bool keep_others = false)
    {
        var s = GetScene(a);
        if (s.IsCurrentlyLoading) return s;
        if (s.IsCurrentlyLoaded) return s;
        if (s.IsCurrentlyActive) return s;
        SceneManager.LoadScene(a, keep_others ? LoadSceneMode.Additive : LoadSceneMode.Single);
        s.IsCurrentlyLoaded.SetValue(true);
        s.IsCurrentlyActive.SetValue(true);
        foreach (SceneData data in Datas.Values)
        {
            if (data == s) continue;
            data.IsCurrentlyActive.SetValue(false);
        }
        return s;
    }

    public async Task<SceneData> LoadSceneBackground(string a, bool keep_others = false, bool active_when_done = false, bool async_unload = true)
    {
        var s = GetScene(a);
        if (s.IsCurrentlyLoading) return s;
        if (s.IsCurrentlyLoaded) return s;
        if (s.IsCurrentlyActive) return s;
        s.IsCurrentlyLoading.SetValue(true);
        var loadOp = SceneManager.LoadSceneAsync(a, keep_others ? LoadSceneMode.Additive : LoadSceneMode.Single);
        while (!loadOp.isDone)
        {
            await Task.Yield(); // wait one frame
        }
        s.IsCurrentlyLoading.SetValue(false);
        s.IsCurrentlyLoaded.SetValue(true);
        if (active_when_done)
        {
            await Task.Yield();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(a));
            s.IsCurrentlyActive.SetValue(true);
            foreach (SceneData data in Datas.Values)
            {
                if (data == s) continue;
                data.IsCurrentlyActive.SetValue(false);
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(data.Name));
            }
        }
        return s;
    }
}

[System.Serializable]
public class SceneData
{
    public string Name;
    public Reactable<bool> HasLoaded = new(false);
    public Reactable<bool> IsCurrentlyLoading = new(false);
    public Reactable<bool> IsCurrentlyLoaded = new(false);
    public Reactable<bool> IsCurrentlyActive = new(false);
    public SceneData(string a)
    {
        Name = a;
    }
}