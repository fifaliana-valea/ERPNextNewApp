namespace ERPNextNewApp.Services.Company;

public interface ICompanyService
{
    Task<List<Models.Company>> GetAllCompanysAsync();

    Task<Models.Company> GetByIdCompanieAsync(string companyId);
}