using Blazor.BrowserExtension.Pages;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TruliooExtension.Model;
using TruliooExtension.Services;

namespace TruliooExtension.Pages;

public partial class CustomFieldGroup : BasePage, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IToastService ToastService { get; set; }
    [Inject] private ICustomFieldGroupService CustomFieldGroupService { get; set; }
    
    [Parameter] public string Culture { get; set; }
    [SupplyParameterFromQuery(Name = "cultureName")] public string CultureName { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IEnumerable<KeyValuePair<string, string>> _dataFields = Enumerable.Empty<KeyValuePair<string, string>>();
    private bool _isAdd;
    private CustomField _model = new ();
    private bool _isEdit;
    private bool _isLoading;
    private Model.CustomFieldGroup _currentCustomFieldGroup = new ();
    private List<Model.CustomFieldGroup> _customFieldGroups = new ();
    
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
        
        _customFieldGroups = (await CustomFieldGroupService.GetAsync()).ToList();
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
        
        _dataFields = Enum.GetValues(typeof(DataField))
            .Cast<DataField>()
            .Select(x => new KeyValuePair<string, string>(x.ToString(), x.ToString()));
        
        await base.OnInitializedAsync();
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldGroup.razor.js"));
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
        _model.Match = string.Format(ConstantStrings.CustomFieldMatchTemplate, val);
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
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this custom field?");

        if (!confirmed)
            return;
        
        _currentCustomFieldGroup.CustomFields.Remove(_currentCustomFieldGroup.CustomFields.First(x => x.DataField == fieldName));
    }

    private async Task HandleAdd()
    {
        _currentCustomFieldGroup.CustomFields.Add(_model);
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
        _model = new CustomField(); // reset the new custom field
        _isAdd = false;
    }

    private async Task HandleUpdate()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
        _model = new CustomField(); // reset the new custom field
        _isEdit = false;
    }

    private Task OnSubmit() => _isAdd ? HandleAdd() : HandleUpdate();

    private async Task SaveChanges()
    {
        _isLoading = true;
        await CustomFieldGroupService.SaveAsync(_customFieldGroups);
        _isLoading = false;
        ToastService.ShowSuccess("Changes saved successfully.");
    }
}