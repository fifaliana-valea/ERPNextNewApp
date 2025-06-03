using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.Services.SalarySlip;

namespace ERPNextNewApp.Services.Utile;

public class UtileService : IUtileService
{
    private readonly ISalarySlipService _salaryService;

    public UtileService(ISalarySlipService salaryService)
    {
        _salaryService = salaryService;
    }

    public async Task<List<SalaryDisplayRow>> GetSalaryDisplayAsync(int mois = 0, int annee = 0, string employeeId = null)
    {
        var slips = await _salaryService.GetSalarySlipsAllAsync(employeeId, mois, annee);
        var result = new List<SalaryDisplayRow>();

        foreach (var slip in slips)
        {
            var row = new SalaryDisplayRow
            {
                EmployeeName = slip.EmployeeName,
                Net = slip.NetPay,
                DeductionsTotal = slip.Deductions.Sum(d => d.Amount),
                SlipId = slip.Name,
                StartDate = slip.StartDate
            };

            foreach (var earning in slip.Earnings)
            {
                if (!row.Earnings.ContainsKey(earning.SalaryComponentName))
                    row.Earnings[earning.SalaryComponentName] = 0;

                row.Earnings[earning.SalaryComponentName] += earning.Amount;
            }

            result.Add(row);
        }

        return result;
    }
}