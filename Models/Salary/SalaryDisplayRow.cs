using System.Globalization;

namespace ERPNextNewApp.Models.Salary;

public class SalaryDisplayRow
{
    public string EmployeeName { get; set; }

    public Dictionary<string, decimal> Earnings { get; set; } = new();

    public Dictionary<string, decimal> DeductionsTotal { get; set; } = new();

    public decimal Net { get; set; }
    
    public string SlipId { get; set; }

    public string StartDate { get; set; }

    public string MonthLabel
    {
        get
        {
            if (DateTime.TryParse(StartDate, out var date))
            {
                return date.ToString("MMMM yyyy", new CultureInfo("fr-FR"));
            }
            return "Date invalide";
        }
    }

}
