namespace TruliooExtension.Common;

public class AppSettings
{
    public string DbName { get; set; }
    public string AssemblyName { get; set; }
    public string DefaultCulture { get; set; }
    public string CustomFieldMatchTemplate { get; set; }
    public Uri AdminPortalEndpoint { get; set; }
    public string CustomFieldGroupGlobalKey { get; set; }
    public string LastFetchDatasourceId { get; set; }
    public string LastUpdatedDatasourceId { get; set; }
    public string UpdateDatasourcePath { get; set; }
    public string GetDatasourcePath { get; set; }
    public Tables Tables { get; set; }
}

public class Tables
{
    public string CspManager { get; set; }
    public string CustomFieldGroup { get; set; }
    public string Temp { get; set; }
}