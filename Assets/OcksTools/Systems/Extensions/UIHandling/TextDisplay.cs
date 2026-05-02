using NaughtyAttributes;
using TMPro;
using UnityEngine;

public abstract class TextDisplay : MonoBehaviour
{
    public string type = "";
    private TextMeshProUGUI jessie;
    public bool UpdateOnEnable = true;
    [ShowIf("UpdateOnEnable")]
    public bool IgnoreProgramStart = false;
    public bool AutomaticallyUpdate = true;
    [ShowIf("AutomaticallyUpdate")]
    public bool UseFixedUpdate = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (jessie == null) jessie = GetComponent<TextMeshProUGUI>();
        if (UpdateOnEnable)
        {
            if (!IgnoreProgramStart)
            {
                ForceUpdate();
            }
            else if (Time.time > 0.1f)
            {
                ForceUpdate();
            }
        }
        EnableCall();
    }
    public virtual void EnableCall() { }
    private void Update()
    {
        if (!AutomaticallyUpdate) return;
        if (!UseFixedUpdate)
        {
            ForceUpdate();
        }
    }
    private void FixedUpdate()
    {
        if (!AutomaticallyUpdate) return;
        if (UseFixedUpdate)
        {
            ForceUpdate();
        }
    }
    public abstract string GetDisplayString();

    public void ForceUpdate()
    {
        jessie.text = GetDisplayString();
    }
}
