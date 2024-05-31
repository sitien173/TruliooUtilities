using System.ComponentModel.DataAnnotations;

namespace TruliooExtension.Model;

public class CSP
{
    public int Id { get; set; }
    [Url]
    public string Url { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
}