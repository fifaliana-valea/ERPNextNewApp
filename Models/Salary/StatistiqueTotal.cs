namespace ERPNextNewApp.Models.Salary;

public class StatistiqueTotal
{
    public List<StatistiqueSalary> Salaries { get; set; }
    
    public Dictionary<string, decimal> TotalGlobalEarnings { get; set; } = new();
    public Dictionary<string, decimal> TotalGlobalDeductions { get; set; } = new();
    public decimal TotalNet { get; set; }
    public decimal TotalBut {get; set;}
    public decimal TotalDeduction {get; set;}
        
}