using System.ComponentModel.DataAnnotations;
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.ViewModels.SalarySlips;

public class ModifViews
{
    public string? Component { get; set; }

    public int Condition { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être supérieur ou égal à 0")]
    public decimal Salary { get; set; }

    public int Pourcentage { get; set; }
    
    public int Action { get; set; }

    public List<SalaryComponent>? SalaireSalaire { get; set; }

    public List<SalarySlip>? SalarySlips { get; set; }
}