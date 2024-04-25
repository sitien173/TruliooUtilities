using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface ICustomFieldGroupService
{
    Task<List<CustomFieldGroup>> GetAsync();
    Task<CustomFieldGroup> GetAsync(string culture);
    Task SaveAsync(IEnumerable<CustomFieldGroup> customFieldGroups);
    Task SaveAsync(CustomFieldGroup customFieldGroup);
    Task UpdateAsync(string culture, CustomFieldGroup customFieldGroup);
    Task InitializeAsync();
}

public class CustomFieldGroupService(IStorageService storageService, ILocaleService localeService) : ICustomFieldGroupService
{
    private const string _customFieldGroupsKey = "CustomFieldGroups";

    public async Task<List<CustomFieldGroup>> GetAsync()
    {
        var result = await storageService.GetAsync<List<CustomFieldGroup>>(_customFieldGroupsKey);
        return result ?? [];
    }

    public async Task<CustomFieldGroup> GetAsync(string culture)
    {
        var customFieldGroups = await GetAsync();
        return customFieldGroups.Find(x => x.Culture == culture) ?? new CustomFieldGroup();
    }

    public Task SaveAsync(IEnumerable<CustomFieldGroup> customFieldGroups) =>
        storageService.SetAsync(_customFieldGroupsKey, customFieldGroups);

    public async Task SaveAsync(CustomFieldGroup customFieldGroup)
    {
        var customFieldGroups = await GetAsync();
        var existingCustomFieldGroup = customFieldGroups.Find(x => x.Culture == customFieldGroup.Culture);

        if (existingCustomFieldGroup is null)
        {
            customFieldGroups.Add(customFieldGroup);
        }
        else
        {
            existingCustomFieldGroup = customFieldGroup;
        }

        await SaveAsync(customFieldGroups);
    }

    public async Task UpdateAsync(string culture, CustomFieldGroup customFieldGroup)
    {
        var customFieldGroups = await GetAsync();
        var existingCustomFieldGroup = customFieldGroups.Find(x => x.Culture == culture);

        if (existingCustomFieldGroup is null)
        {
            customFieldGroups.Add(customFieldGroup);
        }
        else
        {
            existingCustomFieldGroup = customFieldGroup;
        }

        await SaveAsync(customFieldGroups);
    }

    public async Task InitializeAsync()
    {
        var customFieldGroups = await GetAsync();
        if (customFieldGroups.Count == 0)
        {
            var locales = await localeService.GetLocalesAsync();
            customFieldGroups.AddRange(locales.Select(locale => new CustomFieldGroup() { Culture = locale.Key, CustomFields = [] }));
            await SaveAsync(customFieldGroups);
        }
    }
}