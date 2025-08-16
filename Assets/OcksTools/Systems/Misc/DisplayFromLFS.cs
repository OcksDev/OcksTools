using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayFromLFS : MonoBehaviour
{
    public string ParentLang = "";
    public string LangIndex = "";
    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text = LanguageFileSystem.Instance.GetString(ParentLang,LangIndex);
    }
}
