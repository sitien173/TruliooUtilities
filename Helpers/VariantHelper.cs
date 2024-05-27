using HtmlAgilityPack;

namespace TruliooExtension.Helpers;

public static class VariantHelper
{
    public static List<(string fieldName, bool isRequired, bool isOptional, bool isOutput, bool isAppended, string comment)> GetVariantsFromRows(HtmlNodeCollection rows)
    {
        var variants = new List<(string fieldName, bool isRequired, bool isOptional, bool isOutput, bool isAppended, string comment)>();
        foreach (var row in rows)
        {
            var columns = row.SelectNodes(".//td");
            if (columns is not { Count: > 2 }) 
                continue;
            
            int beginIndex = int.TryParse(columns[0].InnerText.Trim(), out _) ? 1 : 0;
            string fieldName = columns[beginIndex].InnerText.Trim().TrimStart('!');
            bool isRequired = IsFieldEnable(columns[beginIndex + 1].InnerText);
            bool isOptional = IsFieldEnable(columns[beginIndex + 2].InnerText);
            bool isOutput = IsFieldEnable(columns[beginIndex + 3].InnerText);
            bool isAppended = IsFieldEnable(columns[beginIndex + 4].InnerText);
            string comment = columns[beginIndex + 5].InnerText.Trim();

            variants.Add((fieldName, isRequired, isOptional, isOutput, isAppended, comment));
        }
        return variants;
    }
    
    private static bool IsFieldEnable(string str)
    {
        string[] enables = ["X", "Y", "YES", "TRUE"];
        return enables.Contains(str.Trim().ToUpper());
    }
}