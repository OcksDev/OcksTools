using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Device;

public class AutoTextSize : MonoBehaviour
{
    public float FontSize = 36;
    private TextMeshProUGUI boner;
    // Start is called before the first frame update
    void Start()
    {
        boner= GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        boner.fontSize = (FontSize * UnityEngine.Device.Screen.height) / 774;
    }
}
