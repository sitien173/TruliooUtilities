using System.Net;
using System.Text;
using HtmlAgilityPack;
using Microsoft.JSInterop;
using TruliooExtension.Helpers;

namespace TruliooExtension.JSInvokers;

public static class PrintDSGroupVariantSetup
{
    [JSInvokable("PrintDSGroupVariantSetup")]
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

        for (int i = 0; i < variant.Count; i++)
        {
            var (fieldName, isRequired, isOptional, isOutput, isAppended, comment) = variant[i];
            if (string.IsNullOrEmpty(comment))
            {
                sb.Append($"\t((int)FieldEnum.{fieldName}, {isRequired.ToString().ToLower()}, {isOptional.ToString().ToLower()}, {isOutput.ToString().ToLower()}, {isAppended.ToString().ToLower()}),  // {i + 1}\n");
            }
            else
            {
                sb.Append($"\t((int)FieldEnum.{fieldName}, {isRequired.ToString().ToLower()}, {isOptional.ToString().ToLower()}, {isOutput.ToString().ToLower()}, {isAppended.ToString().ToLower()}),  // {i + 1} - {WebUtility.HtmlDecode(comment)}\n");
            }
        }

        sb.Append("};");

        return CodeFormatterHelper.FormatCode(sb.ToString());
    }
}