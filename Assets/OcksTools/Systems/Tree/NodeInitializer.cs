using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInitializer : MonoBehaviour
{
    public bool InitOnAwake = true;
    public bool GetFromChildren = false;
    public List<TreeNode> treeNodes = new List<TreeNode>();
    public List<NodeInitializer> otherInitializers = new List<NodeInitializer>();
    private void Awake()
    {
        if(InitOnAwake)
            InitializeNodes();
    }


    public void InitializeNodes()
    {
        if (GetFromChildren)
        {
            var ss = GetComponentsInChildren<TreeNode>();
            foreach (var a in ss)
            {
                a.InitializeNode();
            }
        }
        else
        {
            foreach (var a in treeNodes)
            {
                a.InitializeNode();
            }
        }
        foreach(var a in otherInitializers)
        {
            a.InitializeNodes();
        }
    }
}
