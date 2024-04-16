namespace trulioo_autofill.Models;

public class CustomField
{
    public int Id { get; set; }
    public string FieldName { get; set; }
    public DataTypeEnum DataType { get; set; }
    public string[] Match { get; set; }
    public string Template { get; set; }
    public string MinValue { get; set; }
    public string MaxValue { get; set; }
}