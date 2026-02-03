using NaughtyAttributes;
using TMPro;
using UnityEngine;

public abstract class TextDisplay : MonoBehaviour
{
    public string type = "";
    private TextMeshProUGUI jessie;
    public bool AutomaticallyUpdate = true;
    [ShowIf("AutomaticallyUpdate")]
    public bool UseFixedUpdate = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (jessie == null) jessie = GetComponent<TextMeshProUGUI>();
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
