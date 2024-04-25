using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface IUpdateDsEndpointService
{
    Task<UpdateDatasourceEndpoint> GetAsync();
    Task SaveAsync(UpdateDatasourceEndpoint model);
    Task InitializeAsync();
}

public class UpdateDsEndpointService(IStorageService storageService, IGlobalConfigurationService globalConfigurationService, HttpClient client) : IUpdateDsEndpointService
{
    private const string UpdateDsEndpointKey = "UpdateDsEndpoint";
    public async Task<UpdateDatasourceEndpoint> GetAsync()
    {
        var result = await storageService.GetAsync<UpdateDatasourceEndpoint>(UpdateDsEndpointKey);
        return result;
    }

    public async Task SaveAsync(UpdateDatasourceEndpoint model)
    {
        await storageService.SetAsync(UpdateDsEndpointKey, model);
        
        var config = await globalConfigurationService.GetAsync();
        if (config != null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{config.Endpoint}/DatasourceStatusDetails/UpdateEndpoint?datasourceID={model.DatasourceId}&endpointUrl={model.LiveUrl}&testEndpointUrl={model.TestUrl}");
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            await storageService.SetAsync(UpdateDsEndpointKey, model);
        }
    }

    public async Task InitializeAsync()
    {
        var result = await GetAsync();
        if (result == null)
        {
            await SaveAsync(new UpdateDatasourceEndpoint());
        }
    }
}