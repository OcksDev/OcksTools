using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleLogger : MonoBehaviour
{
    private TextMeshProUGUI p;
    // Start is called before the first frame update
    private void OnEnable()
    {
        p = GetComponent<TextMeshProUGUI>();
    }
    float tim = 0;
    // Update is called once per frame
    void Update()
    {
        if((tim += Time.deltaTime) >= 0.04f)
        {
            tim = 0;
            p.text = ConsoleLol.Instance.BackLog;
        }
    }
}
