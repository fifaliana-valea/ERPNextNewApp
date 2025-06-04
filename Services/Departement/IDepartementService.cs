using ERPNextNewApp.Models;

namespace ERPNextNewApp.Services.Departement;


public interface IDepartementService
{
    Task<List<Department>> GetAllDepartmentsAsync();
}