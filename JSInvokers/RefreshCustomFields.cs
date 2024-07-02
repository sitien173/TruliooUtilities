using Microsoft.JSInterop;
using TruliooExtension.Common;
using TruliooExtension.Entities;

namespace TruliooExtension.JSInvokers;

public static class RefreshCustomFields
{
    [JSInvokable("RefreshCustomFields")]
    public static List<CustomField> GetCustomFields(CustomFieldGroup customFieldGroup, CustomFieldGroup customFieldGroupGlobal, GlobalConfiguration globalConfiguration, bool merge = true)
    {
        var customFields = new List<CustomField>();

        var fieldFaker = FieldFaker.GenerateWithCustomFieldGroup(customFieldGroupGlobal, customFieldGroup);
        var properties = fieldFaker.GetType().GetProperties();
        foreach (var property in properties)
        {
            var val = property.GetValue(fieldFaker)?.ToString();
            if (string.IsNullOrEmpty(val))
                continue;

            CustomField? customField;
            if (merge)
            {
                customField = customFieldGroup.CustomFields
                    .Concat(customFieldGroupGlobal.CustomFields)
                    .LastOrDefault(x => x.IsCustomize && x.DataField == property.Name);
            }
            else
            {
                customField = customFieldGroup.CustomFields.LastOrDefault(x => x.IsCustomize && x.DataField == property.Name);
            }

            var match = customField?.Match ?? globalConfiguration.MatchTemplate;
            match = string.Format(match, property.Name);

            customFields.Add(new CustomField
            {
                DataField = property.Name,
                Match = match,
                GenerateValue = val,
                StaticValue = customField?.StaticValue,
                Template = customField?.Template,
                IsIgnore = customField?.IsIgnore ?? false,
                Domain = customField?.Domain,
                IsCustomize = customField?.IsCustomize ?? false
            });
        }

        return customFields;
    }
}