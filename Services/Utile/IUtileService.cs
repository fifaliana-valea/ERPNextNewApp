using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.Utile;

public interface IUtileService
{
    Task<string> ResetSelectedPayrollDataAsync();
    Task<List<string>> GetDocumentNamesByDoctypeAsync(string doctype);
}