using TruliooExtension.Model;

namespace TruliooExtension.Services;
public interface IGlobalConfigurationService
{
    Task<GlobalConfiguration?> GetAsync();
    Task SaveAsync(GlobalConfiguration model);
    Task InitializeAsync();
}
public class GlobalConfigurationService(IStorageService storageService) : IGlobalConfigurationService
{
    private const string _key = nameof(GlobalConfiguration);
    public async Task<GlobalConfiguration?> GetAsync()
    {
        var result = await storageService.GetAsync<string, GlobalConfiguration>(ConstantStrings.SettingTable, _key);
        return result;
    }

    public Task SaveAsync(GlobalConfiguration model)
    {
        return storageService.SetAsync<string, GlobalConfiguration>(ConstantStrings.SettingTable, _key, model);
    }

    public async Task InitializeAsync()
    {
        var result = await GetAsync();
        if (result == null)
        {
            await SaveAsync(new GlobalConfiguration());
        }
    }
}