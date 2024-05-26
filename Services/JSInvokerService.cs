using System.Net;
using System.Reflection;
using System.Text;
using CSharpier;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.JSInterop;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace TruliooExtension.Services;

public static class JSInvokerService
{
    private static readonly CodeFormatterOptions _formatOpt = new()
    {
        Width = 250,
        IndentSize = 2,
        EndOfLine = EndOfLine.LF,
        IndentStyle = IndentStyle.Tabs
    };
    private static readonly Dictionary<string, MetadataReference> _metadataReferenceCache = new();
    private static readonly HttpClient _httpClient = new();
    
    [JSInvokable("JsonToObjectInitializer")]
    public static async Task<string> JsonToObjectInitializerInvoker(string extensionID, string csharpClassCode, string json)
    {
        try
        {
            // compile script to in memory dll assembly
            var scriptAssembly = await CompileToDllAssembly(extensionID, csharpClassCode, release: true);
            
            // use reflection to load our type (a shared project with interfaces would help here ... )
            Type type = scriptAssembly.GetType("TruliooExtApp.Example")!;

            object? obj = JsonConvert.DeserializeObject(json, type);
            return ObjectInitializerGenerator.ObjectInitializer(obj);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

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
            var variants = GetVariantsFromRows(rows);
            if (variants.Count != 0)
            {
                sb.AppendLine(VariantInitializer(variants, variantIndex++));
            }
        }

        return sb.ToString();
    }
    
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
            var variants = GetVariantsFromRows(rows);
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

        return CodeFormatter.Format(sb.ToString(), _formatOpt).Code;
    }

    private static List<(string fieldName, bool isRequired, bool isOptional, bool isOutput, bool isAppended, string comment)> GetVariantsFromRows(HtmlNodeCollection rows)
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

        return CodeFormatter.Format(sb.ToString(), _formatOpt).Code;
    }
    
    private static async Task<MetadataReference> GetAssemblyMetadataReference(Assembly assembly, string extensionID)
    {
        string assemblyName = assembly.GetName().Name ?? throw new ArgumentNullException(nameof(assembly), "Assembly name is null");

        if (_metadataReferenceCache.TryGetValue(assemblyName, out MetadataReference? cachedReference))
            return cachedReference;

        string assemblyUrl = $"chrome-extension://{extensionID}/framework/{assemblyName}.dll";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(assemblyUrl);
            if (response.IsSuccessStatusCode)
            {
                byte[] assemblyBytes = await response.Content.ReadAsByteArrayAsync();
                cachedReference = MetadataReference.CreateFromImage(assemblyBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load metadata reference for {assemblyName}: {ex.Message}");
        }

        _metadataReferenceCache[assemblyName] = cachedReference ?? throw new Exception("Reference metadata not found. If using .NET 8, <WasmEnableWebcil>false</WasmEnableWebcil> must be set in the project .csproj file.");
        return cachedReference;
    }

    private static async Task<Assembly> CompileToDllAssembly(
        string extensionID,
        string sourceCode,
        string assemblyName = "",
        bool release = true,
        SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
    {
        assemblyName = string.IsNullOrEmpty(assemblyName) ? Path.GetRandomFileName() : assemblyName;

        SourceText sourceText = SourceText.From(sourceCode);
        CSharpParseOptions parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest)
            .WithKind(sourceCodeKind);

        SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, parseOptions);

        MetadataReference[] references = await Task.WhenAll(Assembly.GetEntryAssembly()!
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Concat(new[] { typeof(object).Assembly, typeof(Uri).Assembly })
            .Select(assembly => GetAssemblyMetadataReference(assembly, extensionID)));

        CSharpCompilationOptions compilationOptions = new(
            OutputKind.DynamicallyLinkedLibrary,
            concurrentBuild: false,
            optimizationLevel: release ? OptimizationLevel.Release : OptimizationLevel.Debug);

        CSharpCompilation compilation = (sourceCodeKind == SourceCodeKind.Script)
            ? CSharpCompilation.CreateScriptCompilation(assemblyName, syntaxTree, references, compilationOptions)
            : CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, compilationOptions);

        using MemoryStream memoryStream = new();
        EmitResult emitResult = compilation.Emit(memoryStream);

        if (!emitResult.Success)
        {
            string errors = string.Join(Environment.NewLine, emitResult.Diagnostics
                .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                .Select(d =>
                {
                    LinePosition position = d.Location.GetLineSpan().StartLinePosition;
                    return $"Line {position.Line}, Col {position.Character}: {d.Id} - {d.GetMessage()}";
                }));

            throw new Exception(errors);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(memoryStream.ToArray());
    }
}