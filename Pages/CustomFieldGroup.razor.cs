using System.Text.Json;
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
    [Inject] private IGlobalConfigurationService GlobalConfigurationService { get; set; }
    
    [Parameter] public string Culture { get; set; }
    [SupplyParameterFromQuery(Name = "cultureName")] public string CultureName { get; set; }
    
    private Lazy<IJSObjectReference> _accessorJsRef = new ();
    private IEnumerable<KeyValuePair<string, string>> _dataFields = [];
    private bool _isAdd;
    private CustomField _model = new ();
    private bool _isLoading;
    private Model.CustomFieldGroup _customFieldGroup = new ();
    
    private IEnumerable<string> ExcludesDataFields => _customFieldGroup.CustomFields.Where(x => x.IsCustomize).Select(x => x.DataField);
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
        _customFieldGroup = await CustomFieldGroupService.GetAsync(Culture) ?? new Model.CustomFieldGroup()
        {
            Culture = Culture,
            Enable = true
        };
        
        if (_customFieldGroup.CustomFields.Count == 0)
        {
            await CustomFieldGroupService.RefreshAsync(Culture);
            _customFieldGroup = await CustomFieldGroupService.GetAsync(Culture) ?? new Model.CustomFieldGroup()
            {
                Culture = Culture,
                Enable = true
            };
        }
        
        _dataFields = FieldFaker.AllFieldName().Select(x => new KeyValuePair<string, string>(x, x));
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new Lazy<IJSObjectReference>(await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/CustomFieldGroup.razor.js"));
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
    
    private async Task ChangeDataField(string val)
    {
        _model.DataField = val;
        _model.Match = (await GlobalConfigurationService.GetAsync())?.MatchTemplate ?? string.Format(ConstantStrings.CustomFieldMatchTemplate, val);
        
        StateHasChanged();
    }

    private async Task PrepareEdit(int index)
    {
        _model = _customFieldGroup.CustomFields[index];
        
        StateHasChanged();
        
        await Task.Delay(10);
        await _accessorJsRef.Value.InvokeVoidAsync("openModal");
    }

    private async Task PrepareAdd()
    {
        _isAdd = true;
        _model = new CustomField()
        {
            IsCustomize = true
        };
        
        StateHasChanged();
        await Task.Delay(10);
        
        await _accessorJsRef.Value.InvokeVoidAsync("openModal");
    }

    private async Task HandleDelete(int index)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this custom field?");

        if (!confirmed)
            return;
        
        _customFieldGroup.CustomFields.RemoveAt(index);
    }

    private Task HandleAdd()
    {
        _customFieldGroup.CustomFields.Add(new CustomField()
        {
            DataField = _model.DataField,
            IsCustomize = _model.IsCustomize,
            Match = _model.Match,
            StaticValue = _model.StaticValue,
            Template = _model.Template,
            GenerateValue = _model.GenerateValue,
            IsIgnore = _model.IsIgnore
        });
        _isAdd = false;
        
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleUpdate()
    {
        var index = _customFieldGroup.CustomFields.FindLastIndex(x => x.IsCustomize && x.DataField == _model.DataField);
        _customFieldGroup.CustomFields[index] = new CustomField()
        {
            DataField = _model.DataField,
            IsCustomize = _model.IsCustomize,
            Match = _model.Match,
            StaticValue = _model.StaticValue,
            Template = _model.Template,
            GenerateValue = _model.GenerateValue,
            IsIgnore = _model.IsIgnore
        };
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnSubmit()
    {
        var task = _isAdd ? HandleAdd() : HandleUpdate();
        await task;
        
        await _accessorJsRef.Value.InvokeVoidAsync("closeModal");
    }

    private async Task SaveChanges()
    {
        _isLoading = true;
        await Task.Delay(10);

        await CustomFieldGroupService.SaveAsync(_customFieldGroup);
        if (Culture == "global")
        {
            var config = await GlobalConfigurationService.GetAsync();
            var currentCulture = config?.CurrentCulture ?? "en";
            Culture = currentCulture;
        }
        
        await CustomFieldGroupService.RefreshAsync(Culture);
        
        _isLoading = false;
        ToastService.ShowSuccess("Changes saved successfully.");
    }
}