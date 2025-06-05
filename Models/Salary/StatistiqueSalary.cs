using System.Globalization;

namespace ERPNextNewApp.Models.Salary;

public class StatistiqueSalary
{
    public string MonthLabel { get; set; }
    public Dictionary<string, decimal> TotalEarnings { get; set; } = new();
    public Dictionary<string, decimal> TotalDeductions { get; set; } = new();
    
    public decimal TotalNet { get; set; } = new();
}