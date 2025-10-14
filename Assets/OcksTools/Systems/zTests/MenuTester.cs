using UnityEngine;

public class MenuTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        MenuHandler.MenuMethods.Append(AssignTestStuff);
    }

    public void AssignTestStuff()
    {
        MenuHandler.CurrentMenuStates["m1"].OnOpen.Append(() => Debug.Log("m1_open") ) ;
        MenuHandler.CurrentMenuStates["m1"].OnClose.Append(() => Debug.Log("m1_close") ) ;
    }
}
