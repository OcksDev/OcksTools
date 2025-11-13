using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowContainerContents : MonoBehaviour
{
    public string ItemToFind;
    public GISContainer Container;
    private TextMeshProUGUI se;
    private GISItem thi;
    // Start is called before the first frame update
    void Start()
    {
        se = GetComponent<TextMeshProUGUI>();
        thi = new GISItem(ItemToFind);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        se.text = GISLol.Instance.ItemDict[ItemToFind].Name + ": " + Container.AmountOfItem(thi).ToString();
    }
}
