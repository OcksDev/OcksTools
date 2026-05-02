public class TextFromLFS : TextDisplay
{
    [NaughtyAttributes.HorizontalLine]
    [NaughtyAttributes.InfoBox("Leave ParentName empty to search all namespaces")]
    public string ParentName = "";
    public override string GetDisplayString()
    {
        return LanguageFileSystem.Instance.GetString(ParentName, type);
    }
}
