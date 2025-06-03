using System.Text;
using System.Text.Json;
using System.Web;
using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ILoginService loginService, ILogger<EmployeeService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }

    public async Task<(List<Employee> Employees, int TotalCount)> GetEmployeesAsync(
        string? name = null,
        string? department = null,
        string? status = null,
        string? gender = null,
        DateTime? dateStart = null,
        DateTime? dateEnd = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var filters = new List<object[]>();

            if (!string.IsNullOrWhiteSpace(name))
                filters.Add(new object[] { "employee_name", "like", $"%{name}%" });

            if (!string.IsNullOrWhiteSpace(department))
                filters.Add(new object[] { "department", "=", department });

            if (!string.IsNullOrWhiteSpace(status))
                filters.Add(new object[] { "status", "=", status });

            if (!string.IsNullOrWhiteSpace(gender))
                filters.Add(new object[] { "gender", "=", gender });

            if (dateStart.HasValue)
                filters.Add(new object[] { "date_of_joining", ">=", dateStart.Value.ToString("yyyy-MM-dd") });

            if (dateEnd.HasValue)
                filters.Add(new object[] { "date_of_joining", "<=", dateEnd.Value.ToString("yyyy-MM-dd") });

            string filtersJson = JsonSerializer.Serialize(filters);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fields"] = "[\"name\",\"employee_name\",\"designation\",\"department\",\"date_of_joining\",\"status\",\"gender\",\"company_email\",\"image\"]";
            query["limit_start"] = ((page - 1) * pageSize).ToString();
            query["limit_page_length"] = pageSize.ToString();
            query["filters"] = filtersJson;

            var endpoint = $"/api/resource/Employee?{query}";

            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            var employees = JsonSerializer.Deserialize<List<Employee>>(data) ?? new();

            // get total count separately
            var countEndpoint = $"/api/resource/Employee?fields=[\"name\"]&filters={filtersJson}";
            var countResponse = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, countEndpoint);
            var countJson = await countResponse.Content.ReadAsStringAsync();
            var countDoc = JsonDocument.Parse(countJson);
            var countData = countDoc.RootElement.GetProperty("data");
            int totalCount = countData.GetArrayLength();

            return (employees, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des employés avec filtre");
            throw;
        }
    }

    public async Task<List<Employee>> GetEmployeesAllAsync()
    {
        const string endpoint = "/api/resource/Employee?fields=[\"name\",\"employee_name\",\"designation\",\"department\",\"date_of_joining\",\"status\",\"gender\",\"company_email\",\"image\"]";

        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            
            response.EnsureSuccessStatusCode(); // Lance une exception si le status code indique une erreur

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse JSON reçue: {json}", json); // Log utile pour le débogage

            using var doc = JsonDocument.Parse(json);
            var employeeJson = doc.RootElement.GetProperty("data");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Important pour la correspondance
            };

            var employees = JsonSerializer.Deserialize<List<Employee>>(employeeJson.GetRawText(), options);

            if (employees == null)
            {
                _logger.LogWarning("Aucun employé trouvé ou désérialisation a échoué");
                return new List<Employee>();
            }

            // Vérification des données désérialisées
            foreach (var employee in employees)
            {
                _logger.LogInformation("Employé récupéré - Nom: {FullName}, ID: {Name}, Société: {Company}", 
                    employee.FullName, employee.Id, employee.Company);
            }

            return employees;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erreur HTTP lors de la récupération des employés");
            return new List<Employee>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Erreur de désérialisation des données employés");
            return new List<Employee>();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erreur inattendue dans GetEmployeesAllAsync");
            return new List<Employee>();
        }
    }
    public async Task<bool> DeleteEmployeesAsync(List<string> employeeIds)
    {
        try
        {
            if (employeeIds == null || employeeIds.Count == 0)
            {
                _logger.LogWarning("Aucun ID d'employé fourni pour la suppression.");
                return false;
            }

            // Création des commandes batch
            var commands = employeeIds.Select(id => new
            {
                method = "DELETE",
                path = $"/api/resource/Employee/{HttpUtility.UrlEncode(id)}"
            }).ToList();

            // Préparation du payload pour la requête batch
            var payload = new
            {
                cmd = "frappe.client.batch",
                commands
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Envoi de la requête POST batch
            var response = await _loginService.MakeAuthenticatedRequest(
                HttpMethod.Post,
                "/api/method/frappe.client.batch",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erreur HTTP lors de la suppression des employés : {StatusCode} - {Body}", response.StatusCode, responseBody);
                return false;
            }

            var jsonDoc = JsonDocument.Parse(responseBody);

            if (jsonDoc.RootElement.TryGetProperty("message", out var message))
            {
                foreach (var item in message.EnumerateArray())
                {
                    if (item.TryGetProperty("error", out var error))
                    {
                        _logger.LogError("Erreur lors de la suppression d’un employé : {Error}", error.ToString());
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception lors de la suppression des employés.");
            return false;
        }
    }
    
    public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
    {
        if (string.IsNullOrEmpty(employeeId))
            return null;

        try
        {
            var endpoint = $"/api/resource/Employee/{employeeId}?fields=[\"name\",\"employee_name\",\"designation\",\"department\",\"date_of_joining\",\"status\",\"gender\",\"company_email\",\"image\"]";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Échec lors de la récupération de l'employé {EmployeeId} - Statut: {Status}", employeeId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var employeeJson = doc.RootElement.GetProperty("data");

            var employee = JsonSerializer.Deserialize<Employee>(employeeJson.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à GetEmployeeByIdAsync pour {EmployeeId}", employeeId);
            return null;
        }
    }

}
