using ERPNextNewApp.Models;
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.ViewModels.SalarySlips;

public class SalarySlipIndexViewModel
{
    public Employee Employee { get; set; }
    public IEnumerable<SalarySlip> SalarySlips { get; set; }
    public string SelectedEmployeeId { get; set; }
    public int SelectedMonth { get; set; }
    public int SelectedYear { get; set; }
    
    public int Page {get; set;}
    
    public int TotalPage {get; set;}
    
    public int TotalItems {get; set;}
    
    public int PageSize {get; set;}
    public Dictionary<int, string> Months { get; set; }
    public Dictionary<int, string> Years { get; set; }
}