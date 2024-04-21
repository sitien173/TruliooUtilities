using System.Text.Json;
using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.Services;

public class DataGenerator
{
    public const string Key = "data-generated";
    public const string MatchTemplate = "[id*=\"{0}\" i], [name*=\"{0}\" i], [class*=\"{0}\" i]";
    private readonly StoreService _storeService;
    
    public DataGenerator(StoreService storeService)
    {
        _storeService = storeService;
    }

    public async Task<string> Generate()
    {
        List<CustomField> customFields = [];
        string dataGenerated;
        var config = await _storeService.GetAsync<GlobalConfiguration>(GlobalConfiguration.Key) ?? new ();

        var culture = config.CurrentCulture;
        var customFieldGroups = await _storeService.GetAsync<List<CustomFieldGroup>>(CustomFieldGroup.Key) ?? [];

        var customFieldGroup = customFieldGroups.Find(x => x.Culture == culture);

        if (customFieldGroup == null)
        {
            var fieldFaker = FieldFaker.Generate(culture);

            // Generate custom fields base on the fieldFaker properties
            customFields.AddRange(fieldFaker.GetType().GetProperties().Select(field => new CustomField
            {
                DataField = field.Name, 
                Match = string.Format(MatchTemplate, field.Name), 
                GenerateValue = field.GetValue(fieldFaker)?.ToString()
            }));

            dataGenerated = JsonSerializer.Serialize(customFields, Program.SerializerOptions);
        }
        else
        {
            var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroup);
            customFields.AddRange(fieldFaker.GetType().GetProperties().Select(field => new CustomField
            {
                DataField = field.Name,
                Match = customFieldGroup.CustomFields.Find(x => x.DataField == field.Name)?.Match ?? string.Format(MatchTemplate, field.Name),
                GenerateValue = field.GetValue(fieldFaker)?.ToString()
            }));

            dataGenerated = JsonSerializer.Serialize(customFields, Program.SerializerOptions);
        }

        return dataGenerated;
    }
}