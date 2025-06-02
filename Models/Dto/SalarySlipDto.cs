namespace ERPNextNewApp.Models.Dto;

public class SalarySlipDto
{
    public DateTime StartDate { get; set; }
    public string EmployeeId { get; set; }
    public decimal BaseAmount { get; set; }
    public string StructureCode { get; set; }
}
