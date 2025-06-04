using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.Utile;

public interface IUtileService
{
    Task<string> ResetSelectedPayrollDataAsync();
}