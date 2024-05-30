using Microsoft.JSInterop;
using TruliooExtension.Model;

namespace TruliooExtension.JSInvokers;

public static class RefreshCustomFields
{
    [JSInvokable("RefreshCustomFields")]
    public static List<CustomField> GetCustomFields(CustomFieldGroup customFieldGroup, CustomFieldGroup customFieldGroupGlobal, GlobalConfiguration globalConfiguration)
    {
        var customFields = new List<CustomField>();

        var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroupGlobal, customFieldGroup);
        var properties = fieldFaker.GetType().GetProperties();
        foreach (var property in properties)
        {
            var val = property.GetValue(fieldFaker)?.ToString();
            if (string.IsNullOrEmpty(val))
                continue;

            var customField = customFieldGroup.CustomFields
                .Concat(customFieldGroupGlobal.CustomFields)
                .LastOrDefault(x => x.IsCustomize && x.DataField == property.Name);
            
            
            var match = customField?.Match ?? globalConfiguration?.MatchTemplate ?? ConstantStrings.CustomFieldMatchTemplate;
            match = string.Format(match, property.Name);

            customFields.Add(new CustomField
            {
                DataField = property.Name,
                Match = match,
                GenerateValue = val,
                StaticValue = customField?.StaticValue,
                Template = customField?.Template,
                IsIgnore = customField?.IsIgnore ?? false,
                IsCustomize = customField?.IsCustomize ?? false
            });
        }

        return customFields;
    }
}