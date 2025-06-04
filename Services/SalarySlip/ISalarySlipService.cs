
using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.ViewModels.SalarySlips;

namespace ERPNextNewApp.Services.SalarySlip;

public interface ISalarySlipService
{
    Task<PaginatedSalarySlips> GetSalarySlipsAllAsync(
        int page,
        int pageSize,
        string employeeId = null,
        int mois = 0,
        int annee = 0);
    Task<Models.Salary.SalarySlip> GetSalarySlipDetailAsync(string slipId);

    Task<AllSalarySlips> GetSalaryDisplayAsync(int page, int pageSize, int mois = 0, int annee = 0,
        string employeeId = null);

    Task<byte[]> CreateProfessionalPdf(Models.Salary.SalarySlip salarySlip);
}