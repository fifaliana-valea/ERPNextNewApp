namespace ERPNextNewApp.Models.Dto;

public class ImportPayload
{
    public List<EmployeeDto> Employees { get; set; }
    public List<SalaryStructureDto> SalaryStructures { get; set; }
    public List<SalarySlipDto> SalarySlips { get; set; }
}
