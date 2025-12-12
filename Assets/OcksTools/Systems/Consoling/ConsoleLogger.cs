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
    private float tim = 0;
    // Update is called once per frame
    private void Update()
    {
        if ((tim += Time.deltaTime) >= 0.04f)
        {
            tim = 0;
            p.text = ConsoleLol.Instance.BackLog;
        }
    }
}
