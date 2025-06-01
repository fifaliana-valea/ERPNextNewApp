using ERPNextNewApp.Models;

namespace ERPNextNewApp.ViewModels.Employees;

public class EmployeesListViewModel
{
    public List<Employee> Employees { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public EmployeeFilters Filters { get; set; }
}