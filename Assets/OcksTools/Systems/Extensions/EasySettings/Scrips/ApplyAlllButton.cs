using UnityEngine;
using UnityEngine.UI;

public class ApplyAlllButton : MonoBehaviour
{
    private Button b;
    private void Start()
    {
        b = GetComponent<Button>();
        b.interactable = false;
    }
    private void FixedUpdate()
    {
        b.interactable = SettingManager.Instance.AnyMarkedForChange();
    }
    public void Apply()
    {
        SettingManager.Instance.ApplyAll();
        b.interactable = SettingManager.Instance.AnyMarkedForChange();
    }
}
