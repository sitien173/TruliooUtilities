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

public class UpdateDatasourceService(
    HttpClient httpClient,
    IGlobalConfigurationService globalConfigurationService,
    ILogger<UpdateDatasourceService> logger,
    IToastService toastService,
    IStorageService storageService)
    : IUpdateDatasourceService
{
    private readonly ILogger<IUpdateDatasourceService> _logger = logger;

    public async Task<Datasource> GetAsync(int datasourceID)
    {
        var config = await globalConfigurationService.GetAsync();
        
        if(config == null)
        {
            toastService.ShowError("Global configuration not found");
            return null;
        }
        Datasource? datasource = new();
        try
        {
            var uriBuilder = new UriBuilder(config.AdminPortalEndpoint);
            uriBuilder.Path += $"api-datasources/get/{datasourceID}";

            datasource = await httpClient.GetFromJsonAsync<Datasource>(uriBuilder.Uri);
            await storageService.SetAsync(ConstantStrings.SettingTable, "LastFetchDatasourceID", datasourceID.ToString());
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to get datasource id {DatasourceID}", datasourceID);
            toastService.ShowError("Failed to get datasourceID {datasourceID}. " + e.Message.Humanize());
        }
        return datasource;
    }

    public async Task<Datasource> GetLastUpdatedDatasourceAsync()
    {
        var datasourceID = await storageService.GetAsync<string, string>(ConstantStrings.SettingTable,"LastFetchDatasourceID");
        if(string.IsNullOrEmpty(datasourceID))
        {
            return null;
        }
        
        return await GetAsync(int.Parse(datasourceID));
    }

    public async Task SaveAsync(Datasource datasource)
    {
        var config = await globalConfigurationService.GetAsync();
        
        if(config == null)
        {
            toastService.ShowError("Global configuration not found");
            return;
        }

        try
        {
            var uriBuilder = new UriBuilder(config.AdminPortalEndpoint);
            uriBuilder.Path += "api-datasources/update";

            await httpClient.PutAsJsonAsync(uriBuilder.Uri, datasource);
            await storageService.SetAsync(ConstantStrings.SettingTable, "LastUpdatedDatasourceID", datasource.ID);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to save datasource id {DatasourceID}", datasource.ID);
            toastService.ShowError($"Failed to save datasourceID {datasource.ID}. " + e.Message.Humanize());
        }
    }
}