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