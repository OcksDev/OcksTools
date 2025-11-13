using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static TreeNode;
[Obsolete]
public class TreeExtraObject : MonoBehaviour
{
    /*
     * This is currently non-fuctional
     * 
     * 
     */
    public List<string> Prerequisites = new List<string>();
    public ViewReq ViewRequirement = ViewReq.AtLeastOne;
    public ViewStates StartState = ViewStates.Hidden;
    [HideInInspector]
    public ViewStates ViewState = ViewStates.Hidden;
    private void Awake()
    {
        ViewState = StartState;
        TreeHandler.LoadCurrentState.Append(Tags.GenerateID(), UpdateState);
    }
    public void UpdateState()
    {
        var t = TreeHandler.Instance;
        if (t.MeetsReqs(Prerequisites, ViewRequirement))
        {
            switch (ViewState)
            {
                case ViewStates.Hidden:
                    ViewState = ViewStates.Available;
                    goto Ragg;
                case ViewStates.Available:
                case ViewStates.Locked:
                Ragg:
                    ViewState = ViewStates.Obtained;
                    break;
            }
        }
        else
        {
            ViewState = StartState;
        }
        canseeme = false;
        switch (ViewState)
        {
            case ViewStates.Available:
            case ViewStates.Locked:
            case ViewStates.Obtained:
                canseeme = true;
                break;
            case ViewStates.Hidden:
                break;
        }
        gameObject.SetActive(canseeme);
    }
    [HideInInspector]
    public bool canseeme = false;
}
