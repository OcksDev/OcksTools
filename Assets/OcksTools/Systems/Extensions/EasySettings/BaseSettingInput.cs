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
    }

    public abstract void UpdateDisplay();
}
