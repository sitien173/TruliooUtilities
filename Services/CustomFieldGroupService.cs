using AsyncAwaitBestPractices;
using TruliooExtension.JSInvokers;
using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface ICustomFieldGroupService
{
    Task<CustomFieldGroup?> GetAsync(string culture);
    Task SaveAsync(CustomFieldGroup customFieldGroup);
    Task RefreshAsync(string culture);
    Task InitializeAsync();
}

public class CustomFieldGroupService(IStorageService storageService, IGlobalConfigurationService configService, ILocaleService localeService)
    : ICustomFieldGroupService
{
    private const string _key = nameof(CustomFieldGroup);
    public async Task<CustomFieldGroup?> GetAsync(string culture)
    {
        var result = await storageService.GetAsync<string, CustomFieldGroup>(_key, culture);
        return result;
    }
    
    public async Task SaveAsync(CustomFieldGroup customFieldGroup)
    {
        await storageService.SetAsync(_key, customFieldGroup.Culture, customFieldGroup);
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
        
        var globalConfiguration = await configService.GetAsync();
        customFieldGroup.CustomFields = RefreshCustomFields.GetCustomFields(customFieldGroup, customFieldGroupGlobal, globalConfiguration!);
        await SaveAsync(customFieldGroup);
    }

    public async Task InitializeAsync()
    {
        var config = await configService.GetAsync();
        if (config == null)
            return;

        await SaveAsync(new CustomFieldGroup()
        {
            Culture = "global",
            Enable = true
        });
        
        var cultures = config.CurrentCulture;
        await RefreshAsync(cultures);

        var allKeys = await storageService.GetAllKeysAsync(_key);
        var locales = (await localeService.GetLocalesAsync()).Select(x => x.Key).Except(allKeys).ToList();
        foreach (string locale in locales)
        {
            RefreshAsync(locale).SafeFireAndForget();
        }
    }
}