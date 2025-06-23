using System.ComponentModel.DataAnnotations;
using ERPNextNewApp.Models;

namespace ERPNextNewApp.ViewModels.SalaryStructureAssignment;

public class InsertSalaryStructureAssignment
{
    public string EmployeeId { get; set; }
    public DateTime? DateDebut { get; set; }
    
    [DateFinAfterDateDebut("DateDebut")]
    public DateTime? DateFin { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être positif ou zéro.")]
    public decimal Salary { get; set; } = 0.0m;
    public List<Employee> Employees { get; set; } = new();
}