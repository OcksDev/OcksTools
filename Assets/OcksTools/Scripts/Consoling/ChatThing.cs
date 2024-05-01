using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatThing : MonoBehaviour
{
    public string text = "";
    public float deatht = 7;
    public float fadet = 1;
    private float dt = 10;
    private VerticalLayoutGroup penis;
    private TextMeshProUGUI bonbon;
    // Start is called before the first frame update
    void Start()
    {

        // to do for v2: fix multiline text sometimes causing text to flow offscreen

        penis = GetComponentInParent<VerticalLayoutGroup>();
        dt = deatht;
        bonbon= GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        dt -= Time.deltaTime;
        if (dt < 0)
        {
            var fsex = bonbon.color;
            fsex.a -= (1 / fadet) * Time.deltaTime;
            bonbon.color = fsex;
            if(fsex.a <= 0.1f)
            {
                Destroy(gameObject);
            }

        }
        var f = transform.localPosition;
        f.x = bonbon.textBounds.max.x;
        transform.localPosition = f;
        if (penis.transform.childCount > 15 && transform.GetSiblingIndex() == 0)
        {
            Destroy(gameObject);
        }
        
    }
}
