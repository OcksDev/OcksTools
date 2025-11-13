using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeybindInput : MonoBehaviour
{
    public KeyCode keyCode;
    public SettingInput me;
    [HideInInspector]
    public bool CurrentlySelecting = false;
    public void ChangeKeybind()
    {
        if (CurrentlySelecting) return;
        StartCoroutine(WaitForKeybind());
    }

    public IEnumerator WaitForKeybind()
    {
        if (CurrentlySelecting) yield break;
        CurrentlySelecting = true;
        yield return new WaitUntil(() =>{ return InputManager.GetAllCurrentlyPressedKeys().Count > 0; });
        var aa = InputManager.GetAllCurrentlyPressedKeys();
        if (aa[0]== KeyCode.Escape)
        {
            CurrentlySelecting = false;
            yield break;
        }
        keyCode = aa[0];
        yield return new WaitUntil(() => { return !Input.GetKey(KeyCode.Mouse0); });
        me.WriteValue();
        CurrentlySelecting = false;
    }
}
