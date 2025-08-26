using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
