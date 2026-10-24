using System.Collections.Generic;
using UnityEngine;
using static TreeNode;

public class TreeHandler : SingleInstance<TreeHandler>
{
    public static Dictionary<string, TreeNode> Nodes = new Dictionary<string, TreeNode>();
    public static Dictionary<string, int> CurrentOwnerships = new Dictionary<string, int>();
    public static OXEvent LoadCurrentState = new OXEvent();
    public static OXEvent SpawnPartners = new OXEvent();
    public static OXEvent SpawnLines = new OXEvent();
    public static OXEvent UpdateLines = new OXEvent();
    public Transform PartnerParent;
    public Transform LineParent;
    public override void Awake2()
    {
        SaveSystem.LoadAllData.Append(LoadAllTree);
        SaveSystem.SaveAllData.Append(SaveAllTree);
    }
    public void LoadAllTree(SaveProfile dict)
    {
        //phase 1 - load all the values
        CurrentOwnerships = dict.GetDict("TreeData", new Dictionary<string, int>());
        //phase 2 - the canvas partner objects
        SpawnPartners.Invoke();
        //phase 3 - all nodes know about their related nodes
        //(done before the state update so backward unlocking can see dependents)
        foreach (var node in Nodes)
        {
            foreach (var n in node.Value.Prerequisites)
            {
                if (!Nodes[n].RelateNodes.Contains(node.Key)) Nodes[n].RelateNodes.Add(node.Key);
            }
            foreach (var n in node.Value.LockPrerequisites)
            {
                if (!Nodes[n].RelatedUpdates.Contains(node.Key)) Nodes[n].RelatedUpdates.Add(node.Key);
            }
        }
        //phase 4 - update the state of nodes to make sure they all know their state
        LoadCurrentState.Invoke();
        //phase 5 - spawn every line object
        SpawnLines.Invoke();
        //phase 6 - set the state of every line object
        UpdateLines.Invoke();
    }
    public void SaveAllTree(SaveProfile dict)
    {
        dict.SetDict("TreeData", CurrentOwnerships);
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
                    if (MetNodeReqs(a))
                    {
                        passedreq = true;
                        goto weenorus;
                    }
                }
                break;
            case ViewReq.AllOf:
                foreach (var a in Prerequisites)
                {
                    if (!MetNodeReqs(a))
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

    /// <summary>
    /// True if any node that has this node as a prerequisite is obtained and
    /// has BidirectionalPrereq enabled. That lets the player work backwards.
    /// </summary>
    public bool BackwardUnlocked(TreeNode node)
    {
        foreach (var name in node.RelateNodes)
        {
            var other = Nodes[name];
            //only dependents (nodes that list us as a prerequisite) count
            if (!other.Prerequisites.Contains(node.Name)) continue;
            if (other.BidirectionalPrereq && MetNodeReqs(other)) return true;
        }
        return false;
    }

    public static int GetNodeLevel(string node)
    {
        if (!CurrentOwnerships.ContainsKey(node)) return 0;
        return CurrentOwnerships[node];
    }
    public static int GetNodeLevel(TreeNode node)
    {
        if (!CurrentOwnerships.ContainsKey(node.Name)) return 0;
        return CurrentOwnerships[node.Name];
    }
    public static bool MetNodeReqs(string node)
    {
        return MetNodeReqs(Nodes[node]);
    }
    public static bool MetNodeReqs(TreeNode node)
    {
        return CurrentOwnerships.ContainsKey(node.Name) && NodeHasMetLevel(node);
    }

    public static bool NodeHasMetLevel(string node)
    {
        return NodeHasMetLevel(Nodes[node]);
    }
    public static bool NodeHasMetLevel(TreeNode node)
    {
        switch (node.LevelRequirement)
        {
            case LevelReq.FirstLevel: return CurrentOwnerships[node.Name] >= 1;
            case LevelReq.MaxLevel: return CurrentOwnerships[node.Name] >= node.MaxLevel;

            default: return false; // not reachable code lol
        }
    }
}
