using System.Text.Json.Serialization;

namespace TruliooExtension.Model;

public class SelectOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}