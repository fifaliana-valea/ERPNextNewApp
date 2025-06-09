using System.Text;
using System.Text.Json;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.SalaryStructure;

public class SalaryStructureService:ISalaryStructureService
{
    public readonly ILogger<SalaryStructureService> _logger;
    public readonly ILoginService _loginService;

    public SalaryStructureService(ILogger<SalaryStructureService> logger, ILoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
    }
    
    public async Task<bool> UpsertSalaryStructureAsync(Models.Salary.SalaryStructure structData)
    {
        string code = structData.Name;
        string companyName = structData.Company?.Name ?? "";
        string name = $"{code} - {companyName}";
        
        string checkEndpoint = $"/api/resource/Salary Structure/{Uri.EscapeDataString(name)}";
        var checkResponse = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, checkEndpoint);

        if (checkResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Salary Structure already exists: {Name}", name);
            return true;
        }
        
        // Vérifie s’il y a une formule dans les déductions
        bool deductionHasFormula = structData.Deductions.Any(d => !string.IsNullOrWhiteSpace(d.Formula) && d.Formula != "0");
        int deductionDependsFlag = deductionHasFormula ? 0 : 1;

        // Prépare earnings
        var earnings = structData.Earnings.Select(e => new
        {
            salary_component = e.SalaryComponentName,
            amount_based_on_formula = (!string.IsNullOrWhiteSpace(e.Formula) && e.Formula != "0") ? 1 : 0,
            formula = (!string.IsNullOrWhiteSpace(e.Formula) && e.Formula != "0") ? e.Formula : "",
            depends_on_payment_days = (!string.IsNullOrWhiteSpace(e.Formula) && e.Formula != "0") ? 0 : 1
        }).ToList();

        // Prépare deductions
        var deductions = structData.Deductions.Select(d => new
        {
            salary_component = d.SalaryComponentName,
            amount_based_on_formula = (!string.IsNullOrWhiteSpace(d.Formula) && d.Formula != "0") ? 1 : 0,
            formula = (!string.IsNullOrWhiteSpace(d.Formula) && d.Formula != "0") ? d.Formula : "",
            depends_on_payment_days = deductionDependsFlag
        }).ToList();

        var payload = new
        {
            name = name,
            company = companyName,
            currency = structData.Company?.Currency ?? "USD",
            is_active = "Yes",
            payroll_frequency = "Monthly",
            earnings = earnings,
            deductions = deductions,
            docstatus = 1
        };

        string endpoint = "/api/resource/Salary Structure";
        string json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Post, endpoint, content);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Salary Structure insérée avec succès : {Name}", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de Salary Structure : {Name}", name);
            return false;
        }
    }
    
}