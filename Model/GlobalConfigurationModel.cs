using System.ComponentModel.DataAnnotations;

namespace TruliooExtension.Model;

public class GlobalConfigurationModel
{
    [Required] 
    public string CurrentCulture { get; set; }
    
    [Required]
    [Url]
    public string NapiEndpoint { get; set; }
    
    [Required]
    public string NapiAuthUserName { get; set; }
    
    [Required]
    public string NapiAuthPassword { get; set; }

    public void Save()
    {
        Program.Culture = CurrentCulture;
        Program.NapiEndpoint = NapiEndpoint;
        Program.NapiAuthUserName = NapiAuthUserName;
        Program.NapiAuthPassword = NapiAuthPassword;
    }
}