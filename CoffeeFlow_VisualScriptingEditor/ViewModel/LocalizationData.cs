using System.Collections.Generic;

public class LocalizationData
{
    public List<LocalizationItem> Items;
}

public class LocalizationItem
{
    public string Key { get; set; }
    public string ValueEnglish { get; set; }
    public string ValueJapanese { get; set; }
}