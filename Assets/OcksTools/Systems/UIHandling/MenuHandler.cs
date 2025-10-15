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
            if (state.InitialTransforms == null) state.InitialTransforms = new Dictionary<GameObject, MultiRef<Vector3, Vector3, Quaternion>>();
            foreach (var gm in state.Menu)
            {
                if (gm == null) continue;
                state.InitialTransforms.Add(gm, new MultiRef<Vector3,Vector3,Quaternion>
                    (
                        gm.transform.localPosition,
                        gm.transform.localScale,
                        gm.transform.localRotation
                    ));
            }
            BaseMenuStates.Add(state.Name, state);
            CurrentMenuStates.Add(state.Name, new MenuState(state));
            SetStates(state.Menu, state.State);
        }
    }

    private void Start()
    {
        MenuMethods.Invoke();
    }

    public bool GetMenuState(string nerd)
    {
        return CurrentMenuStates[nerd].State;
    }
    public bool GetMenuLocked(string nerd)
    {
        return CurrentMenuStates[nerd].AnimLocked;
    }

    public void SetMenuState(string name, bool newstate, bool update = true, bool forced = false)
    {
        StartCoroutine(UpdateMenuStateEnumee(name, newstate, update, forced));
    }

    public IEnumerator UpdateMenuStateEnumee(string name, bool newstate, bool update = true, bool forced = false)
    {
        var cum = CurrentMenuStates[name];
        if (!forced && cum.State == newstate) yield break;
        if (!forced && cum.AnimLocked) yield break;

        cum.AnimLocked = true;
        LoadInitials(cum);

        if (!forced && !newstate && cum.ClosingAnimation != null)
        {
            yield return StartCoroutine(cum.ClosingAnimation(cum.Menu));
        }

        cum.State = newstate;
        SetStates(cum.Menu, newstate);
        
        if (!forced && newstate && cum.OpeningAnimation != null)
        {
            yield return StartCoroutine(cum.OpeningAnimation(cum.Menu));
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
    public static void LoadInitials(MenuState cum)
    {
        foreach(var gm in cum.Menu)
        {
            var dd = cum.InitialTransforms[gm];
            gm.transform.localPosition = dd.a;
            gm.transform.localScale = dd.b;
            gm.transform.localRotation = dd.c;
        }
    }
    public void PlayAnim(string name, System.Func<List<GameObject>, IEnumerator> anim)
    {
        var cum = CurrentMenuStates[name];
        StartCoroutine(AnimSmegging(cum, anim));
    }
    public void PlayAnimOneShot(List<GameObject> nerds, System.Func<List<GameObject>, IEnumerator> anim)
    {
        StartCoroutine(AnimSmegging(new MenuState(nerds), anim));
    }
    public IEnumerator AnimSmegging(MenuState cum, System.Func<List<GameObject>, IEnumerator> anim)
    {
        if ( cum.AnimLocked) yield break;
        cum.AnimLocked = true;
        yield return StartCoroutine(anim(cum.Menu));
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
    [HideInInspector]
    public Dictionary<GameObject, MultiRef<Vector3, Vector3, Quaternion>> InitialTransforms;
    public bool State;
    public System.Func<List<GameObject>, IEnumerator> OpeningAnimation;
    public System.Func<List<GameObject>, IEnumerator> ClosingAnimation;
    public OXEvent OnOpen = new OXEvent();
    public OXEvent OnClose = new OXEvent();
    [HideInInspector]
    public bool AnimLocked = false;
    public MenuState(MenuState a)
    {
        Name = a.Name;
        Menu = a.Menu;
        State = a.State;
        InitialTransforms = a.InitialTransforms;
    }
    public MenuState(List<GameObject> nerds)
    {
        Name = "-TEMP-";
        Menu = nerds;
        State = true;
    }
}

