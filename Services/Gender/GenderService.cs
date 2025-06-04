using System.Text.Json;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.Gender;

public class GenderService : IGenderService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<GenderService> _logger;

    public GenderService(ILoginService loginService, ILogger<GenderService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public async Task<List<Models.Gender>> GetAllGendersAsync()
    {
        try
        {
            var endpoint = "/api/resource/Gender?fields=[\"name\"]";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json);

            var data = jsonDoc.RootElement.GetProperty("data");
            var genders = JsonSerializer.Deserialize<List<Models.Gender>>(data);

            return genders ?? new List<Models.Gender>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des genres");
            throw;
        }
    }
}
