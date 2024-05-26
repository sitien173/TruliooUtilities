using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface ICustomFieldGroupService
{
    Task<CustomFieldGroup?> GetAsync(string culture);
    Task SaveAsync(CustomFieldGroup customFieldGroup);
    Task RefreshAsync(string culture);
    Task InitializeAsync();
}

public class CustomFieldGroupService(IStorageService storageService, IGlobalConfigurationService configService)
    : ICustomFieldGroupService
{
    public async Task<CustomFieldGroup?> GetAsync(string culture)
    {
        var result = await storageService.GetAsync<string, CustomFieldGroup>(nameof(CustomFieldGroup), culture);
        return result;
    }

    private async Task<List<CustomField>> GetCustomFieldsAsync(CustomFieldGroup customFieldGroup, CustomFieldGroup customFieldGroupGlobal)
    {
        var customFields = new List<CustomField>();

        var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroupGlobal, customFieldGroup);
        var properties = fieldFaker.GetType().GetProperties();
        var config = await configService.GetAsync();
        foreach (var property in properties)
        {
            var val = property.GetValue(fieldFaker)?.ToString();
            if (string.IsNullOrEmpty(val))
                continue;

            var customField = customFieldGroup.CustomFields
                .Concat(customFieldGroupGlobal.CustomFields)
                .LastOrDefault(x => x.IsCustomize && x.DataField == property.Name);
            
            
            var match = customField?.Match ?? config?.MatchTemplate ?? ConstantStrings.CustomFieldMatchTemplate;
            match = string.Format(match, property.Name);

            customFields.Add(new CustomField
            {
                DataField = property.Name,
                Match = match,
                GenerateValue = val,
                StaticValue = customField?.StaticValue,
                Template = customField?.Template,
                IsIgnore = customField?.IsIgnore ?? false,
                IsCustomize = customField?.IsCustomize ?? false
            });
        }

        return customFields;
    }


    public async Task SaveAsync(CustomFieldGroup customFieldGroup)
    {
        await storageService.SetAsync(nameof(CustomFieldGroup), customFieldGroup.Culture, customFieldGroup);
    }

    public async Task RefreshAsync(string culture)
    {
        if (culture == "global")
            return;
        
        var customFieldGroup = await GetAsync(culture) ?? new CustomFieldGroup()
        {
            Culture = culture,
            Enable = true
        };
        
        var customFieldGroupGlobal = await GetAsync("global") ?? new CustomFieldGroup()
        {
            Culture = "global",
            Enable = true
        };
        
        customFieldGroup.CustomFields = await GetCustomFieldsAsync(customFieldGroup, customFieldGroupGlobal);
        await SaveAsync(customFieldGroup);
    }

    public async Task InitializeAsync()
    {
        var config = await configService.GetAsync();
        if (config == null)
            return;
        
        var cultures = config.CurrentCulture;
        await RefreshAsync(cultures);
    }
}