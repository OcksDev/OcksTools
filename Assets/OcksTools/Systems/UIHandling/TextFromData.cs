using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFromData : MonoBehaviour
{
    public string type = "";
    private TextMeshProUGUI jessie;
    public bool UseFixedUpdate = false;
    // Start is called before the first frame update
    void OnEnable()
    {
        jessie= GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!UseFixedUpdate)
        {
            j();
        }
    }
    void FixedUpdate()
    {
        if (UseFixedUpdate)
        {
            j();
        }
    }
    public void j()
    {

        string a = "";
        switch (type)
        {
            case "TestNumber":
                a = Converter.NumToRead("1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
                break;
            case "TestNumber2":
                a = Converter.NumToRead("-1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.169");
                break;
            case "TestNumber3":
                a = Converter.NumToRead("-1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.55", 2);
                break;
            case "TestNumber4":
                a = Converter.NumToRead("-10000.69", 0) + "<br>" + Converter.NumToRead("-10000.69", 1) + "<br>" + Converter.NumToRead("-10000.69", 2) + "<br>";
                break;
            case "TestNumber5":
                a = Converter.NumToRead("3000", 3);
                a +="<br>" + Converter.NumToRead("2345", 3);
                a += "<br>" + Converter.NumToRead("45", 3);
                a += "<br>" + Converter.NumToRead("5", 3);
                a += "<br>" + Converter.NumToRead("3", 3);
                a += "<br>" + Converter.NumToRead("19", 3);
                break;
        }

        jessie.text = a;
    }
}
