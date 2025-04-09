using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInitializer : MonoBehaviour
{
    public bool InitOnAwake = true;
    public bool GetFromChildren = false;
    public List<TreeNode> treeNodes = new List<TreeNode>();
    private void Awake()
    {
        if(InitOnAwake)
            InitializeNodes();
        if (GetFromChildren)
        {
            var ss = GetComponentsInChildren<TreeNode>();
            foreach(var a in ss)
            {
                a.InitializeNode();
            }
        }
    }


    public void InitializeNodes()
    {
        foreach(var a in treeNodes)
        {
        }
    }
}
