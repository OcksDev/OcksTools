using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GISDisplay : MonoBehaviour
{
    public GISItem item;
    public Image display;
    public TextMeshProUGUI amnt;
    public bool UpdateOnFixedUpdate = true;
    public void UpdateDisplay()
    {
        if (item == null) item = new GISItem();
        amnt.text = item.Amount > 0 ? "x" + item.Amount : "";
        display.sprite = GISLol.Instance.ItemDict[item.ItemIndex].Sprite;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(UpdateOnFixedUpdate)UpdateDisplay();
    }
}
