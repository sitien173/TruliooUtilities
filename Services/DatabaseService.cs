using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace trulioo_autofill.Services;

public class DatabaseService<T>
    where T : DbContext
{
    public const string FileName = "/database/app.db";
    private readonly IDbContextFactory<T> _dbContextFactory;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    
    public DatabaseService(IJSRuntime jsRuntime
        , IDbContextFactory<T> dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

        _moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/file.js").AsTask());
    }

    public async Task InitDatabaseAsync()
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("mountAndInitializeDb");
            if (!File.Exists(FileName))
            {
                File.Create(FileName).Close();
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            await dbContext.Database.EnsureCreatedAsync();     
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error initializing database: " + ex.Message);
        }
    }
}