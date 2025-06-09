using ERPNextNewApp.Models;

namespace ERPNextNewApp.ViewModels.Employees;

public class EmployeeViewModels
{
    public List<Department> Departments {get; set;}
    public List<Gender> Genders {get; set;}
    public List<string> Designations {get; set;}
    
    public Employee Employee { get; set; }
    
    public List<string> CompanyNames {get; set;}
}