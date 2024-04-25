using TruliooExtension.Model;

namespace TruliooExtension.Services;

public interface ICustomFieldService
{
    Task<List<CustomField>> GetDataGenerateAsync(string culture);
}
public class CustomFieldService(ICustomFieldGroupService customFieldGroupService) : ICustomFieldService
{
    public async Task<List<CustomField>> GetDataGenerateAsync(string culture)
    {
        var customFieldGroups = await customFieldGroupService.GetAsync();
        var customFieldGroup = customFieldGroups.FirstOrDefault(x => x.Culture == culture) ?? new CustomFieldGroup();
        var customFieldGroupGlobal = customFieldGroups.FirstOrDefault(x => x.Culture == "global") ?? new CustomFieldGroup();
        var customFields = new List<CustomField>();
        if (!customFieldGroup.Enable)
        {
            return customFields;
        }
        
        var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroupGlobal, customFieldGroup);
        customFields.AddRange(fieldFaker.GetType().GetProperties().Select(field => new CustomField
        {
            DataField = field.Name,
            Match = customFieldGroupGlobal.CustomFields.Find(x => x.DataField == field.Name)?.Match ?? 
                    customFieldGroup.CustomFields.Find(x => x.DataField == field.Name)?.Match ?? 
                    string.Format(ConstantStrings.CustomFieldMatchTemplate, field.Name),
            GenerateValue = field.GetValue(fieldFaker)?.ToString()
        }));
        
        return customFields;
    }
}