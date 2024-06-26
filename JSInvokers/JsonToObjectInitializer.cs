﻿using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using TruliooExtension.Helpers;

namespace TruliooExtension.JSInvokers;

public static class JsonToObjectInitializer
{
    private static readonly Dictionary<string, MetadataReference> _metadataReferenceCache = new();
    private static readonly string[] _additionalAssemblies = [ "System.Private.CoreLib", "System.Private.Uri", "Newtonsoft.Json" ];
    private static readonly HttpClient _httpClient = new();
    
    [JSInvokable("JsonToObjectInitializer")]
    public static async Task<string> JsonToObjectInitializerInvoker(string extensionID, string @namespace, string className, string csharpClassCode, string json)
    {
        try
        {
            var scriptAssembly = await CompileToDllAssembly(extensionID, csharpClassCode, release: true);
            Type type = scriptAssembly.GetType($"{@namespace}.{className}")!;

            object? obj = JsonConvert.DeserializeObject(json, type);
            return ObjectInitializerHelper.ObjectInitializer(obj);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
    
    private static async Task<MetadataReference> GetAssemblyMetadataReference(string assemblyName, string extensionID)
    {
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
            .Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith("System"))
            .Select(x => x.Name)
            .Concat(_additionalAssemblies)
            .Select(x => GetAssemblyMetadataReference(x!, extensionID)));

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