using TruliooExtension.Entities;

namespace TruliooExtension.Services;

using Common;

public interface IGlobalConfigurationService : IRunner
{
    Task<GlobalConfiguration> GetAsync();
    Task SaveAsync(GlobalConfiguration model);
}
public class GlobalConfigurationService(IStorageService storageService, IConfigurationProvider configurationProvider) : IGlobalConfigurationService
{
    private GlobalConfiguration? _globalConfiguration;
    public async Task<GlobalConfiguration> GetAsync()
    {
        return _globalConfiguration ??= await storageService.GetAsync<string, GlobalConfiguration>((await configurationProvider.GetAppSettingsAsync()).Tables.Config, nameof(GlobalConfiguration));
    }

    public async Task SaveAsync(GlobalConfiguration model)
    {
        await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.Config, nameof(GlobalConfiguration), model);
        _globalConfiguration = model;
    }

    public async Task RunAsync()
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