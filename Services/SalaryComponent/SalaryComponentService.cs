using System.Text.Json;
using ERPNextNewApp.Services.Company;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.SalaryComponent;

public class SalaryComponentService: ISalaryComponentService
{
    public readonly ILoginService _loginService;
    public readonly ILogger<SalaryComponentService> _logger;

    public SalaryComponentService(ILoginService loginService, ILogger<SalaryComponentService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }
    
    public async Task<List<Models.Salary.SalaryComponent>> GetAllSalaryComponentsAsync()
    {
        const string endpoint = "/api/resource/Salary Component?fields=[\"salary_component\",\"salary_component_abbr\",\"type\",\"amount\",\"formula\"]&limit=0";

        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse JSON reçue (Salary Components): {json}", json);

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var salaryComponentsJson))
            {
                _logger.LogWarning("Champ 'data' introuvable dans la réponse JSON");
                return new List<Models.Salary.SalaryComponent>();
            }

            var salaryComponents = JsonSerializer.Deserialize<List<Models.Salary.SalaryComponent>>(
                salaryComponentsJson.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return salaryComponents ?? new List<Models.Salary.SalaryComponent>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération des composants de salaire");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erreur JSON lors de la lecture des composants de salaire");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inconnue lors de la récupération des composants de salaire");
        }

        return new List<Models.Salary.SalaryComponent>();
    }

}