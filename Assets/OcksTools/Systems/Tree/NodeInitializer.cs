using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInitializer : MonoBehaviour
{
    public bool InitOnAwake = true;
    public List<TreeNode> treeNodes = new List<TreeNode>();
    private void Awake()
    {
        if(InitOnAwake)
        InitializeNodes();
    }


    public void InitializeNodes()
    {
        foreach(var a in treeNodes)
        {
            a.InitializeNode();
        }
    }
}
