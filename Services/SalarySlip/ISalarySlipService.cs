
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.SalarySlip;

public interface ISalarySlipService
{
    Task<List<Models.Salary.SalarySlip>> GetSalarySlipsAsync(string employeeId = null, int mois = 0, int annee = 0);
    Task<Models.Salary.SalarySlip> GetSalarySlipDetailAsync(string slipId);

    Task<byte[]> CreateProfessionalPdf(Models.Salary.SalarySlip salarySlip);
}