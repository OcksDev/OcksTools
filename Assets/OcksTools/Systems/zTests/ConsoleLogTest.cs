using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleLogTest : MonoBehaviour
{
    public int amnt = 1;
    public bool weewee = false;
    public TextMeshProUGUI textMeshProUGUI;
    void Update()
    {
        if (weewee)
        {
            for (int i = 0; i < amnt; i++)
            {
                string weet = ConsoleLol.Instance.BackLog + Tags.GenerateID();
                //ConsoleLol.Instance.balls = 1;
            }
        }
        else
        {
            for (int i = 0; i < amnt; i++)
            {
                Console.Log("test");
            }
        }
    }
}
