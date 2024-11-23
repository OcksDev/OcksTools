using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode : MonoBehaviour
{
    public string Name;
    public List<string> Prerequisites = new List<string>();
    [HideInInspector]
    public List<string> RelatedNerds;
    public ViewReq ViewRequirement = ViewReq.AtLeastOne;
    public ViewStates StartState = ViewStates.Hidden;
    [HideInInspector]
    public ViewStates ViewState = ViewStates.Hidden;
    public GameObject CanvasPartner;
    public GameObject LineObject;
    private PartnerScrpt prntr;
    public Dictionary<string, Transform> lines = new Dictionary<string, Transform>();
    private void Awake()
    {
        TreeHandler.Nodes.Add(Name, this);
        ViewState = StartState;
        RelatedNerds = new List<string>(Prerequisites);
        TreeHandler.SpawnPartners.Append(Name, SpawnPartner);
        TreeHandler.LoadCurrentState.Append(Name, UpdateState);
        TreeHandler.SpawnLines.Append(Name, SpawnLines);
        TreeHandler.UpdateLines.Append(Name, UpdateAllLines);
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
         canseeme = false;
        switch (ViewState)
        {
            case ViewStates.Available:
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

    public void PropogatedUpdate()
    {
        UpdateState();
        foreach(var a in RelatedNerds)
        {
            TreeHandler.Nodes[a].UpdateState();
        }
        UpdateAllLines();
        foreach(var a in RelatedNerds)
        {
            if (Prerequisites.Contains(a)) continue;
            TreeHandler.Nodes[a].UpdateAllLines();
            TreeHandler.Nodes[a].UpdatePrereqLines(Name);
        }
    }
    public void UpdateAllLines()
    {
        foreach (var a in lines)
        {
            UpdateLineStatus(a);
        }
    }
    public void UpdatePrereqLines(string exludeme)
    {
        foreach (var a in Prerequisites)
        {
            if (a == exludeme) continue;
            TreeHandler.Nodes[a].UpdateAllLines();
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
    public void SpawnLines()
    {
        foreach(var a in RelatedNerds)
        {
            if (Prerequisites.Contains(a)) continue;
            var li = Instantiate(LineObject, transform.position, Quaternion.identity, TreeHandler.Instance.LineParent.transform);
            lines.Add(a, li.transform);
        }
        foreach (var a in lines)
        {
            UpdateLinePos(a);
        }
    }

    public void UpdateLinePos(KeyValuePair<string, Transform> s)
    {
        var targ = TreeHandler.Nodes[s.Key].transform.position;
        s.Value.position = Vector3.Lerp(transform.position, targ, 0.5f);
        s.Value.rotation = RandomFunctions.PointAtPoint2D(transform.position, targ, 0);
        s.Value.localScale = new Vector3(RandomFunctions.Dist(transform.position, targ), 1, 1);
    }
    public void UpdateLineStatus(KeyValuePair<string, Transform> s)
    {
        if (!TreeHandler .CurrentOwnerships.ContainsKey(Name))
        {
            s.Value.gameObject.SetActive(false);
            return;
        }
        if (TreeHandler.Nodes[s.Key].canseeme)
        {
            s.Value.gameObject.SetActive(true);
            return;
        }
        s.Value.gameObject.SetActive(false);
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
