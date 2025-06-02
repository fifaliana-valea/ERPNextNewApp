namespace ERPNextNewApp.Models.Salary;

public class SalaryComponent
{
    public string SalaryComponentName { get; set; }   // Nom du composant (ex: Basic, Transport)
    public string Abbr { get; set; }                  // Abréviation (ex: BAS)
    public string Type { get; set; }                  // "Earning" ou "Deduction"
    public decimal Amount { get; set; }               // Montant
    public string Description { get; set; }           // Optionnel
}