using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TruliooExtension.Model;

public class GlobalConfiguration
{
    [JsonIgnore]
    public const string Key = "global-config";

    [Required] 
    public string CurrentCulture { get; set; } = "en";
    
    [Required]
    [Url]
    public string Endpoint { get; set; } = "https://localhost:44331";
    
    public bool EnableDebugButton { get; set; }
}