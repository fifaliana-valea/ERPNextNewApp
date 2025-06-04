namespace ERPNextNewApp.Models.Salary;

public class PaginatedSalarySlips
{
    public List<Models.Salary.SalarySlip> Slips { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
