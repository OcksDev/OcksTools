using TMPro;
using UnityEngine;

public class AutoTextSize : MonoBehaviour
{
    public float FontSize = 36;
    private TextMeshProUGUI boner;
    // Start is called before the first frame update
    private void Start()
    {
        boner = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        boner.fontSize = (FontSize * UnityEngine.Device.Screen.height) / 774;
    }
}
