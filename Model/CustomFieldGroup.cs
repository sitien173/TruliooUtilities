using System.Text.Json.Serialization;

namespace TruliooExtension.Model;

public class CustomFieldGroup
{
    [JsonIgnore]
    public const string Key = "custom-field-groups";
    public string Culture { get; set; }
    public List<CustomField> CustomFields { get; set; } = [];
}
