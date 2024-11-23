using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode : MonoBehaviour
{
    public string Name;
    public List<string> Prerequisites = new List<string>();
    //[HideInInspector]
    public List<string> RelatedNerds;
    public ViewReq ViewRequirement = ViewReq.AtLeastOne;
    public ViewStates StartState = ViewStates.Hidden;
    //[HideInInspector]
    public ViewStates ViewState = ViewStates.Hidden;
    public GameObject CanvasPartner;
    private PartnerScrpt prntr;
    private void Awake()
    {
        TreeHandler.Nodes.Add(Name, this);
        ViewState = StartState;
        RelatedNerds = new List<string>(Prerequisites);
        TreeHandler.SpawnPartners.Append(Name, SpawnPartner);
        TreeHandler.LoadCurrentState.Append(Name, UpdateState);
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
                Ragg:
                    if (TreeHandler.CurrentOwnerships.ContainsKey(Name))
                    {
                        ViewState = ViewStates.Obtained;
                    }
                    break;
            }
        }
        else
        {
            ViewState = StartState;
        }
        switch (ViewState)
        {
            case ViewStates.Available:
            case ViewStates.Obtained:
                gameObject.SetActive(true);
                break;
            case ViewStates.Hidden:
                gameObject.SetActive(false);
                break;
        }
    }

    public void PropogatedUpdate()
    {
        UpdateState();
        foreach(var a in RelatedNerds)
        {
            TreeHandler.Nodes[a].UpdateState();
        }
    }

    public void Click()
    {
        var t = TreeHandler.Instance;
        if (!t.MeetsReqs(Prerequisites, ViewRequirement))
        {
            return;
        }
        switch(ViewState)
        {
            case ViewStates.Obtained:
                //clicked an already obtained thing
                break;
            case ViewStates.Available:
                TreeHandler.CurrentOwnerships.Add(Name, "");
                PropogatedUpdate();
                break;
        }
    }

    public void SpawnPartner()
    {
        CanvasPartner = Instantiate(CanvasPartner, transform.position, transform.rotation, TreeHandler.Instance.PartnerParent.transform);
        prntr = CanvasPartner.GetComponent<PartnerScrpt>();
        prntr.Partner = this;
    }
    private void OnEnable()
    {
        CanvasPartner.SetActive(true);
    }
    private void OnDisable()
    {
        if (CanvasPartner != null) CanvasPartner.SetActive(false);
    }
    private void OnDestroy()
    {
        Destroy(CanvasPartner);
    }

    public enum ViewReq
    {
        AtLeastOne,
        AllOf,
    }
    public enum ViewStates
    {
        Hidden,
        Locked, // unused
        ShownButDisabled, // unused
        Available,
        Obtained,
    }
}
