using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Blazor.BrowserExtension;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
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
        
        private static Lazy<IServiceProvider> _serviceProvider = new();
        
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
            builder.Services.AddScoped<StoreService>();
            builder.Services.AddScoped<ToastService>();
            builder.Services.AddScoped<DataGenerator>();
            var host = builder.Build();
            
            if (!_serviceProvider.IsValueCreated)
            {
                _serviceProvider = new Lazy<IServiceProvider>(() => host.Services);
            }
            
            await host.RunAsync();
        }
        
        [JSInvokable]
        public static Task<string> GenerateDummyData()
        {
            var dataGenerator = _serviceProvider.Value.GetRequiredService<DataGenerator>();
            return dataGenerator.Generate();
        }
        
        [JSInvokable]
        public static Task SyncData()
        {
            var storeService = _serviceProvider.Value.GetRequiredService<StoreService>();
            return storeService.Sync();
        }
    }
}
