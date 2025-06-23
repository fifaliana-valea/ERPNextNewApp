using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Web;
using ERPNextNewApp.Services.Company;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.SalaryStructure;

public class SalaryStructureService:ISalaryStructureService
{
    public readonly ILogger<SalaryStructureService> _logger;
    public readonly ILoginService _loginService;
    public readonly ICompanyService _companyService;

    public SalaryStructureService(ILogger<SalaryStructureService> logger, ILoginService loginService, ICompanyService companyService)
    {
        _logger = logger;
        _loginService = loginService;
        _companyService = companyService;
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
    
    
    public async Task<Models.Salary.SalaryStructure> GetSalaryStructureCompanyAsync(string company = null)
    {
        try
        {
            var filters = new List<object[]>();

            if (!string.IsNullOrWhiteSpace(company))
                filters.Add(new object[] { "company", "=", company });

            filters.Add(new object[] { "docstatus", "=", 1 });

            var filtersJson = JsonSerializer.Serialize(filters);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fields"] = JsonSerializer.Serialize(new[]
            {
                "name", "company"
            });
            query["filters"] = filtersJson;

            var endpoint = $"/api/resource/Salary Structure?{query}";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            var dataElement = jsonDoc.RootElement.GetProperty("data");

            if (dataElement.ValueKind == JsonValueKind.Array && dataElement.GetArrayLength() > 0)
            {
                var firstStructure = dataElement[0];
                var salaryStructure = JsonSerializer.Deserialize<Models.Salary.SalaryStructure>(firstStructure.GetRawText());
                return salaryStructure;
            }

            return null; // Aucun résultat
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des salary structure avec filtre");
            throw;
        }
    }
    
    public async Task<Models.Salary.SalaryStructure> GetSalaryStructureDetailAsync(string salaryStructureId = null)
    {
        if (string.IsNullOrWhiteSpace(salaryStructureId))
            throw new ArgumentException("L'identifiant du Salary Slip est requis.");

        string endpoint = $"/api/resource/Salary Structure/{salaryStructureId}?" +
                          "fields=[\"name\",\"company\",\"earnings\",\"deductions\"]";

        var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur lors de la récupération du détail du Salary structure  {SalaryStructureId}: {StatusCode}", salaryStructureId, response.StatusCode);
            throw new Exception("Erreur API: " + response.ReasonPhrase);
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var salarySlip = new Models.Salary.SalaryStructure()
        {
            Name = data.GetProperty("name").GetString(),
            CompanyName = data.GetProperty("company").GetString(),
            Company = await _companyService.GetByIdCompanieAsync(data.GetProperty("company").GetString()),
            Earnings = JsonSerializer.Deserialize<List<Models.Salary.SalaryComponent>>(data.GetProperty("earnings").ToString()),
            Deductions = JsonSerializer.Deserialize<List<Models.Salary.SalaryComponent>>(data.GetProperty("deductions").ToString())
        };

        return salarySlip;
    } 

    
}