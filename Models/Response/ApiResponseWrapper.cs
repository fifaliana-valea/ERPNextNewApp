using Newtonsoft.Json;

namespace ERPNextNewApp.Models.Response;

public class ApiResponseWrapper<T>
{
    [JsonProperty("data")]
    public T Data { get; set; }
}
