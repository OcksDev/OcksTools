using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsolRefs : MonoBehaviour
{
    public TMP_InputField input;
    public TextMeshProUGUI backlog;
    public TextMeshProUGUI predictr;
    public ContentSizeFitter backlog_size;
    public UnityEngine.UI.Button fix;
    public Scrollbar scrollbar;
    public ScrollRect scrollview;
    public void Submit()
    {
        ConsoleLol.Instance.Submit(input.text);
    }
    public void NewVal()
    {
        ConsoleLol.Instance.NewVal(input.text);
    }
}
