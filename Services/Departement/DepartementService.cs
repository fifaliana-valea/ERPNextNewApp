using System.Text.Json;
using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Login;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ERPNextNewApp.Services.Departement;

public class DepartementService : IDepartementService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<DepartementService> _logger;

    public DepartementService(ILoginService loginService, ILogger<DepartementService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            var endpoint = "/api/resource/Department?fields=[\"name\",\"department_name\",\"parent_department\",\"company\",\"is_group\"]";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json);

            var data = jsonDoc.RootElement.GetProperty("data");
            var departments = JsonSerializer.Deserialize<List<Department>>(data);


            _logger.LogInformation("Get all departments {departments.Count}", departments.Count);
            return departments ?? new List<Department>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des départements");
            throw;
        }
    }
}
