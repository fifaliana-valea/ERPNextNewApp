using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models;

public class Company
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("default_currency")]
    public string Currency { get; set; } = "USD";
}