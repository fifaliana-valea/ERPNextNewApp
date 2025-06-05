
namespace ERPNextNewApp.Models.Salary;
public class AllSalarySlips
{
    public List<SalaryDisplayRow> Rows { get; set; }
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    public Dictionary<string, decimal> TotalEarnings { get; set; } = new();
    public Dictionary<string, decimal> TotalDeductions { get; set; } = new();

    public decimal TotalNet { get; set; } // 👉 somme des nets
    public decimal TotalBrut { get; set; } // Total des salaires bruts
    public decimal TotalDeduction { get; set; }
    
}
