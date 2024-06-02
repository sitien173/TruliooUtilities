﻿using System.Net.Http.Json;
using Blazored.Toast.Services;
using TruliooExtension.Entities;

namespace TruliooExtension.Services;

public interface IUpdateDatasourceService {
    Task<Datasource> GetAsync(int datasourceID);
    Task<Datasource> GetLastUpdatedDatasourceAsync();
    Task SaveAsync(Datasource datasource);
    Task<bool> CanConnectAsync();
}

public class UpdateDatasourceService(
    HttpClient httpClient,
    IGlobalConfigurationService globalConfigurationService,
    ILogger<UpdateDatasourceService> logger,
    IToastService toastService,
    IConfigurationProvider configurationProvider,
    IStorageService storageService)
    : IUpdateDatasourceService
{
    public async Task<Datasource> GetAsync(int datasourceID)
    {
        var config = await globalConfigurationService.GetAsync();
        
        if(config == null)
        {
            toastService.ShowError("Global configuration not found");
            return null;
        }
        Datasource? datasource = null;
        try
        {
            var uriBuilder = new UriBuilder(config.AdminPortalEndpoint);
            uriBuilder.Path += (await configurationProvider.GetAppSettingsAsync()).GetDatasourcePath + datasourceID;

            datasource = await httpClient.GetFromJsonAsync<Datasource>(uriBuilder.Uri);
            await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.Temp, (await configurationProvider.GetAppSettingsAsync()).LastFetchDatasourceID, datasourceID.ToString());
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to get datasource id {DatasourceID}", datasourceID);
        }
        return datasource;
    }

    public async Task<Datasource> GetLastUpdatedDatasourceAsync()
    {
        var datasourceID = await storageService.GetAsync<string, string>((await configurationProvider.GetAppSettingsAsync()).Tables.Temp,(await configurationProvider.GetAppSettingsAsync()).LastFetchDatasourceID);
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
            uriBuilder.Path += (await configurationProvider.GetAppSettingsAsync()).UpdateDatasourcePath;

            await httpClient.PutAsJsonAsync(uriBuilder.Uri, datasource);
            await storageService.SetAsync((await configurationProvider.GetAppSettingsAsync()).Tables.Temp, (await configurationProvider.GetAppSettingsAsync()).LastUpdatedDatasourceID, datasource.ID);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to save datasource id {DatasourceID}", datasource.ID);
            toastService.ShowError($"Failed to save datasourceID {datasource.ID}. " + e.Message);
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        return (await GetAsync(1)) != null;
    }
}