namespace ERPNextNewApp.Services.SalaryComponent;

public interface ISalaryComponentService
{
    Task<List<Models.Salary.SalaryComponent>> GetAllSalaryComponentsAsync();
}