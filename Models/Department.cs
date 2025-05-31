using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ERPNextNewApp.Models;

public class Department
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("department_name")]
    public string DepartmentName { get; set; }

    [JsonPropertyName("parent_department")]
    public string ParentDepartment { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }

    [JsonPropertyName("is_group")]
    public int IsGroup { get; set; }

}