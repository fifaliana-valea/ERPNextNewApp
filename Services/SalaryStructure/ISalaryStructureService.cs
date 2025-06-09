namespace ERPNextNewApp.Services.SalaryStructure;

public interface ISalaryStructureService
{
    Task<bool> UpsertSalaryStructureAsync(Models.Salary.SalaryStructure structData);
}