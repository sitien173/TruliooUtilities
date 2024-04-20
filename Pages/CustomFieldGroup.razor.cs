using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CustomFieldGroup : BasePage, IAsyncDisposable
{
    private Model.CustomFieldGroup _currentCustomFieldGroup = new ();
    private List<Model.CustomFieldGroup> _customFieldGroups = new ();
    [Inject] private IJSRuntime jsRuntime { get; set; }
    [Inject] private ToastService toastService { get; set; }
    [Inject] private StoreService storeService { get; set; }
    
    [Parameter] public string Culture { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private CustomField _newCustomField = new ();
    private CustomField _editField = new ();
    private IEnumerable<KeyValuePair<string, string>> dataFields = Enumerable.Empty<KeyValuePair<string, string>>();
    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Culture))
        {
            Culture = "en";
        }
        await base.OnParametersSetAsync();
    }
    
    

    protected override async Task OnInitializedAsync()
    {
        _currentCustomFieldGroup.Culture = Culture;
        
        _customFieldGroups = (await storeService.GetAsync<List<Model.CustomFieldGroup>>(Model.CustomFieldGroup.Key)) ?? new ();
        _currentCustomFieldGroup = _customFieldGroups.Find(x => x.Culture == Culture);
        
        if (_currentCustomFieldGroup is null)
        {
            _currentCustomFieldGroup = new Model.CustomFieldGroup
            {
                Culture = Culture,
                CustomFields = new List<CustomField>()
            };
            _customFieldGroups.Add(_currentCustomFieldGroup);
        }
        
        
        dataFields = Enum.GetValues(typeof(DataField))
            .Cast<DataField>()
            .Select(x => new KeyValuePair<string, string>(x.ToString(), x.ToString()));
        
        await base.OnInitializedAsync();
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldGroup.razor.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await WaitForReference();
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
    private Task ChangeNewModelDataField(string val)
    {
        _newCustomField.DataField = val;
        _newCustomField.Match = val + ", *-" + val;
        return Task.CompletedTask;
    }
    
    private Task ChangeEditModelDataField(string val)
    {
        _editField.DataField = val;
        _editField.Match = val + ", *-" + val;
        return Task.CompletedTask;
    }

    private async Task HandleEdit(string fieldName)
    {
        var field = _currentCustomFieldGroup.CustomFields.First(x => x.DataField == fieldName);
        _newCustomField = field;
        
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("openModal", "customFieldGroupModalEdit");
    }

    private async Task HandleDelete(string fieldName)
    {
        // show confirmation dialog
        bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this custom field?");

        if (!confirmed)
            return;
        
        _currentCustomFieldGroup.CustomFields.Remove(_currentCustomFieldGroup.CustomFields.First(x => x.DataField == fieldName));
        await toastService.ShowSuccess("Success","Custom field deleted successfully.");
        
        await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
    }

    private async Task HandleAdd()
    {
        var existingField = _currentCustomFieldGroup.CustomFields.Find(x => x.DataField == _newCustomField.DataField);
        
        if (existingField is null)
        {
            _currentCustomFieldGroup.CustomFields.Add(_newCustomField);
            await toastService.ShowSuccess("Success","Custom field added successfully.");
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
            await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
        }
        else
        {
            var confirm = await jsRuntime.InvokeAsync<bool>("confirm", "A custom field with this data field already exists. Do you want to update it?");

            if (confirm)
            {
                _editField = _newCustomField;
                await HandleUpdate();
            }
        }
        
        _newCustomField = new (); // reset the new custom field
    }

    private async Task HandleUpdate()
    {
        var existingField = _currentCustomFieldGroup.CustomFields.Find(x => x.DataField == _editField.DataField)!;
        existingField.DataField = _editField.DataField;
        existingField.StaticValue = _editField.StaticValue;
        existingField.Match = _editField.Match;
        existingField.Template = _editField.Template;
        
        await toastService.ShowSuccess("Success","Custom field updated successfully.");
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
        
        await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
    }
}