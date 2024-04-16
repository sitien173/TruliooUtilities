using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using trulioo_autofill.Data;
using trulioo_autofill.Services;

namespace trulioo_autofill
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.UseBrowserExtension(browserExtension =>
            {
                builder.RootComponents.Add<App>("#app");
                builder.RootComponents.Add<HeadOutlet>("head::after");
            });

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddDbContextFactory<AppDbContext>(opt =>
            {
                opt.UseSqlite($"Filename={DatabaseService<AppDbContext>.FileName}");
            });
            builder.Services.AddSingleton<DatabaseService<AppDbContext>>();

            var host = builder.Build();
            
            var dbService = host.Services.GetRequiredService<DatabaseService<AppDbContext>>();
            await dbService.InitDatabaseAsync();
            
            await host.RunAsync();
        }
    }
}
