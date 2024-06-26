using TruliooExtension.Entities;

namespace TruliooExtension.Services;

public interface ICSPManagerService
{
    Task<CSP> GetAsync(int id);
    Task<List<CSP>> GetAllAsync();
    Task SaveAsync(CSP model);
    Task DeleteAsync(int id);
}

public class CSPManagerService(IStorageService storageService, IConfigurationProvider configurationProvider) : ICSPManagerService
{
    public async Task<CSP> GetAsync(int id)
    {
        return await storageService.GetAsync<int, CSP>((await configurationProvider.GetAppSettingsAsync()).Tables.CspManager, id);
    }

    public async Task<List<CSP>> GetAllAsync()
    {
        return await storageService.GetAllAsync<CSP>((await configurationProvider.GetAppSettingsAsync()).Tables.CspManager);
    }

    public async Task SaveAsync(CSP model)
    {
        if (model.Id == 0)
        {
            model.Id = await storageService.NextIdAsync((await configurationProvider.GetAppSettingsAsync()).Tables.CspManager);
        }

        await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.CspManager, model.Id, model);
    }

    public async Task DeleteAsync(int id)
    {
        await storageService.DeleteAsync((await configurationProvider.GetAppSettingsAsync()).Tables.CspManager, id);
    }
}