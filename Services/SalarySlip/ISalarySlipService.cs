
using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.ViewModels.SalarySlips;

namespace ERPNextNewApp.Services.SalarySlip;

public interface ISalarySlipService
{
    Task<Models.Salary.SalarySlip> GetSalarySlipDetailAsync(string slipId);

    Task<AllSalarySlips> GetSalaryDisplayAsync(int page, int pageSize, int mois = 0, int annee = 0,
        string employeeId = null);

    Task<byte[]> CreateProfessionalPdf(Models.Salary.SalarySlip salarySlip);

    Task<List<Models.Salary.SalarySlip>> GetSalarySlipsMonthYearsAsync(string employeeId = null, int mois = 0,
        int annee = 0);

    Task<List<Models.Salary.SalarySlip>> GetSalarySlipsStatistiqueAsync(int annee = 0);

    Task<StatistiqueTotal> GetSalaryStatistiqueAsync(int annee = 0);

    Task<bool> DeleteSalarySlipsAsync(string slpis);

    Task<bool> ModificationSlipsAsync(Models.Salary.SalarySlip slips);

    Task<bool> InsertSalarySlipsAsync(Models.Salary.SalarySlip salarySlip);

    Task<List<Models.Salary.SalarySlip>> GetSalarySlipsWithConditionAsync(decimal salary, int condition,
        string componentName);

}