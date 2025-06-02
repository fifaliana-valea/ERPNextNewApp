using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models.Salary;

public class SalarySlip
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("employee")]
    public string Employee { get; set; }

    [JsonPropertyName("employee_name")]
    public string EmployeeName { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }

    [JsonPropertyName("posting_date")]
    public string PostingDate { get; set; }

    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public string EndDate { get; set; }

    [JsonPropertyName("net_pay")]
    public decimal NetPay { get; set; }

    [JsonPropertyName("gross_pay")]
    public decimal GrossPay { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    public List<SalaryComponent> Earnings { get; set; }
    public List<SalaryComponent> Deductions { get; set; }
}
