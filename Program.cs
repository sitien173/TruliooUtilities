using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Blazor.BrowserExtension;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TruliooExtension.Pages;
using TruliooExtension.Services;

namespace TruliooExtension
{
    public static class Program
    {
        public static readonly JsonSerializerOptions? SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.UseBrowserExtension(browserExtension =>
            {
                if (browserExtension.Mode == BrowserExtensionMode.ContentScript)
                {
                    builder.RootComponents.Add<ContentScript>("#TruliooExtAppID");
                }
                else
                {
                    builder.RootComponents.Add<App>("#app");
                    builder.RootComponents.Add<HeadOutlet>("head::after");
                }
            });

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IStorageService, StorageService>();
            builder.Services.AddScoped<ICustomFieldGroupService, CustomFieldGroupService>();
            builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
            builder.Services.AddScoped<ILocaleService, LocaleService>();
            builder.Services.AddScoped<IGlobalConfigurationService, GlobalConfigurationService>();
            builder.Services.AddScoped<IToastService, ToastService>();
            builder.Services.AddScoped<IUpdateDsEndpointService, UpdateDsEndpointService>();
            var host = builder.Build();
            
            var storageService = host.Services.GetRequiredService<IStorageService>();
            await storageService.ClearAsync();
            
            var globalConfigurationService = host.Services.GetRequiredService<IGlobalConfigurationService>();
            await globalConfigurationService.InitializeAsync();
            
            var customFieldGroupService = host.Services.GetRequiredService<ICustomFieldGroupService>();
            await customFieldGroupService.InitializeAsync();
            
            var updateDsEndpointService = host.Services.GetRequiredService<IUpdateDsEndpointService>();
            await updateDsEndpointService.InitializeAsync();
            
            await host.RunAsync();
        }
    }
}
