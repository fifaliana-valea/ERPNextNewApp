namespace ERPNextNewApp.Models.Salary;

public class StatistiqueTotal
{
    public List<StatistiqueSalary> Salaries { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalBut {get; set;}
    public decimal TotalDeduction {get; set;}
        
}