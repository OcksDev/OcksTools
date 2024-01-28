using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxL : MonoBehaviour
{
    public GameObject TextObject;
    public GameObject TitleObject;
    private Image img;
    public List<GameObject> q_gameobjects = new List<GameObject>();
    private TextMeshProUGUI b1;
    private TextMeshProUGUI b2;
    public List<TextMeshProUGUI> q_tmps = new List<TextMeshProUGUI>();
    public string text = "";
    public string title = "";
    public string color = "";
    public string tit_color = "";
    public string bg_color = "";
    public List<string> qs = new List<string>();
    private Color32 co;
    private Color32 ti_co;
    private Color32 bg_co;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
        b1 = TextObject.GetComponent<TextMeshProUGUI>();
        b2 = TitleObject.GetComponent<TextMeshProUGUI>();
        int i = 0;
        foreach(var f in q_gameobjects)
        {
            q_tmps.Add(f.GetComponent<TextMeshProUGUI>());
            i++;
        }
        co = new Color32(255, 255, 255, 255);
    }
    private void OnEnable()
    {
        if(b1 != null)
        {
            qs.Clear();
            qs.Add("");
            qs.Add("");
            qs.Add("");
            qs.Add("");
            text = "";
            title = "";
            color = "255|255|255|255";
            tit_color = "255|255|255|255";
            bg_color = "59|50|84|255";
            UpdateColor();
            UpdateText();
        }
    }


    public void UpdateText()
    {
        b1.text = text;
        b1.color = co;
        b2.text = title;
        b2.color = ti_co;
        img.color = bg_co;
        int i = 0;
        foreach (var f in q_tmps)
        {
            f.gameObject.SetActive(qs[i] != "");
            f.text = qs[i];
            i++;
        }
    }
    public void UpdateColor()
    {
        List<string> list = new List<string>(color.Split("|"));
        List<string> list2 = new List<string>(tit_color.Split("|"));
        List<string> list3 = new List<string>(bg_color.Split("|"));
        co = new Color32(byte.Parse(list[0]), byte.Parse(list[1]), byte.Parse(list[2]), byte.Parse(list[3]));
        ti_co = new Color32(byte.Parse(list2[0]), byte.Parse(list2[1]), byte.Parse(list2[2]), byte.Parse(list2[3]));
        bg_co = new Color32(byte.Parse(list3[0]), byte.Parse(list3[1]), byte.Parse(list3[2]), byte.Parse(list3[3]));
    }
}
