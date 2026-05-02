public class TextFromInput : TextDisplay
{
    public string PreText = "";
    public string PostText = "";
    public string Splitter = ", ";
    public int SpecificIndex = -1;
    public override string GetDisplayString()
    {
        var shids = InputManager.gamekeys[type].AListToBList((x) => InputManager.keynames[x]);
        string s = PreText;
        if (SpecificIndex >= 0 && SpecificIndex < shids.Count)
        {
            s += shids[SpecificIndex];
        }
        else
        {
            s += shids.ListToString(Splitter);
        }
        s += PostText;
        return s;
    }
}
