using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class OXSceneManager : SingleInstance<OXSceneManager>
{
    public List<MRefNoName<string, SceneField>> StaticNameOverrides = new();
    public Dictionary<string, SceneData> Datas = new();
    public Dictionary<string, string> StaticNames = new();
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
        foreach (var s in StaticNameOverrides)
        {
            StaticNames[s.a] = s.b.Name;
        }
    }
    public SceneData GetScene(string a)
    {
        if (StaticNames.TryGetValue(a, out var x)) a = x;
        if (!Datas.ContainsKey(a))
        {
            Datas[a] = new SceneData(a);
        }
        return Datas[a];
    }
    public bool IsSceneActive(string a) => GetScene(a).IsCurrentlyActive;
    public SceneData LoadSceneInstant(string scenename, bool keep_others = false)
    {
        var s = GetScene(scenename);
        if (s.IsCurrentlyLoading) return s;
        if (s.IsCurrentlyLoaded) return s;
        if (s.IsCurrentlyActive) return s;
        SceneManager.LoadScene(s.Name, keep_others ? LoadSceneMode.Additive : LoadSceneMode.Single);
        s.IsCurrentlyLoaded.SetValue(true);
        s.IsCurrentlyActive.SetValue(true);
        foreach (SceneData data in Datas.Values)
        {
            if (data == s) continue;
            data.IsCurrentlyActive.SetValue(false);
        }
        return s;
    }
    public SceneData LoadSceneInstant(SceneField scene, bool keep_others = false)
    {
        return LoadSceneInstant(scene.Name, keep_others);
    }

    public async Task<SceneData> LoadSceneBackground(string scenename, bool keep_others = false, bool active_when_done = false, bool async_unload = true)
    {
        var s = GetScene(scenename);
        if (s.IsCurrentlyLoading) return s;
        if (s.IsCurrentlyLoaded) return s;
        if (s.IsCurrentlyActive) return s;
        s.IsCurrentlyLoading.SetValue(true);
        var loadOp = SceneManager.LoadSceneAsync(s.Name, keep_others ? LoadSceneMode.Additive : LoadSceneMode.Single);
        while (!loadOp.isDone)
        {
            await Task.Yield(); // wait one frame
        }
        s.IsCurrentlyLoading.SetValue(false);
        s.IsCurrentlyLoaded.SetValue(true);
        if (active_when_done)
        {
            await Task.Yield();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(s.Name));
            s.IsCurrentlyActive.SetValue(true);
            foreach (SceneData data in Datas.Values)
            {
                if (data == s) continue;
                data.IsCurrentlyActive.SetValue(false);
                _ = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(data.Name));
            }
        }
        return s;
    }

    public async Task<SceneData> LoadSceneBackground(SceneField scene, bool keep_others = false, bool active_when_done = false, bool async_unload = true)
    {
        return await LoadSceneBackground(scene.Name, keep_others, active_when_done, async_unload);
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

[System.Serializable]
public struct SceneField : ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    public UnityEditor.SceneAsset Scene;
#endif
    [HideInInspector]
    public string InternalName;
    public string Name
    {
        get
        {
#if UNITY_EDITOR
            return Scene.name;
#else
            return InternalName;
#endif
        }
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (Scene != null)
        {
            InternalName = Scene.name;
        }
#endif
    }

    public void OnAfterDeserialize()
    {
        // nyothing needed hewe, nya
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class FuckassSceneDrawer : AutoCompressedInspectorWithName
{
}
#endif