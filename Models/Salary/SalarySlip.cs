namespace ERPNextNewApp.Models.Salary;

public class SalarySlip
{
    public string Name { get; set; }
    public string Employee { get; set; }
    public string EmployeeName { get; set; }
    public string Company { get; set; }
    public string PostingDate { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public decimal NetPay { get; set; }
    public decimal GrossPay { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    
    public List<SalaryComponent> Earnings { get; set; }
    public List<SalaryComponent> Deductions { get; set; }
}
