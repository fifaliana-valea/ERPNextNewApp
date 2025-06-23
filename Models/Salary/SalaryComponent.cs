using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models.Salary;

public class SalaryComponent
{
    [JsonPropertyName("salary_component")]
    public string SalaryComponentName { get; set; }   // Nom du composant (ex: Basic, Transport)

    [JsonPropertyName("salary_component_abbr")]
    public string Abbr { get; set; }                  // Abréviation (ex: BAS)

    [JsonPropertyName("type")]
    public string Type { get; set; }                  // "Earning" ou "Deduction"

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }               // Montant

    [JsonPropertyName("formula")]
    public string? Formula { get; set; }               // Optionnel
}