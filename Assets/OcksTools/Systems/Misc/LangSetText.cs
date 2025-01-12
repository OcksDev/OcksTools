using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LangSetText : MonoBehaviour
{
    public bool DoOnStart = false;
    public bool DoOnEnable = true;
    public bool DoOnFixedUpdate = false;
    List<Thingymambobob> nerds = new List<Thingymambobob>();
    private void OnEnable()
    {
        if(DoOnEnable) SetTexts();
    }
    private void Start()
    {
        if(DoOnStart) SetTexts();
    }
    private void FixedUpdate()
    {
        if(DoOnFixedUpdate) SetTexts();
    }
    public void SetTexts()
    {
        foreach(var a in nerds)
        {
            a.text.text = LanguageFileSystem.Instance.GetString(a.Namespace, a.Key);
        }
    }
}
[System.Serializable]
public class Thingymambobob
{
    public string Namespace;
    public string Key;
    public TextMeshProUGUI text;
}

