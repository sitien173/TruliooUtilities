using System.ComponentModel.DataAnnotations;

namespace TruliooExtension.Model;

public class GlobalConfiguration
{
    [Required] 
    public string CurrentCulture { get; set; } = "en";
    public string MatchTemplate { get; set; } = ConstantStrings.CustomFieldMatchTemplate;
    [Url]
    public string AdminPortalEndpoint { get; set; } = ConstantStrings.AdminPortalEndpoint;
}