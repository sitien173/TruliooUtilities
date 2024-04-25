using System.Net.Http.Json;

namespace TruliooExtension.Services;

public interface ILocaleService
{
    Task<IReadOnlyDictionary<string, string>> GetLocalesAsync();
}

public class LocaleService(HttpClient client) : ILocaleService
{
    public async Task<IReadOnlyDictionary<string, string>> GetLocalesAsync()
    {
        var locales = await client.GetFromJsonAsync<List<KeyValuePair<string, string>>>("/jsonData/locale.json");
        return locales!.ToDictionary(x => x.Key, x => x.Value);
    }
}