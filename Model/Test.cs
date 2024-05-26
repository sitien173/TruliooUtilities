using Newtonsoft.Json;

public partial class Example
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("age")]
    public long Age { get; set; }

    [JsonProperty("car")]
    public object Car { get; set; }

    public void Something()
    {
        
    }
}