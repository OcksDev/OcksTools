public class TextFromData : TextDisplay
{
    public override string GetDisplayString()
    {
        string a = "";
        switch (type)
        {
            case "TestNumber":
                a = Converter.NumToRead("1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
                break;
            case "TestNumber2":
                a = Converter.NumToRead("-1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.169");
                break;
            case "TestNumber3":
                a = Converter.NumToRead("-1032040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.55", 2);
                break;
            case "TestNumber4":
                a = Converter.NumToRead("-10000.69", 0) + "<br>" + Converter.NumToRead("-10000.69", 1) + "<br>" + Converter.NumToRead("-10000.69", 2) + "<br>";
                break;
            case "TestNumber5":
                a = Converter.NumToRead("3000", 3);
                a += "<br>" + Converter.NumToRead("2345", 3);
                a += "<br>" + Converter.NumToRead("45", 3);
                a += "<br>" + Converter.NumToRead("5", 3);
                a += "<br>" + Converter.NumToRead("3", 3);
                a += "<br>" + Converter.NumToRead("19", 3);
                break;
            case "FPS":
                a = "FPS: " + FPSLightweightReader.Instance.LastOutgoingFPS;
                break;
        }
        return a;
    }
}
