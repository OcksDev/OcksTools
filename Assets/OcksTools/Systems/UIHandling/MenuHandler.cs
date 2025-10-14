using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public List<MenuState> Menus = new List<MenuState>();
    public static Dictionary<string, MenuState> BaseMenuStates = new Dictionary<string, MenuState>();
    public static Dictionary<string, MenuState> CurrentMenuStates = new Dictionary<string, MenuState>();
    public static MenuHandler Instance;
    public static OXEvent MenuMethods = new OXEvent();
    private void Awake()
    {
        Instance = this;
        foreach(var state in Menus)
        {
            BaseMenuStates.Add(state.Name, state);
            CurrentMenuStates.Add(state.Name, new MenuState(state));
            state.Menu.SetActive(state.State);
        }
    }

    private void Start()
    {
        MenuMethods.Invoke();
    }

    public void SetMenuState(string name, bool newstate, bool update = true)
    {
        if (CurrentMenuStates[name].State == newstate) return;
        CurrentMenuStates[name].State = newstate;
        CurrentMenuStates[name].Menu.SetActive(newstate);
        if (!update) return;
        if (newstate)
        {
            CurrentMenuStates[name].OnOpen.Invoke();
        }
        else
        {
            CurrentMenuStates[name].OnClose.Invoke();
        }
    }

    public void ResetAllMenus(bool update = true)
    {
        foreach (var a in CurrentMenuStates)
        {
            SetMenuState(a.Key, BaseMenuStates[a.Key].State, update);
        }
    }
    public void Menu_Disable(string menu)
    {
        SetMenuState(menu, false, true);
    }
    public void Menu_Enable(string menu)
    {
        SetMenuState(menu, true, true);
    }
    public void Menu_Toggle(string menu)
    {
        SetMenuState(menu, !CurrentMenuStates[menu].State, true);
    }

}

[System.Serializable]
public class MenuState
{
    public string Name;
    public GameObject Menu;
    public bool State;
    public OXEvent OnOpen = new OXEvent();
    public OXEvent OnClose = new OXEvent();
    public MenuState(MenuState a)
    {
        Name = a.Name;
        Menu = a.Menu;
        State = a.State;
    }
}