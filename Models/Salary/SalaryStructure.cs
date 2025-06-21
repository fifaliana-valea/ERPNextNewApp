using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Models.Salary;

public class SalaryStructure
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
        
    [JsonPropertyName("company")]
    public string CompanyName { get; set; }
    public Company Company { get; set; }
    
    [BindProperty(Name = "Earnings")]
    public List<SalaryComponent> Earnings { get; set; } = new List<SalaryComponent>();
        
    [BindProperty(Name = "Deductions")]
    public List<SalaryComponent> Deductions { get; set; } = new List<SalaryComponent>();
}