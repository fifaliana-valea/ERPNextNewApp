using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.Services.Login;
using ERPNextNewApp.Services.SalarySlip;
using Newtonsoft.Json;

namespace ERPNextNewApp.Services.Utile;

public class UtileService : IUtileService
{
    private readonly ISalarySlipService _salaryService;
    private readonly ILoginService _loginService;

    public UtileService(ISalarySlipService salaryService,ILoginService loginService)
    {
        _salaryService = salaryService;
        _loginService = loginService;
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
    
    public async Task<string> ResetSelectedPayrollDataAsync()
    {
        try
        {
            var url = "/api/method/custom_reset.api.reset_data.reset_data";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Post, url, null);

            if (!response.IsSuccessStatusCode)
                return "Erreur de connexion à l'API.";

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(responseBody);

            if (json.status == "success")
            {
                var details = string.Join("<br>", json.details);
                return $"{json.message}<br><br>Détails:<br>{details}";
            }
            return $"Erreur: {json.message}";
        }
        catch (Exception ex)
        {
            return $"Erreur critique: {ex.Message}";
        }
    }

}