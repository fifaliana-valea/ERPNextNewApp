using ERPNextNewApp.Models.Dto;

namespace ERPNextNewApp.Services.Import;

public interface IImportService
{
    Task<(List<EmployeeDto> data, List<string> errors)> LoadAndValidateEmployees(string filePath);
    Task<(List<SalaryStructureDto> data, List<string> errors)> LoadAndValidateSalaryStructures(string filePath);

   Task<(List<SalarySlipDto> data, List<string> errors)> LoadAndValidateSalarySlips(string filePath);

   Task<List<string>> ImportAllAsync(string employeeFile, string structureFile, string salaryFile);
}