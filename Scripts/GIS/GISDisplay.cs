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

    private void Awake()
    {
        if (item == null) item = new GISItem();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        amnt.text = item.Amount > 0?"x" + item.Amount:"";
        display.sprite = GISLol.Instance.ItemImages[item.ItemIndex];
    }
}
