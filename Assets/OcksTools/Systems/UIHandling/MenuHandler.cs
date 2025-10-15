using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
            SetStates(state.Menu, state.State);
        }
    }

    private void Start()
    {
        MenuMethods.Invoke();
    }

    public void SetMenuState(string name, bool newstate, bool update = true, bool forced = false)
    {
        StartCoroutine(UpdateMenuStateEnumee(name, newstate, update, forced));
    }

    public IEnumerator UpdateMenuStateEnumee(string name, bool newstate, bool update = true, bool forced = false)
    {
        var cum = CurrentMenuStates[name];
        if (cum.State == newstate) yield break;
        if (!forced && cum.AnimLocked) yield break;

        cum.AnimLocked = true;
        if (!forced && !newstate && cum.ClosingAnimation != null)
        {
            yield return StartCoroutine(cum.ClosingAnimation(cum));
        }

        cum.State = newstate;
        SetStates(cum.Menu, newstate);

        if (!forced && newstate && cum.OpeningAnimation != null)
        {
            yield return StartCoroutine(cum.OpeningAnimation(cum));
        }


        cum.AnimLocked = false;
        if (!update) yield break;
        if (newstate)
        {
            cum.OnOpen.Invoke();
        }
        else
        {
            cum.OnClose.Invoke();
        }
        yield return null;
    }

    public void PlayAnim(string name, System.Func<MenuState, IEnumerator> anim)
    {
        var cum = CurrentMenuStates[name];
        StartCoroutine(AnimSmegging(cum, anim));
    }
    public void PlayAnimOneShot(MenuState cum, System.Func<MenuState, IEnumerator> anim)
    {
        StartCoroutine(AnimSmegging(cum, anim));
    }
    public IEnumerator AnimSmegging(MenuState cum, System.Func<MenuState, IEnumerator> anim)
    {
        if ( cum.AnimLocked) yield break;
        cum.AnimLocked = true;
        yield return StartCoroutine(anim(cum));
        cum.AnimLocked = false;
    }

    public void ResetAllMenus(bool update = true)
    {
        foreach (var a in CurrentMenuStates)
        {
            SetMenuState(a.Key, BaseMenuStates[a.Key].State, update, true);
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
    public void Menu_Disable_Force(string menu)
    {
        SetMenuState(menu, false, true, true);
    }
    public void Menu_Enable_Force(string menu)
    {
        SetMenuState(menu, true, true, true);
    }
    public void Menu_Toggle_Force(string menu)
    {
        SetMenuState(menu, !CurrentMenuStates[menu].State, true, true);
    }
    public static void SetStates(List<GameObject> gms, bool newstate)
    {
        foreach(var a in gms) a.SetActive(newstate);
    }
}

[System.Serializable]
public class MenuState
{
    public string Name;
    public List<GameObject> Menu;
    public bool State;
    public System.Func<MenuState, IEnumerator> OpeningAnimation;
    public System.Func<MenuState, IEnumerator> ClosingAnimation;
    public OXEvent OnOpen = new OXEvent();
    public OXEvent OnClose = new OXEvent();
    [HideInInspector]
    public bool AnimLocked = false;
    public MenuState(MenuState a)
    {
        Name = a.Name;
        Menu = a.Menu;
        State = a.State;
    }
    public MenuState(List<GameObject> nerds)
    {
        Name = "-TEMP-";
        Menu = nerds;
        State = true;
    }
}



public class ExampleMenuAnims
{
    public static IEnumerator PopIn(MenuState cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float overshoot = RandomFunctions.EaseOvershoot(x, 4, 2f);
            foreach (var a in cum.Menu)
            {
                a.transform.localScale = Vector3.one * overshoot;
            }
        }, 0.5f);
    }
    public static IEnumerator TVShut(MenuState cum)
    {
        yield return OXLerp.Linear((x) =>
        {
            float overshootx = RandomFunctions.EaseIn(x,3);
            float overshooty = RandomFunctions.EaseIn(x,9);
            foreach (var a in cum.Menu)
            {
                a.transform.localScale = new Vector3(1 + overshootx, 1-overshooty);
            }
        }, 0.35f);
    }
}



