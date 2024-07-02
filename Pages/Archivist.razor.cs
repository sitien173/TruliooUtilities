namespace TruliooExtension.Pages;

using System.Text;
using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Services;

public partial class Archivist : BasePage, IAsyncDisposable
{
    [Inject] private IStorageService StorageService { get; set; }
    [Inject] private IConfigurationProvider ConfigurationProvider { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    
    private IBrowserFile? _file { get; set; }
    private IJSObjectReference? _jsModule { get; set; }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Archivist.razor.js");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task Import()
    {
        if (_file is null)
            return;
        
        await using var stream = _file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024);
        var fileContent = await new StreamReader(stream).ReadToEndAsync();
        await StorageService.ImportDatabase(fileContent);
        
        ToastService.ShowSuccess("Imported successfully");
    }

    private async Task Export()
    {
        var appSettings = await ConfigurationProvider.GetAppSettingsAsync();
        Dictionary<string, string> exportData = new();
        foreach (var table in appSettings.Tables.GetType().GetProperties())
        {
            var instance = table.GetValue(appSettings.Tables)?.ToString();
            if (string.IsNullOrEmpty(instance))
                continue;

            var json = await StorageService.ExportDatabase(instance);
            exportData.Add(instance, json);
        }

        var obj = JsonConvert.SerializeObject(exportData);
        var fileStream = new MemoryStream(new UTF8Encoding(true).GetBytes(obj));
        using var streamRef = new DotNetStreamReference(stream: fileStream);
        var fileName = $"{appSettings.AssemblyName}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.json";
        
        ToastService.ShowSuccess("Exported successfully");
        await _jsModule!.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
            await _jsModule.DisposeAsync();
    }
}

