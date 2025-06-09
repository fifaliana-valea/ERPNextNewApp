namespace ERPNextNewApp.Services.Company;

public interface ICompanyService
{
    Task<List<Models.Company>> GetAllSalaryComponentsAsync();
}