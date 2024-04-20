using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TruliooExtension.Model;

public class UpdateDatasourceEndpoint
{
    [JsonIgnore]
    public const string Key = "update-datasource-endpoint";
    
    public int DatasourceId { get; set; }
    
    [Url]
    public string TestUrl { get; set; } = string.Empty;
    
    [Url]
    public string LiveUrl { get; set; } = string.Empty;
}