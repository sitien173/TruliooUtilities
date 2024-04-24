using System.Text.Json;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Services;

public class DataGenerator
{
    public const string Key = "data-generated";
    public const string MatchTemplate = "[id$=\"{0}\" i], [name$=\"{0}\" i], [class$=\"{0}\" i]";
    private readonly StoreService _storeService;
    
    public DataGenerator(StoreService storeService)
    {
        _storeService = storeService;
    }

    public async Task<string> Generate()
    {
        var config = await _storeService.GetAsync<GlobalConfiguration>(GlobalConfiguration.Key) ?? new GlobalConfiguration();
        var culture = config.CurrentCulture;
        var customFieldGroups = await _storeService.GetAsync<List<CustomFieldGroup>>(CustomFieldGroup.Key) ?? new List<CustomFieldGroup>();
        var customFieldGroup = customFieldGroups.FirstOrDefault(x => x.Culture == culture) ?? new CustomFieldGroup()
        {
            Enable = true,
            Culture = culture
        };
        var customFieldGroupGlobal = customFieldGroups.FirstOrDefault(x => x.Culture == "global") ?? new CustomFieldGroup();

        var customFields = new List<CustomField>();
        if (!customFieldGroup.Enable)
        {
            return JsonSerializer.Serialize(customFields, Program.SerializerOptions);
        }
        
        var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroupGlobal, customFieldGroup);
        customFields.AddRange(fieldFaker.GetType().GetProperties().Select(field => new CustomField
        {
            DataField = field.Name,
            Match = customFieldGroupGlobal.CustomFields.Find(x => x.DataField == field.Name)?.Match ?? 
                    customFieldGroup.CustomFields.Find(x => x.DataField == field.Name)?.Match ?? 
                    string.Format(MatchTemplate, field.Name),
            GenerateValue = field.GetValue(fieldFaker)?.ToString()
        }));
        
        return JsonSerializer.Serialize(customFields, Program.SerializerOptions);
    }

}