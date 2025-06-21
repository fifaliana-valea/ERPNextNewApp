using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models.Salary;

public class SalaryStructureAssignment
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("employee")]
    public string Employee { get; set; }

    [JsonPropertyName("salary_structure")]
    public string SalaryStructure { get; set; }

    [JsonPropertyName("from_date")]
    public DateTime FromDate { get; set; }

    [JsonPropertyName("base")]
    public decimal BaseSalary { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }

    [JsonPropertyName("docstatus")]
    public int Docstatus { get; set; } = 1;

}