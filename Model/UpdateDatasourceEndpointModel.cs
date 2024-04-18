using System.ComponentModel.DataAnnotations;

namespace trulioo_autofill.Model;

public class UpdateDatasourceEndpointModel
{
    public int DatasourceId { get; set; }
    
    [Url]
    public string TestUrl { get; set; } = string.Empty;
    
    [Url]
    public string LiveUrl { get; set; } = string.Empty;
}