using System.Net;
using System.Text;
using HtmlAgilityPack;
using Microsoft.JSInterop;
using TruliooExtension.Helpers;

namespace TruliooExtension.JSInvokers;

public static class PrintDSGroupVariantSetup
{
    [JSInvokable("PrintDsGroupVariantSetup")]
    public static string PrintDSGroupVariantSetupInvoker(string dsGroupVariantSetupHtml)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(dsGroupVariantSetupHtml);
        var tables = doc.DocumentNode.SelectNodes("//table[contains(@class, 'confluenceTable')]");
        if (tables == null) 
            return "Failed to read HTML.";

        StringBuilder sb = new();
        int variantIndex = 0;
        foreach (var table in tables)
        {
            var rows = table.SelectNodes(".//tr");
            if (rows == null) 
                continue;
            var variants = VariantHelper.GetVariantsFromRows(rows);
            if (variants.Count != 0)
            {
                sb.AppendLine(VariantInitializer(variants, variantIndex++));
            }
        }

        return sb.ToString();
    }
    
    private static string VariantInitializer(List<(string fieldName, bool isRequired, bool isOptional, bool isOutput, bool isAppended, string comment)> variant, int variantIndex = 0)
    {
        StringBuilder sb = new();
        sb.AppendLine();
        sb.Append($"private readonly List<(int fieldId, bool isRequired, bool isOptional, bool isOutput, bool isAppended)> _variant{variantIndex + 1} = new()\n{{\n");

        int maxLengthCol1 = variant.MaxBy(x => x.fieldName.Length).fieldName.Length + 1;
        int maxLengthCol2 = variant.MaxBy(x => x.isRequired.ToString().Length).isRequired.ToString().Length + 1;
        int maxLengthCol3 = variant.MaxBy(x => x.isOptional.ToString().Length).isOptional.ToString().Length + 1;
        int maxLengthCol4 = variant.MaxBy(x => x.isOutput.ToString().Length).isOutput.ToString().Length + 1;
        int maxLengthCol5 = variant.MaxBy(x => x.isAppended.ToString().Length).isAppended.ToString().Length + 1;
        for (int i = 0; i < variant.Count; i++)
        {
            var (fieldName, isRequired, isOptional, isOutput, isAppended, comment) = variant[i];
            if (string.IsNullOrEmpty(comment))
            {
                sb.AppendFormat("\t((int)FieldEnum.{0}, {1}{2}, {3}{4}, {5}{6}, {7}{8}), {9}// {10}\n", 
                    fieldName, 
                    string.Join(' ', Enumerable.Range(0, maxLengthCol1 - fieldName.Length).Select(_ => string.Empty)),
                    isRequired.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol2 - isRequired.ToString().Length).Select(_ => string.Empty)),
                    isOptional.ToString().ToLower(), 
                    string.Join(' ', Enumerable.Range(0, maxLengthCol3 - isOptional.ToString().Length).Select(_ => string.Empty)),
                    isOutput.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol4 - isOutput.ToString().Length).Select(_ => string.Empty)),
                    isAppended.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol5 - isAppended.ToString().Length).Select(_ => string.Empty)),
                    i + 1);
            }
            else
            {
                sb.AppendFormat("\t((int)FieldEnum.{0}, {1}{2}, {3}{4}, {5}{6}, {7}{8}), {9}// {10}\n", 
                    fieldName, 
                    string.Join(' ', Enumerable.Range(0, maxLengthCol1 - fieldName.Length).Select(_ => string.Empty)),
                    isRequired.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol2 - isRequired.ToString().Length).Select(_ => string.Empty)),
                    isOptional.ToString().ToLower(), 
                    string.Join(' ', Enumerable.Range(0, maxLengthCol3 - isOptional.ToString().Length).Select(_ => string.Empty)),
                    isOutput.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol4 - isOutput.ToString().Length).Select(_ => string.Empty)),
                    isAppended.ToString().ToLower(),
                    string.Join(' ', Enumerable.Range(0, maxLengthCol5 - isAppended.ToString().Length).Select(_ => string.Empty)),
                    $"{i + 1} - {WebUtility.HtmlDecode(comment)}");
            }
        }

        sb.Append("};");

        return sb.ToString();
    }
}