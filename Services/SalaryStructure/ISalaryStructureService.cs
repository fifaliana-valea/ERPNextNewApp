namespace ERPNextNewApp.Services.SalaryStructure;

public interface ISalaryStructureService
{
    Task<bool> UpsertSalaryStructureAsync(Models.Salary.SalaryStructure structData);

    Task<Models.Salary.SalaryStructure> GetSalaryStructureCompanyAsync(string company = null);

    Task<Models.Salary.SalaryStructure> GetSalaryStructureDetailAsync(string salaryStructureId = null);
}