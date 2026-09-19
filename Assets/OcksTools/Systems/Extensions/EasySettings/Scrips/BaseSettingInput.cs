using System.Collections;
using UnityEngine;

public abstract class BaseSettingInput<T> : BaseBaseSettingLol where T : SettingData
{
    public T Setting;
    private void OnEnable()
    {
        if (Time.time < 0.02f) return; // dont update display on game start
        UpdateDisplay();
        Init();
    }
    public virtual void Init() { }
    private IEnumerator Start()
    {
        if (SaveSystem.Instance != null) yield return new WaitUntil(() => SaveSystem.Instance.LoadedData);
        UpdateDisplay();
        Init();
        if (SettingManager.Instance != null)
        {
            if (!SettingManager.Instance.StoredData.ContainsKey(Setting.Name))
            {
                Debug.LogWarning($"Setting '{Setting.Name}' is not declared in the setting manager!");
            }
        }
        else
        {
            Debug.LogWarning("Setting manager missing!");
        }
        if (!SettingManager.HasCalledForLateData && Setting.HasLateDefault)
        {
            yield return null;
            UpdateDisplay();
            Init();
        }
    }

    public abstract void UpdateDisplay();
    public override SettingData GetData() => Setting;
}
public abstract class BaseBaseSettingLol : MonoBehaviour
{
    public abstract SettingData GetData();
}