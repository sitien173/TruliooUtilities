using System.ComponentModel.DataAnnotations;
using TruliooExtension.Common;
using ConfigurationProvider = TruliooExtension.Services.ConfigurationProvider;

namespace TruliooExtension.Entities;

public class GlobalConfiguration
{
    [Required] 
    public string CurrentCulture { get; set; } = "en";

    public string MatchTemplate { get; set; }
    [Url]
    public string AdminPortalEndpoint { get; set; }
    public bool RefreshOnFill { get; set; }
}