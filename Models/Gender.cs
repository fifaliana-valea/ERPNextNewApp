using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ERPNextNewApp.Models;

public class Gender
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}