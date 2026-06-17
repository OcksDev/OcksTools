using System;
using System.Collections.Generic;
using UnityEngine;

public static class _DialogNPCLoader
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        var g = RandomFunctions.GetListOfInheritors<DialogNPC>();
        foreach (var npc in g)
        {
            if (npc.AutoInit())
            {
                DialogNPC.All.AddOrUpdate(npc.GetName(), npc);
            }
        }
    }
}
public abstract class DialogNPC
{
    public static Dictionary<string, DialogNPC> All = new Dictionary<string, DialogNPC>();

    public virtual bool AutoInit() => true;
    public abstract string GetName();
    public abstract Func<string> ContextSwitcher();
    public Func<string> Context;

    public string GetDialog() => ContextSwitcher()();
}