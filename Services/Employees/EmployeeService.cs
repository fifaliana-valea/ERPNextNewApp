using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Login;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            
            var filtersJson = JsonSerializer.Serialize(filters);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fields"] = JsonSerializer.Serialize(new[]
            {
                "name", "employee_name", "designation","date_of_birth", "department", "date_of_joining",
                "status", "gender", "company_email", "image"
            });
            query["limit_start"] = ((page - 1) * pageSize).ToString();
            query["limit_page_length"] = pageSize.ToString();
            query["filters"] = filtersJson;

            var endpoint = $"/api/resource/Employee?{query}";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var employees = JsonSerializer.Deserialize<List<Employee>>(jsonDoc.RootElement.GetProperty("data")) ?? new();

            // Récupération du total avec les mêmes filtres
            var countQuery = HttpUtility.ParseQueryString(string.Empty);
            countQuery["filters"] = filtersJson;
            countQuery["limit_page_length"] = "0"; // Important: désactive la limite
            
            var countEndpoint = $"/api/resource/Employee?{countQuery}";
            var countResponse = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, countEndpoint);
            countResponse.EnsureSuccessStatusCode();

            using var countDoc = JsonDocument.Parse(await countResponse.Content.ReadAsStringAsync());
            var totalCount = countDoc.RootElement.GetProperty("data").GetArrayLength();

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
        const string endpoint = "/api/resource/Employee?fields=[\"name\", \"employee_name\", \"designation\", \"date_of_birth\", \"department\", \"date_of_joining\", \"status\", \"gender\", \"company_email\", \"image\", \"company\"]&limit=0";
        try
        {
            using var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse JSON reçue: {json}", json);

            using var doc = JsonDocument.Parse(json);
            var employeeJson = doc.RootElement.GetProperty("data");

            var employees = JsonSerializer.Deserialize<List<Employee>>(
                employeeJson.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return employees ?? new List<Employee>();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des employés");
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
    
    public async Task<bool> InsertEmployeeAsync(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        try
        {
            // Construire le corps JSON à envoyer (seuls les champs pertinents sont inclus)
            var employeeData = new
            {
                first_name = employee.FirstName,
                last_name = employee.Name,
                designation = employee.Position,
                department = employee.Department,
                date_of_birth = employee.DateOfBirth?.ToString("yyyy-MM-dd"),
                date_of_joining = employee.HiringDate?.ToString("yyyy-MM-dd"),
                status = employee.Status,
                gender = employee.Gender,
                company_email = employee.Email,
                company = employee.Company,
                image = employee.PhotoUrl
            };

            string jsonContent = JsonConvert.SerializeObject(employeeData);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Appel POST vers l'API
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Post, "/api/resource/Employee", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la création de l'employé : {Error}", error);
                return false;
            }

            _logger.LogInformation("Employé créé avec succès : {Id}", employee.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'employé");
            return false;
        }
    }
    
    public async Task<bool> DeleteEmployeeAsync(string employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        try
        {
            var endpoint = $"/api/resource/Employee/{employee}";
            // Appel Delete vers l'API
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Delete, endpoint, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la suppression de l'employé : {Error}", error);
                return false;
            }

            _logger.LogInformation("Employé supprimé avec succès : {Id}", employee);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'employé");
            return false;
        }
    }
    
    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        try
        {
            var employeeData = new
            {
                first_name = employee.FirstName,
                last_name = employee.Name,
                designation = employee.Position,
                department = employee.Department,
                date_of_birth = employee.DateOfBirth?.ToString("yyyy-MM-dd"),
                date_of_joining = employee.HiringDate?.ToString("yyyy-MM-dd"),
                status = employee.Status,
                gender = employee.Gender,
                company_email = employee.Email,
                company = employee.Company,
                image = employee.PhotoUrl
            };

            string jsonContent = JsonConvert.SerializeObject(employeeData);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var endpoint = $"/api/resource/Employee/{employee.Id}";

            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Put, endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la modification de l'employé : {Error}", error);
                return false;
            }

            _logger.LogInformation("Employé modifié avec succès : {Id}", employee.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de l'employé");
            return false;
        }
    }

}
