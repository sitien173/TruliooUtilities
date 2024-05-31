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
            builder.Services.AddScoped<ILocaleService, LocaleService>();
            builder.Services.AddScoped<IGlobalConfigurationService, GlobalConfigurationService>();
            builder.Services.AddScoped<IToastService, ToastService>();
            builder.Services.AddScoped<IUpdateDatasourceService, UpdateDatasourceService>();
            builder.Services.AddScoped<ICSPManagerService, CSPManagerService>();
            var host = builder.Build();
            
            var extensionEnvironment = host.Services.GetRequiredService<IBrowserExtensionEnvironment>().Mode;
            if (extensionEnvironment != BrowserExtensionMode.ContentScript)
            {
                // Initialize services
                await host.Services.GetRequiredService<IGlobalConfigurationService>().InitializeAsync();
                await host.Services.GetRequiredService<ICustomFieldGroupService>().InitializeAsync();
            }
            await host.RunAsync();
        }
    }
}
