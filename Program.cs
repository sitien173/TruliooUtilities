﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace trulioo_autofill
{
    public static class Program
    {
        public static string NapiEndpoint = "https://localhost:44331";
        public static string NapiAuthUserName = "localdev_napi_user";
        public static string NapiAuthPassword = "DevUser123!#"; 
        public static string Culture { get; set; } = "en";
        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.UseBrowserExtension(_ =>
            {
                builder.RootComponents.Add<App>("#app");
                builder.RootComponents.Add<HeadOutlet>("head::after");
            });

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
