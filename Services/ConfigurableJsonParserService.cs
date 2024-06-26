namespace TruliooExtension.Services;

using Common;
using Entities;

public interface IConfigurableJsonParserService : IRunner
{
    Task<ConfigurableJsonParser?> GetAsync();
    Task SaveAsync(ConfigurableJsonParser model);
}

public class ConfigurableJsonParserServiceService(IStorageService storageService, IConfigurationProvider configurationProvider) : IConfigurableJsonParserService
{
    private ConfigurableJsonParser? _configurableJsonParser;
    public async Task<ConfigurableJsonParser?> GetAsync()
    {
        return _configurableJsonParser ??= await storageService.GetAsync<string, ConfigurableJsonParser>((await configurationProvider.GetAppSettingsAsync()).Tables.Config, nameof(ConfigurableJsonParser));
    }
    public async Task SaveAsync(ConfigurableJsonParser model)
    {
        await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.Config, nameof(ConfigurableJsonParser), model);
        _configurableJsonParser = model;
    }
    
    public async Task RunAsync()
    {
        var result = await GetAsync();
        if (result == null)
        {
            await SaveAsync(ConfigurableJsonParser.Default());
        }
    }
}
