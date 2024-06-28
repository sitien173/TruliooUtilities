using TruliooExtension.Entities;
using TruliooExtension.JSInvokers;

namespace TruliooExtension.Services;

public interface ICustomFieldGroupService
{
    Task<CustomFieldGroup?> GetAsync(string culture);
    Task SaveAsync(CustomFieldGroup customFieldGroup);
    Task RefreshAsync(string culture);
}

public class CustomFieldGroupService(IStorageService storageService, IGlobalConfigurationService configService, IConfigurationProvider configurationProvider)
    : ICustomFieldGroupService
{
    public async Task<CustomFieldGroup?> GetAsync(string culture)
    {
        var result = await storageService.GetAsync<string, CustomFieldGroup>((await configurationProvider.GetAppSettingsAsync()).Tables.CustomFieldGroup, culture);
        return result;
    }
    
    public async Task SaveAsync(CustomFieldGroup customFieldGroup)
    {
        await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.CustomFieldGroup, customFieldGroup.Culture, customFieldGroup);
    }

    public async Task RefreshAsync(string culture)
    {
        var customFieldGroupGlobalKey = (await configurationProvider.GetAppSettingsAsync()).CustomFieldGroupGlobalKey; 
        if (culture == customFieldGroupGlobalKey)
            return;
        
        var customFieldGroup = await GetAsync(culture) ?? new CustomFieldGroup()
        {
            Culture = culture,
            Enable = true
        };
        
        var customFieldGroupGlobal = await GetAsync(customFieldGroupGlobalKey) ?? new CustomFieldGroup()
        {
            Culture = customFieldGroupGlobalKey,
            Enable = true
        };
        
        var globalConfiguration = await configService.GetAsync();
        customFieldGroup.CustomFields = RefreshCustomFields.GetCustomFields(customFieldGroup, customFieldGroupGlobal, globalConfiguration, false);
        await SaveAsync(customFieldGroup);
    }
}