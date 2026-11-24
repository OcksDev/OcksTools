using UnityEngine;
using UnityEngine.UI;

public class ApplyButton : MonoBehaviour
{
    public SettingData sd;
    private Button b;
    private void Start()
    {
        b = GetComponent<Button>();
        b.interactable = false;
    }
    private void FixedUpdate()
    {
        b.interactable = sd.MarkedChanged;
    }
    public void Apply()
    {
        sd.Apply();
        b.interactable = sd.MarkedChanged;
    }
}
