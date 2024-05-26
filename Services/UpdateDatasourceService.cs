using System.Net.Http.Json;
using Blazored.Toast.Services;
using Humanizer;
using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface IUpdateDatasourceService {
    Task<Datasource> GetAsync(int datasourceID);
    Task<Datasource> GetLastUpdatedDatasourceAsync();
    Task SaveAsync(Datasource datasource);
}

public class UpdateDatasourceService : IUpdateDatasourceService
{
    private readonly HttpClient _httpClient;
    private readonly IGlobalConfigurationService _globalConfigurationService;
    private readonly ILogger<IUpdateDatasourceService> _logger;
    private readonly IToastService _toastService;
    private readonly IStorageService _storageService;

    public UpdateDatasourceService(HttpClient httpClient, IGlobalConfigurationService globalConfigurationService, ILogger<UpdateDatasourceService> logger, IToastService toastService, IStorageService storageService)
    {
        _httpClient = httpClient;
        _globalConfigurationService = globalConfigurationService;
        _logger = logger;
        _toastService = toastService;
        _storageService = storageService;
    }

    public async Task<Datasource> GetAsync(int datasourceID)
    {
        var config = await _globalConfigurationService.GetAsync();
        
        if(config == null)
        {
            _toastService.ShowError("Global configuration not found");
            return null;
        }
        Datasource? datasource = new();
        try
        {
            var uriBuilder = new UriBuilder(config.AdminPortalEndpoint);
            uriBuilder.Path += $"api-datasources/get/{datasourceID}";

            datasource = await _httpClient.GetFromJsonAsync<Datasource>(uriBuilder.Uri);
            await _storageService.SetAsync(ConstantStrings.SettingTable, "LastFetchDatasourceID", datasourceID.ToString());
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to get datasource id {DatasourceID}", datasourceID);
            _toastService.ShowError("Failed to get datasourceID {datasourceID}. " + e.Message.Humanize());
        }
        return datasource;
    }

    public async Task<Datasource> GetLastUpdatedDatasourceAsync()
    {
        var datasourceID = await _storageService.GetAsync<string, string>(ConstantStrings.SettingTable,"LastFetchDatasourceID");
        if(string.IsNullOrEmpty(datasourceID))
        {
            return null;
        }
        
        return await GetAsync(int.Parse(datasourceID));
    }

    public async Task SaveAsync(Datasource datasource)
    {
        var config = await _globalConfigurationService.GetAsync();
        
        if(config == null)
        {
            _toastService.ShowError("Global configuration not found");
            return;
        }

        try
        {
            var uriBuilder = new UriBuilder(config.AdminPortalEndpoint);
            uriBuilder.Path += "api-datasources/update";

            await _httpClient.PutAsJsonAsync(uriBuilder.Uri, datasource);
            await _storageService.SetAsync(ConstantStrings.SettingTable, "LastUpdatedDatasourceID", datasource.ID);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to save datasource id {DatasourceID}", datasource.ID);
            _toastService.ShowError($"Failed to save datasourceID {datasource.ID}. " + e.Message.Humanize());
        }
    }
}