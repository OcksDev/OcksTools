using System.Collections.Generic;
using UnityEngine;
using static EntityOXS;

public class SkillManager : SingleInstance<SkillManager>
{
    public CompileableDictionaryAlt<string, SkillData> AllSkills = new();
    public override void Awake2()
    {
        AllSkills.Compile((x) => { return x.Name; });
    }
    private void Update()
    {
        foreach (var entity in ExtensionForEntityOXSForSkills.SkillsTicking)
        {
            entity.Value.UpdateContainer(Time.deltaTime);
        }
    }
}

public static class ExtensionForEntityOXSForSkills
{
    public static Dictionary<EntityOXS, SkillContainer> SkillsTicking = new Dictionary<EntityOXS, SkillContainer>();
    public static Dictionary<EntityOXS, SkillContainer> SkillsGlobal = new Dictionary<EntityOXS, SkillContainer>();
    public static SkillContainer Skill(this EntityOXS nerd)
    {
        if (SkillsGlobal.ContainsKey(nerd))
        {
            return SkillsGlobal[nerd];
        }
        else
        {
            var a = new SkillContainer(nerd);
            SkillsGlobal.Add(nerd, a);
            nerd.OnKillEvent.Append(99999, "CleanUpSkills", CleanUpSkills);
            return a;
        }
    }

    public static void CleanUpSkills(EntityOXS nerd, MultiRef<object, EntityType> b)
    {
        if (SkillsTicking.ContainsKey(nerd))
        {
            SkillsTicking.Remove(nerd);
        }
        SkillsGlobal.Remove(nerd);
    }
}





public class SkillContainer : ContainerListStyle<Skill>
{
    public SkillContainer(EntityOXS e)
    {
        entity = e;
        List = new List<Skill>();
    }
    public override void UpdateContainer(float time)
    {
        for (int i = List.Count - 1; i >= 0; i--)
        {
            List[i].Update(time);
        }
    }

    public override void Add(Skill eff)
    {
        if (SkillManager.Instance != null && eff.data == null && SkillManager.Instance.AllSkills.Dict.ContainsKey(eff.Name)) eff.SetDataRefFromManager();

        if (!ExtensionForEntityOXSForSkills.SkillsTicking.ContainsKey(entity))
        {
            ExtensionForEntityOXSForSkills.SkillsTicking.Add(entity, this);
        }

        List.Add(eff);
    }


    public override void CheckExistence()
    {
        if (List.Count == 0)
        {
            ExtensionForEntityOXSForSkills.SkillsTicking.Remove(entity);
        }
    }
}