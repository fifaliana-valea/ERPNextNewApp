using System.Text.Json;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.Company;

public class CompanyService: ICompanyService
{
    public readonly ILoginService _loginService;
    public readonly ILogger<CompanyService> _logger;

    public CompanyService(ILoginService loginService, ILogger<CompanyService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }
        
    public async Task<List<Models.Company>> GetAllCompanysAsync()
    {
        const string endpoint = "/api/resource/Company?fields=[\"name\",\"default_currency\"]&limit=0";

        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse JSON reçue (Salary Components): {json}", json);

            using var doc = JsonDocument.Parse(json);
            var salaryComponentsJson = doc.RootElement.GetProperty("data");

            var companies = JsonSerializer.Deserialize<List<Models.Company>>(
                salaryComponentsJson.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return companies ?? new List<Models.Company>();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des composants de salaire");
            return new List<Models.Company>();
        }
    }
    
    public async Task<Models.Company> GetByIdCompanieAsync(string companyId)
    {
        string endpoint = $"/api/resource/Company/{companyId}?fields=[\"name\",\"default_currency\"]";

        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse JSON reçue (Salary Components): {json}", json);

            using var doc = JsonDocument.Parse(json);
            var salaryComponentsJson = doc.RootElement.GetProperty("data");

            var companies = JsonSerializer.Deserialize<Models.Company>(
                salaryComponentsJson.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return companies ?? new Models.Company();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des composants de salaire");
            return new Models.Company();
        }
    }
}