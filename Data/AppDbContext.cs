using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using trulioo_autofill.Models;

namespace trulioo_autofill.Data;

public class AppDbContext : DbContext
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    public AppDbContext(DbContextOptions<AppDbContext> options
        , IJSRuntime jsRuntime) : base(options)
    {
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/file.js").AsTask());
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CustomField>();
        
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.FieldName)
            .IsRequired()
            .HasMaxLength(100);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await PersistDatabaseAsync(cancellationToken);
        return result;
    }

    private async Task PersistDatabaseAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Start saving database");
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("syncDatabase", cancellationToken, false, cancellationToken);
        Console.WriteLine("Finish save database");
    }
}