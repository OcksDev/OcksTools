using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsolRefs : MonoBehaviour
{
    public TMP_InputField input;
    public Button fix;
    public void Submit()
    {
        ConsoleLol.Instance.Submit(input.text);
    }
}
