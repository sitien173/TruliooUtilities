using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface ICSPManagerService
{
    Task<CSP> GetAsync(int id);
    Task<List<CSP>> GetAllAsync();
    Task SaveAsync(CSP model);
    Task DeleteAsync(int id);
}

public class CSPManagerService(IStorageService storageService) : ICSPManagerService
{
    private const string _key = nameof(CSP);
    public async Task<CSP> GetAsync(int id)
    {
        var csp = await storageService.GetAsync<int, CSP>(_key, id);
        return csp ?? new CSP();
    }

    public Task<List<CSP>> GetAllAsync()
    {
        return storageService.GetAllAsync<CSP>(_key);
    }

    public async Task SaveAsync(CSP model)
    {
        if (model.Id == 0)
        {
            model.Id = await storageService.NextIdAsync(_key);
        }

        await storageService.SetAsync(_key, model.Id, model);
    }

    public Task DeleteAsync(int id)
    {
        return storageService.DeleteAsync(_key, id);
    }
}