using System.Collections;
using UnityEngine;

public abstract class BaseSettingInput<T> : MonoBehaviour where T : SettingData
{
    public T Setting;
    private void OnEnable()
    {
        if (Time.time < 0.02f) return; // dont update display on game start
        UpdateDisplay();
    }

    private IEnumerator Start()
    {
        if (SaveSystem.Instance != null) yield return new WaitUntil(() => SaveSystem.Instance.LoadedData);
        UpdateDisplay();
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
    }

    public abstract void UpdateDisplay();
}
