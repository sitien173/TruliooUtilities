using TruliooExtension.Model;

namespace TruliooExtension.Services;
public interface IGlobalConfigurationService
{
    Task<GlobalConfiguration> GetAsync();
    Task SaveAsync(GlobalConfiguration globalConfiguration);
    Task InitializeAsync();
}
public class GlobalConfigurationService(IStorageService storageService) : IGlobalConfigurationService
{
    private const string _globalConfigurationKey = "GlobalConfiguration";

    public async Task<GlobalConfiguration> GetAsync()
    {
        var result = await storageService.GetAsync<GlobalConfiguration>(_globalConfigurationKey);
        return result;
    }

    public Task SaveAsync(GlobalConfiguration globalConfiguration) =>
        storageService.SetAsync(_globalConfigurationKey, globalConfiguration);

    public async Task InitializeAsync()
    {
        var globalConfiguration = await GetAsync();
        if (globalConfiguration == null)
        {
            await SaveAsync(new GlobalConfiguration());
        }
    }
}