using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
    [Inject] private DataGenerator dataGenerator { get; set; }
    
    [Parameter] public string Culture { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IEnumerable<KeyValuePair<string, string>> dataFields = Enumerable.Empty<KeyValuePair<string, string>>();
    private bool _isAdd;
    private CustomField _model = new ();
    private bool _isEdit;
    
    private IEnumerable<string> ExcludesDataFields => _currentCustomFieldGroup.CustomFields.Select(x => x.DataField);

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
        
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("openModal");
        
        await base.OnAfterRenderAsync(firstRender);
    }
    
    private Task ChangeDataField(string val)
    {
        _model.DataField = val;
        _model.Match = string.Format(DataGenerator.MatchTemplate, val);
        return Task.CompletedTask;
    }

    private Task HandleEdit(string fieldName)
    {
        _isEdit = true;
        _model = _currentCustomFieldGroup.CustomFields.Find(x => x.DataField == fieldName)!;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleAddNew()
    {
        _isAdd = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleDelete(string fieldName)
    {
        bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this custom field?");

        if (!confirmed)
            return;
        
        _currentCustomFieldGroup.CustomFields.Remove(_currentCustomFieldGroup.CustomFields.First(x => x.DataField == fieldName));
        await toastService.ShowSuccess("Success","Custom field deleted successfully.");
        
        await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
    }

    private async Task HandleAdd()
    {
        _currentCustomFieldGroup.CustomFields.Add(_model);
        await toastService.ShowSuccess("Success","Custom field added successfully.");
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
        await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
        
        _model = new CustomField(); // reset the new custom field
        _isAdd = false;
    }

    private async Task HandleUpdate()
    {
        await toastService.ShowSuccess("Success","Custom field updated successfully.");
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
        
        await storeService.SetAsync(Model.CustomFieldGroup.Key, _customFieldGroups);
        
        _model = new CustomField(); // reset the new custom field
        _isEdit = false;
    }
    
    private async Task OnSubmit(EditContext editContext)
    {
        if (editContext.Validate())
        {
            if (_isAdd)
            {
                await HandleAdd();
            }
            else if (_isEdit)
            {
                await HandleUpdate();
            }
            
            var generate = await dataGenerator.Generate();
            await storeService.SetAsync(DataGenerator.Key, generate);
        }
    }
}