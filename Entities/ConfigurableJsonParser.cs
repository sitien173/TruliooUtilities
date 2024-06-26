namespace TruliooExtension.Entities;

using System.Reflection;
using Common;

public class ConfigurableJsonParser
{
    public string Lang { get; set; }
    public string SerializationType { get; set; }
    public string GenerateNamespace { get; set; }
    public string RootClass { get; set; }
    public string CollectionType { get; set; }
    public string OutputFeature { get; set; }
    public bool GenerateVirtualProperties { get; set; }
    public bool RequireSerializableAttribute { get; set; }
    public bool KeepOriginalPropertyCasing { get; set; }
    public bool MakeAllPropertiesOptional { get; set; }
    public int Version { get; set; }
    public string PropertyDensity { get; set; }
    public string TypeUseForNumerics { get; set; }
    public string TypeUseForAny { get; set; }
    public bool DetectUuID { get; set; }
    
    public bool DetectUrls { get; set; }
    public bool DetectBooleans { get; set; }
    public bool DetectDates { get; set; }
    public bool DetectEnums { get; set; }
    public bool DetectNumbers { get; set; }
    public bool DetectMaps { get; set; }
    public bool IgnoreReferences { get; set; } = true;
    public bool MergeSimilarClasses { get; set; } = true;
    public bool AlphabetizeProperties { get; set; }
    public bool StandardizeIdProperty { get; set; } = true;
    public bool SetDefaultEmptyCollection { get; set; } = true;
    public bool SetDefaultStringEmpty { get; set; } = true;

    public static ConfigurableJsonParser Default()
    {
        return new ConfigurableJsonParser()
        {
            Lang = GetSupportedLanguages().First(x => x.Selected).Id,
            SerializationType = GetSupportedSerializationTypes().First(x => x.Selected).Id,
            GenerateNamespace = Assembly.GetExecutingAssembly().GetName().Name,
            CollectionType = GetSupportedCollectionType().First(x => x.Selected).Id,
            OutputFeature = GetSupportedOutputFeatures().First(x => x.Selected).Id,
            Version = int.Parse(GetSupportedCSharpVersion().First(x => x.Selected).Id),
            PropertyDensity = GetSupportedPropertyDensity().First(x => x.Selected).Id,
            TypeUseForNumerics = GetSupportedTypeUseForNumerics().First(x => x.Selected).Id,
            TypeUseForAny = GetSupportedTypeUseForAny().First(x => x.Selected).Id,
            RootClass = "Example"
        };
    }
    
    public static List<SelectOption> GetSupportedSerializationTypes()
    {
        return
        [
            new SelectOption { Id = "NewtonSoft", Text = "Newtonsoft.Json", Selected = true },
            new SelectOption { Id = "SystemTextJson", Text = "System.Text.Json" }
        ];
    }

    public static List<SelectOption> GetSupportedCSharpVersion()
    {
        return
        [
            new SelectOption { Id = "5", Text = ".NET 5.0" },
            new SelectOption { Id = "6", Text = ".NET 6.0", Selected = true },
        ];
    }
    
    public static List<SelectOption> GetSupportedPropertyDensity()
    {
        return
        [
            new SelectOption { Id = "normal", Text = "Normal", Selected = true },
            new SelectOption { Id = "dense", Text = "Dense" }
        ];
    }
    
    public static List<SelectOption> GetSupportedTypeUseForNumerics()
    {
        return
        [
            new SelectOption { Id = "decimal", Text = "decimal" },
            new SelectOption { Id = "double", Text = "double", Selected = true },
        ];
    }
    
    public static List<SelectOption> GetSupportedTypeUseForAny()
    {
        return
        [
            new SelectOption { Id = "object", Text = "object", Selected = true },
            new SelectOption { Id = "dynamic", Text = "dynamic" },
        ];
    }
    
    public static List<SelectOption> GetSupportedCollectionType()
    {
        return
        [
            new SelectOption { Id = "list", Text = "List" },
            new SelectOption { Id = "array", Text = "Array", Selected = true }
        ];
    }
    
    public static List<SelectOption> GetSupportedOutputFeatures()
    {
        return
        [
            new SelectOption { Id = "complete", Text = "Complete" },
            new SelectOption { Id = "attributes-only", Text = "Attributes Only", Selected = true },
            new SelectOption { Id = "just-types-and-namespace", Text = "Just Types And Namespace" },
            new SelectOption { Id = "just-types", Text = "Just Types" }
        ];
    }
    
    public static List<SelectOption> GetSupportedLanguages()
    {
        return
        [
            new SelectOption { Id = "C", Text = "C (cJSON)", Disabled = true },
            new SelectOption { Id = "C#", Text = "C#", Selected = true },
            new SelectOption { Id = "C++", Text = "C++", Disabled = true },
            new SelectOption { Id = "Crystal", Text = "Crystal", Disabled = true },
            new SelectOption { Id = "Dart", Text = "Dart", Disabled = true },
            new SelectOption { Id = "Elixir", Text = "Elixir", Disabled = true },
            new SelectOption { Id = "Elm", Text = "Elm", Disabled = true },
            new SelectOption { Id = "Flow", Text = "Flow", Disabled = true },
            new SelectOption { Id = "Go", Text = "Go", Disabled = true },
            new SelectOption { Id = "Haskell", Text = "Haskell", Disabled = true },
            new SelectOption { Id = "Java", Text = "Java", Disabled = true },
            new SelectOption { Id = "JavaScript", Text = "JavaScript", Disabled = true },
            new SelectOption { Id = "JavaScript PropTypes", Text = "JavaScript PropTypes", Disabled = true },
            new SelectOption { Id = "JSON Schema", Text = "JSON Schema", Disabled = true },
            new SelectOption { Id = "Kotlin", Text = "Kotlin", Disabled = true },
            new SelectOption { Id = "Objective-C", Text = "Objective-C", Disabled = true },
            new SelectOption { Id = "PHP", Text = "PHP", Disabled = true },
            new SelectOption { Id = "Pike", Text = "Pike", Disabled = true },
            new SelectOption { Id = "Python", Text = "Python", Disabled = true },
            new SelectOption { Id = "Ruby", Text = "Ruby", Disabled = true },
            new SelectOption { Id = "Rust", Text = "Rust", Disabled = true },
            new SelectOption { Id = "Scala3", Text = "Scala3", Disabled = true },
            new SelectOption { Id = "Smithy", Text = "Smithy", Disabled = true },
            new SelectOption { Id = "Swift", Text = "Swift", Disabled = true },
            new SelectOption { Id = "TypeScript", Text = "TypeScript", Disabled = true },
            new SelectOption { Id = "TypeScript Effect Schema", Text = "TypeScript Effect Schema", Disabled = true },
            new SelectOption { Id = "TypeScript Zod", Text = "TypeScript Zod", Disabled = true }
        ];
    }
}
