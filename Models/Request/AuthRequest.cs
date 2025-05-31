using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models.Request;

public class AuthRequest
{
    [JsonPropertyName("usr")]
    public string Usr { get; set; }

    [JsonPropertyName("pwd")]
    public string Pwd { get; set; }
}