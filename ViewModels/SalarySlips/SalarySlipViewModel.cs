using ERPNextNewApp.Models;
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.ViewModels.SalarySlips;

public class SalarySlipViewModel
{
    public List<SalaryDisplayRow> SalaryRows { get; set; }
    public List<Employee> Employees { get; set; }
    public int SelectedMonth { get; set; }
    public int SelectedYear { get; set; }
    public string SelectedEmployeeId { get; set; }
}