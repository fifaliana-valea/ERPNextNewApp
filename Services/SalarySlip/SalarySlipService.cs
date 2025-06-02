using System.Text.Json;
using ERPNextNewApp.Services.Login;
using ERPNextNewApp.Models;
using System.Net;
using ERPNextNewApp.Models.Salary;

namespace ERPNextNewApp.Services.SalarySlip;

public class SalarySlipService : ISalarySlipService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<SalarySlipService> _logger;

    public SalarySlipService(ILoginService loginService, ILogger<SalarySlipService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public async Task<List<Models.Salary.SalarySlip>> GetSalarySlipsAsync(string employeeId = null, int mois = 0, int annee = 0)
    {
        var filtersArray = new List<string[]>();

        if (!string.IsNullOrEmpty(employeeId))
        {
            filtersArray.Add(new[] { "employee", "=", employeeId });
        }

        if (mois > 0)
        {
            filtersArray.Add(new[] { "month(start_date)", "=", mois.ToString() });
        }

        if (annee > 0)
        {
            filtersArray.Add(new[] { "year(start_date)", "=", annee.ToString() });
        }

        // Construction de l’URL avec ou sans filtres
        string baseUrl = "/api/resource/Salary Slip?";
        string fieldsPart = "fields=[\"name\",\"employee\",\"employee_name\",\"company\",\"posting_date\",\"start_date\",\"end_date\",\"net_pay\",\"gross_pay\",\"currency\",\"status\"]";

        string filtersPart = filtersArray.Count > 0
            ? $"&filters={JsonSerializer.Serialize(filtersArray)}"
            : "";

        string endpoint = baseUrl + fieldsPart + filtersPart;

        Console.WriteLine("Endpoint appelé : " + endpoint);

        var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur lors de la récupération des fiches de paie: {StatusCode}", response.StatusCode);
            throw new Exception("Erreur API: " + response.ReasonPhrase);
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var result = new List<Models.Salary.SalarySlip>();
        foreach (var item in data.EnumerateArray())
        {
            var slip = JsonSerializer.Deserialize<Models.Salary.SalarySlip>(item.ToString());
            result.Add(slip);
        }

        return result;
    }
    
    public async Task<Models.Salary.SalarySlip> GetSalarySlipDetailAsync(string slipId)
    {
        if (string.IsNullOrWhiteSpace(slipId))
            throw new ArgumentException("L'identifiant du Salary Slip est requis.");

        string endpoint = $"/api/resource/Salary Slip/{slipId}?" +
                          "fields=[\"name\",\"employee\",\"employee_name\",\"company\",\"posting_date\",\"start_date\",\"end_date\"," +
                          "\"net_pay\",\"gross_pay\",\"currency\",\"status\",\"earnings\",\"deductions\"]";

        var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur lors de la récupération du détail du Salary Slip {SlipId}: {StatusCode}", slipId, response.StatusCode);
            throw new Exception("Erreur API: " + response.ReasonPhrase);
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var salarySlip = new Models.Salary.SalarySlip
        {
            Name = data.GetProperty("name").GetString(),
            Employee = data.GetProperty("employee").GetString(),
            EmployeeName = data.GetProperty("employee_name").GetString(),
            Company = data.GetProperty("company").GetString(),
            PostingDate = data.GetProperty("posting_date").GetString(),
            StartDate = data.GetProperty("start_date").GetString(),
            EndDate = data.GetProperty("end_date").GetString(),
            NetPay = data.GetProperty("net_pay").GetDecimal(),
            GrossPay = data.GetProperty("gross_pay").GetDecimal(),
            Currency = data.GetProperty("currency").GetString(),
            Status = data.GetProperty("status").GetString(),
            Earnings = JsonSerializer.Deserialize<List<SalaryComponent>>(data.GetProperty("earnings").ToString()),
            Deductions = JsonSerializer.Deserialize<List<SalaryComponent>>(data.GetProperty("deductions").ToString())
        };

        return salarySlip;
    }


}