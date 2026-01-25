using System.Collections.Generic;

public abstract class ContainerListStyle<T> where T : Containable
{
    public EntityOXS entity;
    public List<T> List;
    public T Get(string name)
    {
        foreach (var ef in List)
        {
            if (name == ef.Name)
            {
                return ef;
            }
        }
        return null;
    }
    public T Get(T eff)
    {
        return Get(eff.Name);
    }
    public bool Has(string name)
    {
        foreach (var ef in List)
        {
            if (name == ef.Name)
            {
                return true;
            }
        }
        return false;
    }
    public bool Has(T eff)
    {
        return Has(eff.Name);
    }
    public void Remove(string name)
    {
        for (int i = List.Count - 1; i >= 0; i--)
        {
            if (List[i].Name == name)
            {
                List.RemoveAt(i);
                break;
            }
        }
        CheckExistence();
    }

    public void RemoveAll(string name)
    {
        for (int i = List.Count - 1; i >= 0; i--)
        {
            if (List[i].Name == name)
            {
                List.RemoveAt(i);
            }
        }
        CheckExistence();
    }

    public void Remove(T name)
    {
        Remove(name.Name);
    }

    public void RemoveAll(T name)
    {
        RemoveAll(name.Name);
    }
    public void Clear()
    {
        List.Clear();
        CheckExistence();
    }
    public abstract void CheckExistence();
    public abstract void UpdateContainer(float time);
    public abstract void Add(T eff);
}
[System.Serializable]
public abstract class Containable
{
    public string Name = "Missing?";
}