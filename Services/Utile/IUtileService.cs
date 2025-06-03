using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.Utile;

public interface IUtileService
{
    Task<List<SalaryDisplayRow>> GetSalaryDisplayAsync(int mois = 0, int annee = 0, string employeeId = null);

}