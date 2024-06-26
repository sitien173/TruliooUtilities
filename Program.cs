namespace TruliooExtension
{
    using System.Reflection;
    using Common;
    using Blazor.BrowserExtension;
    using Blazored.Toast.Services;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Pages;
    using Services;
    using ConfigurationProvider = TruliooExtension.Services.ConfigurationProvider;
    using IConfigurationProvider = TruliooExtension.Services.IConfigurationProvider;

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

            builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<IStorageService, StorageService>();
            builder.Services.AddSingleton<ICustomFieldGroupService, CustomFieldGroupService>();
            builder.Services.AddSingleton<ILocaleService, LocaleService>();
            builder.Services.AddSingleton<IGlobalConfigurationService, GlobalConfigurationService>();
            builder.Services.AddSingleton<IToastService, ToastService>();
            builder.Services.AddSingleton<IUpdateDatasourceService, UpdateDatasourceService>();
            builder.Services.AddSingleton<ICSPManagerService, CSPManagerService>();
            builder.Services.AddSingleton<IConfigurableJsonParserService, ConfigurableJsonParserServiceService>();
            builder.Services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
            var host = builder.Build();
            
            var extensionEnvironment = host.Services.GetRequiredService<IBrowserExtensionEnvironment>().Mode;
            if (extensionEnvironment != BrowserExtensionMode.ContentScript)
            {
                var services = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(IRunner)))
                    .Select(x => ActivatorUtilities.CreateInstance(host.Services, x));

                foreach (var service in services)
                {
                    service.GetType().GetMethod(nameof(IRunner.RunAsync))?.Invoke(service, null);
                }
            }
            await host.RunAsync();
        }
    }
}
