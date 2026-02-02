using System.Collections.Generic;

[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> dict = new();
        foreach (var item in items)
            dict[item.key] = item.value;
        return dict;
    }
}