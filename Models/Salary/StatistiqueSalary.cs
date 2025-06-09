using System.Globalization;

namespace ERPNextNewApp.Models.Salary;

public class StatistiqueSalary
{
    public string MonthLabel { get; set; }
    public Dictionary<string, decimal> TotalMonthEarnings { get; set; } = new();
    public Dictionary<string, decimal> TotalMonthDeductions { get; set; } = new();
    
    public decimal TotalNet { get; set; } = new();
}