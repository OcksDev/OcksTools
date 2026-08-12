using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GISContainer : MonoBehaviour
{
    public string Name = "RandomThingIDK";
    public bool CanBeMultiple = false;
    [NaughtyAttributes.HideIf("CanBeMultiple")]
    public bool SaveLoadData = true;
    [HideInInspector]
    public virtual bool IsAbstract => false;
    [HideInInspector]
    public bool LoadedData = false;
    private void OnDestroy()
    {
        GISLol.Instance.All_Containers.Remove(Name);
        if (!IsAbstract) GISLol.Instance.All_ComplexContainers.Remove(Name);
    }
    [HideInInspector]
    public List<GISItem> saved_items = new List<GISItem>();
    // Start is called before the first frame update

    public abstract void StartCode();
    public virtual bool CanRandomGen => false;
    public virtual void RunRandomGen() { }

    private void Start()
    {
        if (CanBeMultiple)
        {
            var p = GISLol.Instance.ContainerMultiples.GetOrDefine(Name, 0u);
            GISLol.Instance.ContainerMultiples[Name]++;
            Name = Name + p;
        }

        GISLol.Instance.All_Containers.Add(Name, this);
        StartCode();

        if (SaveLoadData && !CanBeMultiple)
        {
            StartCoroutine(WaitForSaveSystem());
        }
        else if (CanRandomGen)
        {
            RunRandomGen();
        }
        else
        {
            SaveTempContents();
        }
    }
    public IEnumerator WaitForSaveSystem()
    {
        yield return new WaitUntil(() => { return SaveSystem.Instance.LoadedData; });
        LoadContents(SaveSystem.ActiveProf);
        LoadedData = true;
    }
    public abstract void UpdateCode();
    public void Update() => UpdateCode();

    public abstract bool SaveTempContents();
    public abstract void LoadTempContents();

    public abstract void SaveContents(SaveProfile dict);

    public string GetName()
    {
        return "cnt_" + Name;
    }

    private void Awake()
    {
        if (SaveLoadData && !CanBeMultiple)
        {
            SaveSystem.SaveAllData.Append($"{GetName()}_save", SaveContents);
        }
    }
    public abstract void LoadContents(SaveProfile dict);

    public abstract int AmountOf(GISItem item, bool usebase = false);
    public abstract int AmountOf(string name);

    public abstract int TotalAmountOfItems();

    public abstract GISItem Add(GISItem item);

    public virtual void AbstractAdd(GISItem item, bool ignore_anim = false) { }

    public abstract void Clear();
    public abstract void Clear(GISItem diedie, bool usebase = true);

    public abstract void Remove(GISItem diedie, int amount, bool usebase = true);

    public abstract void Remove(string name, int amount);
    public abstract void Clear(string name);

    public abstract int IndexOf(GISItem item, bool truecompare = false);

    public abstract int IndexOf(string name);
    protected static SortingMethod _m;
    public abstract void Sort(SortingMethod prefered_sort, bool reversed);
    public static int CompareGISItems(GISItem a, GISItem b)
    {
        int i = 0;
        switch (_m)
        {
            case SortingMethod.Alphabetical:
                i = a.Name.CompareTo(b.Name);
                if (i != 0) return i;
                goto amnt;
            case SortingMethod.Amount:
                i = a.Amount.GetValue().CompareTo(b.Amount.GetValue());
                if (i != 0) return i;
                goto alph;
            default:
                return 0;
            alph: return a.Name.CompareTo(b.Name);
            amnt: return a.Amount.GetValue().CompareTo(b.Amount.GetValue());
        }
    }


    public enum SortingMethod
    {
        Alphabetical,
        Amount,
    }



}
