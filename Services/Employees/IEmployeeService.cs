using ERPNextNewApp.Models;

namespace ERPNextNewApp.Services.Employees;

public interface IEmployeeService
{
    Task<(List<Employee> Employees, int TotalCount)> GetEmployeesAsync(
        string? name = null,
        string? department = null,
        string? status = null,
        string? gender = null,
        DateTime? dateStart = null,
        DateTime? dateEnd = null,
        int page = 1,
        int pageSize = 10);

    Task<bool> DeleteEmployeesAsync(List<string> employeeNames);
}