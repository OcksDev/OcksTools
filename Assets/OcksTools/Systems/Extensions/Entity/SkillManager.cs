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
            entity.Value.UpdateEntitySkills(Time.deltaTime);
        }
    }
}

public static class ExtensionForEntityOXSForSkills
{
    public static Dictionary<EntityOXS, EntitySkillMiddleMan> SkillsTicking = new Dictionary<EntityOXS, EntitySkillMiddleMan>();
    public static Dictionary<EntityOXS, EntitySkillMiddleMan> SkillsGlobal = new Dictionary<EntityOXS, EntitySkillMiddleMan>();
    public static EntitySkillMiddleMan Skill(this EntityOXS nerd)
    {
        if (SkillsGlobal.ContainsKey(nerd))
        {
            return SkillsGlobal[nerd];
        }
        else
        {
            var a = new EntitySkillMiddleMan(nerd);
            SkillsGlobal.Add(nerd, a);
            nerd.OnKillEvent.Append(99999, "CleanUpSkills", CleanUpSkills);
            return a;
        }
    }

    public static EntitySkillMiddleManReadOnly SkillReadOnly(this EntityOXS nerd)
    {
        if (SkillsGlobal.ContainsKey(nerd))
        {
            return SkillsGlobal[nerd];
        }
        else
        {
            var a = new EntitySkillMiddleMan(nerd);
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





public class EntitySkillMiddleMan : EntitySkillMiddleManReadOnly
{
    public EntitySkillMiddleMan(EntityOXS e) : base(e) { }
    public void UpdateEntitySkills(float time)
    {
        for (int i = Skills.Count - 1; i >= 0; i--)
        {
            Skills[i].Update(time);
        }
    }

    public void Add(Skill eff)
    {

    }

    public void Remove(string name)
    {
        for (int i = Skills.Count - 1; i >= 0; i--)
        {
            if (Skills[i].Name == name)
            {
                Skills.RemoveAt(i);
                break;
            }
        }
        CheckExistence();
    }

    public void RemoveAll(string name)
    {
        for (int i = Skills.Count - 1; i >= 0; i--)
        {
            if (Skills[i].Name == name)
            {
                Skills.RemoveAt(i);
            }
        }
        CheckExistence();
    }

    public void Remove(Skill name)
    {
        Remove(name.Name);
    }

    public void RemoveAll(Skill name)
    {
        RemoveAll(name.Name);
    }
    public void Clear()
    {
        Skills.Clear();
        CheckExistence();
    }

    public void CheckExistence()
    {
        if (Skills.Count == 0)
        {
            ExtensionForEntityOXSForSkills.SkillsTicking.Remove(entity);
        }
    }
}
public class EntitySkillMiddleManReadOnly
{
    public EntityOXS entity;
    public List<Skill> Skills;
    public EntitySkillMiddleManReadOnly(EntityOXS e)
    {
        entity = e;
        Skills = new List<Skill>();
    }
    public Skill Get(string name)
    {
        foreach (var ef in Skills)
        {
            if (name == ef.Name)
            {
                return ef;
            }
        }
        return null;
    }
    public Skill Get(Skill eff)
    {
        return Get(eff.Name);
    }
    public bool Has(string name)
    {
        foreach (var ef in Skills)
        {
            if (name == ef.Name)
            {
                return true;
            }
        }
        return false;
    }
    public bool Has(Skill eff)
    {
        return Has(eff.Name);
    }

}