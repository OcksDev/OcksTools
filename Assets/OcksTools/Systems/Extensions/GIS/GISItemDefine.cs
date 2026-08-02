using System.Collections.Generic;
using UnityEngine;

// Add this attribute right here:
[CreateAssetMenu(fileName = "NewItemDefine", menuName = "Scriptable Objects/OcksTools/Item Data Definition")]
public class GISItemDefine : ScriptableObject
{
    public List<GISItem_Data> Items = new List<GISItem_Data>();
}
