namespace TruliooExtension.Entities;

public class CustomFieldGroup
{
    public string Culture { get; set; }
    public bool Enable { get; set; }
    public List<CustomField> CustomFields { get; set; } = [];
}
