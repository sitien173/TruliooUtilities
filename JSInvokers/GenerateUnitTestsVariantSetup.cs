using System.Text;
using HtmlAgilityPack;
using Microsoft.JSInterop;
using TruliooExtension.Helpers;

namespace TruliooExtension.JSInvokers;

public static class GenerateUnitTestsVariantSetup
{
    [JSInvokable("GenerateUnitTestsVariantSetup")]
    public static string GenerateUnitTestsVariantSetupInvoker(string dsGroupVariantSetupHtml)
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
            var rows = table.SelectNodes(".//tr[position()>1]");
            if (rows == null) 
                continue;
            
            var variants = VariantHelper.GetVariantsFromRows(rows);
            if (variants.Count != 0)
            {
                sb.AppendLine(UnitTestInitializerVariant(variants, variantIndex++));
            }
        }

        return sb.ToString();
    }
    
    private static string UnitTestInitializerVariant(List<(string fieldName, bool isRequired, bool isOptional, bool isOutput, bool isAppended, string comment)> variants, int i)
    {
        StringBuilder sb = new();
        sb.AppendLine();
        sb.AppendLine(@$"\\ variant {i + 1}");

        var fieldTypes = new Dictionary<string, Func<(string, bool, bool, bool, bool, string), bool>>
        {
            { "_requiredFieldsVariant", variant => variant.Item2 },
            { "_optionalFieldsVariant", variant => variant.Item3 },
            { "_outputFieldsVariant", variant => variant.Item4 },
            { "_appendFieldsVariant", variant => variant.Item5 }
        };

        foreach (var fieldType in fieldTypes)
        {
            sb.AppendLine($"private readonly List<{(fieldType.Key == "_outputFieldsVariant" ? "Tuple<FieldGroupEnum?, Field>" : "Field")}> {fieldType.Key}{i + 1} = new() {{");
            foreach (var (fieldName, _, _, _, _, _) in variants.Where(fieldType.Value))
            {
                sb.AppendLine($"\t{(fieldType.Key == "_outputFieldsVariant" ? "Tuple.Create<FieldGroupEnum?, Field>(null, " : "")}FieldEnum.{fieldName}{(fieldType.Key == "_outputFieldsVariant" ? ")" : "")},");
            }

            sb.AppendLine("};");
        }

        return CodeFormatterHelper.FormatCode(sb.ToString());
    }

}