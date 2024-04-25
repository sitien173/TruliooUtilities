using System.ComponentModel.DataAnnotations;

namespace TruliooExtension.Model;

public class GlobalConfiguration
{
    [Required] 
    public string CurrentCulture { get; set; } = "en";
    
    [Required]
    [Url]
    public string Endpoint { get; set; } = "https://localhost:44331";
    
    public bool EnableDebugButton { get; set; }
}