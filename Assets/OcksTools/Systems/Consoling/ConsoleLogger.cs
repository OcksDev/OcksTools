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

    // Update is called once per frame
    void Update()
    {
        p.text = ConsoleLol.Instance.BackLog;
    }
}
