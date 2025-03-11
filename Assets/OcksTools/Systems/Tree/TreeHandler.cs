using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreeNode;

public class TreeHandler : MonoBehaviour
{
    public static Dictionary<string, TreeNode> Nodes = new Dictionary<string, TreeNode>();
    public static Dictionary<string, string> CurrentOwnerships = new Dictionary<string, string>();
    public static OXEvent LoadCurrentState = new OXEvent();
    public static OXEvent SpawnPartners = new OXEvent();
    public static OXEvent SpawnLines = new OXEvent();
    public static OXEvent UpdateLines = new OXEvent();
    public static TreeHandler Instance;
    public Transform PartnerParent;
    public Transform LineParent;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        SaveSystem.LoadAllData.Append(LoadAllTree);
        SaveSystem.SaveAllData.Append(SaveAllTree);
    }
    public void LoadAllTree(string dict)
    {
        //phase 1 - load all the values
        CurrentOwnerships = SaveSystem.Instance.GetDict("TreeData", new Dictionary<string, string>(), dict);
        //phase 2 - the canvas partner objects
        SpawnPartners.Invoke();
        //phase 3 - set the state of the things
        LoadCurrentState.Invoke();
        //phase 4 - all nodes know about their related nodes
        foreach (var node in Nodes)
        {
            foreach (var n in node.Value.Prerequisites)
            {
                Nodes[n].RelateNodes.Add(node.Key);
            }
            foreach (var n in node.Value.LockPrerequisites)
            {
                Nodes[n].RelatedUpdates.Add(node.Key);
            }
        }
        //phase 5 - spawn every line object
        SpawnLines.Invoke();
        //phase 6 - set the state of every line object
        UpdateLines.Invoke();
    }
    public void SaveAllTree(string dict)
    {
        SaveSystem.Instance.SetDict("TreeData", CurrentOwnerships, dict);
    }

    public bool MeetsReqs(List<string> Prerequisites, TreeNode.ViewReq ViewRequirement)
    {
        bool passedreq = false;
        if (Prerequisites.Count == 0) return true;
        switch (ViewRequirement)
        {
            case ViewReq.AtLeastOne:
                foreach (var a in Prerequisites)
                {
                    if (CurrentOwnerships.ContainsKey(a))
                    {
                        passedreq = true;
                        goto weenorus;
                    }
                }
                break;
            case ViewReq.AllOf:
                foreach (var a in Prerequisites)
                {
                    if (!CurrentOwnerships.ContainsKey(a))
                    {
                        passedreq = false;
                        goto weenorus;
                    }
                }
                passedreq = true;
                break;
        }
    weenorus:
        return passedreq;
    }
}
