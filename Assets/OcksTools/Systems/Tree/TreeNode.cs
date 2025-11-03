using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode : MonoBehaviour
{
    public string Name;
    public int MaxLevel = 1;
    public LevelReq LevelRequirement = LevelReq.FirstLevel;
    public List<string> Prerequisites = new List<string>();
    [HideInInspector]
    public List<string> RelateNodes;
    [HideInInspector]
    public List<string> RelatedUpdates;
    public ViewReq ViewRequirement = ViewReq.AtLeastOne;
    public List<string> LockPrerequisites = new List<string>();
    public ViewReq UnlockRequirement = ViewReq.AtLeastOne;
    public ViewStates StartState = ViewStates.Hidden;
    [HideInInspector]
    public ViewStates ViewState = ViewStates.Hidden;
    [HideInInspector]
    public GameObject CanvasPartner;
    [HideInInspector]
    public GameObject LineObject;
    private PartnerScrpt prntr;
    public Dictionary<string, Transform> lines = new Dictionary<string, Transform>();



    public void Click()
    {
        //return out of this function to stop a click from going through and getting this node
        ClickEffect();
    }




    private void Awake()
    {
        InitializeNode();
    }
    bool hasinit = false;
    public void InitializeNode()
    {
        if (hasinit) return;
        hasinit = true;
        TreeHandler.Nodes.Add(Name, this);
        ViewState = StartState;
        RelateNodes = new List<string>(Prerequisites);
        TreeHandler.SpawnPartners.Append(Name, SpawnPartner);
        TreeHandler.LoadCurrentState.Append(Name, UpdateState);
        TreeHandler.SpawnLines.Append(Name, SpawnLines);
        TreeHandler.UpdateLines.Append(Name, UpdateAllLines);
    }
    public void UpdateState()
    {
        var t = TreeHandler.Instance;
        bool lockedview = false;
        if (t.MeetsReqs(Prerequisites, ViewRequirement))
        {
            switch (ViewState)
            {
                case ViewStates.Locked:
                    lockedview = true;
                    if(t.MeetsReqs(LockPrerequisites, UnlockRequirement))
                    {
                        ViewState = ViewStates.Available;
                        goto Ragg;
                    }
                    break;
                case ViewStates.Hidden:
                    ViewState = ViewStates.Available;
                    goto Ragg;
                case ViewStates.Available:
                Ragg:
                    if (TreeHandler.MetNodeReqs(this))
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
            case ViewStates.Locked:
                canseeme = lockedview;
                break;
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
        foreach(var a in RelateNodes)
        {
            TreeHandler.Nodes[a].UpdateState();
        }
        foreach(var a in RelatedUpdates)
        {
            TreeHandler.Nodes[a].UpdateState();
        }
        UpdateAllLines();
        foreach(var a in RelateNodes)
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
    public void ClickEffect()
    {
        var t = TreeHandler.Instance;
        if (!t.MeetsReqs(Prerequisites, ViewRequirement))
        {
            return;
        }
        switch (ViewState)
        {
            case ViewStates.Obtained:
                //clicked an already obtained thing
                break;
            case ViewStates.Locked:
                //clicked something that is locked
                break;
            case ViewStates.Available:
                if (TreeHandler.CurrentOwnerships.ContainsKey(Name))
                {
                    TreeHandler.CurrentOwnerships[Name]++;
                }
                else
                {
                    TreeHandler.CurrentOwnerships.Add(Name, 1);
                }
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
        foreach(var a in RelateNodes)
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
        if (!TreeHandler.MetNodeReqs(this))
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
    public enum LevelReq
    {
        FirstLevel,
        MaxLevel,
    }
    public enum ViewStates
    {
        Hidden,
        Locked,
        Available,
        Obtained,
    }

}
