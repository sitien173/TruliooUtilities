using TruliooExtension.Entities;

namespace TruliooExtension.Services;
public interface IGlobalConfigurationService
{
    Task<GlobalConfiguration?> GetAsync();
    Task SaveAsync(GlobalConfiguration model);
    Task InitializeAsync();
}
public class GlobalConfigurationService(IStorageService storageService, IConfigurationProvider configurationProvider) : IGlobalConfigurationService
{
    public async Task<GlobalConfiguration?> GetAsync()
    {
        var result = await storageService.GetAsync<string, GlobalConfiguration>((await configurationProvider.GetAppSettingsAsync()).Tables.Temp, nameof(GlobalConfiguration));
        return result;
    }

    public async Task SaveAsync(GlobalConfiguration model)
    {
        await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.Temp, nameof(GlobalConfiguration), model);
    }

    public async Task InitializeAsync()
    {
        var result = await GetAsync();
        if (result == null)
        {
            var appSettings = await configurationProvider.GetAppSettingsAsync();
            var model = new GlobalConfiguration()
            {
                CurrentCulture = appSettings.DefaultCulture,
                MatchTemplate = appSettings.CustomFieldMatchTemplate,
                AdminPortalEndpoint = appSettings.AdminPortalEndpoint.ToString(),
                RefreshOnFill = true
            };
            await SaveAsync(model);
        }
    }
}