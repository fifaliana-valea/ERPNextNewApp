using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.Utile;

public interface IUtileService
{
    Task<string> ResetSelectedPayrollDataAsync();
    Task<List<string>> GetDocumentNamesByDoctypeAsync(string doctype);

    List<DateTime> GetListeDate(DateTime? dateDebut, DateTime? dateFin);
    List<DateTime> GetDate(DateTime date);
}