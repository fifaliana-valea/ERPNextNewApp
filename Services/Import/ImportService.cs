using System.Globalization;
using System.Text;
using System.Text.Json;
using ERPNextNewApp.Models.Dto;
using ERPNextNewApp.Services.Login;

namespace ERPNextNewApp.Services.Import;

public class ImportService : IImportService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<ImportService> _logger;

    public ImportService(ILoginService loginService, ILogger<ImportService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }
    
    public async Task<(List<EmployeeDto> data, List<string> errors)> LoadAndValidateEmployees(string filePath)
    {
        var result = new List<EmployeeDto>();
        var errors = new List<string>();
        var lines = await File.ReadAllLinesAsync(filePath);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Split(','); // comma-separated
            try
            {
                if (line.Length < 7)
                    throw new Exception("Colonnes manquantes");

                var emp = new EmployeeDto
                {
                    Id = line[0].Trim(),
                    LastName = line[1].Trim(),
                    FirstName = line[2].Trim(),
                    Gender = line[3].Trim(),
                    HireDate = DateTime.ParseExact(line[4].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    BirthDate = DateTime.ParseExact(line[5].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Company = line[6].Trim()
                };

                if (string.IsNullOrWhiteSpace(emp.Id)) throw new Exception("ID vide");
                if (emp.HireDate < emp.BirthDate) throw new Exception("Date embauche avant naissance");

                result.Add(emp);
            }
            catch (Exception ex)
            {
                errors.Add($"[employees.csv][Ligne {i + 1}] {ex.Message} → {lines[i]}");
            }
        }

        return (result, errors);
    }
    
    public async Task<(List<SalaryStructureDto> data, List<string> errors)> LoadAndValidateSalaryStructures(string filePath)
    {
        var result = new List<SalaryStructureDto>();
        var errors = new List<string>();
        var lines = await File.ReadAllLinesAsync(filePath);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Split(',');
            try
            {
                if (line.Length < 5)
                    throw new Exception("Colonnes manquantes");

                var structure = new SalaryStructureDto
                {
                    StructureCode = line[0].Trim(),
                    Name = line[1].Trim(),
                    Abbreviation = line[2].Trim(),
                    Type = line[3].Trim().ToLower(),
                    Value = line[4].Trim(),
                    Remark = line.Length > 5 ? line[5].Trim() : null
                };

                if (string.IsNullOrWhiteSpace(structure.StructureCode)) throw new Exception("Code vide");
                if (structure.Type != "earning" && structure.Type != "deduction") throw new Exception("Type invalide");

                result.Add(structure);
            }
            catch (Exception ex)
            {
                errors.Add($"[structures.csv][Ligne {i + 1}] {ex.Message} → {lines[i]}");
            }
        }

        return (result, errors);
    }
    
    public async Task<(List<SalarySlipDto> data, List<string> errors)> LoadAndValidateSalarySlips(string filePath)
    {
        var result = new List<SalarySlipDto>();
        var errors = new List<string>();
        var lines = await File.ReadAllLinesAsync(filePath);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Split(',');
            try
            {
                if (line.Length < 4)
                    throw new Exception("Colonnes manquantes");

                var slip = new SalarySlipDto
                {
                    StartDate = DateTime.ParseExact(line[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    EmployeeId = line[1].Trim(),
                    BaseAmount = decimal.Parse(line[2].Trim()),
                    StructureCode = line[3].Trim()
                };

                if (string.IsNullOrWhiteSpace(slip.EmployeeId)) throw new Exception("ID employé vide");

                result.Add(slip);
            }
            catch (Exception ex)
            {
                errors.Add($"[salaries.csv][Ligne {i + 1}] {ex.Message} → {lines[i]}");
            }
        }

        return (result, errors);
    }

public async Task<List<string>> ImportAllAsync(
    string employeeFile,
    string structureFile,
    string salaryFile)
{
    _logger.LogInformation("Début de l'importation des fichiers.");

    var (employees, employeeErrors) = await LoadAndValidateEmployees(employeeFile);
    var (structures, structureErrors) = await LoadAndValidateSalaryStructures(structureFile);
    var (slips, slipErrors) = await LoadAndValidateSalarySlips(salaryFile);

    var allErrors = employeeErrors.Concat(structureErrors).Concat(slipErrors).ToList();
    if (allErrors.Any())
    {
        _logger.LogWarning("Erreurs de validation détectées : {Errors}", string.Join(" | ", allErrors));
        return allErrors;
    }

    var payload = new
    {
        employees,
        salaryStructures = structures,
        salarySlips = slips
    };

    try
    {
        var json = JsonSerializer.Serialize(payload);
        _logger.LogInformation("Payload JSON généré : {Json}", json);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Envoi de la requête POST vers l'API Frappe...");

        var response = await _loginService.MakeAuthenticatedRequest(
            HttpMethod.Post,
            "api/method/custom_app.api.import_bulk_data.import_bulk_data",
            content
        );

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Réponse reçue : {ResponseContent}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur HTTP: {StatusCode} - {Reason}", (int)response.StatusCode, response.StatusCode);
            return new List<string> {
                $"Erreur serveur: {(int)response.StatusCode} - {response.StatusCode} - {responseContent}"
            };
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        ImportResponse result;
        try
        {
            result = JsonSerializer.Deserialize<ImportResponse>(responseContent, options);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Erreur lors du parsing JSON de la réponse.");
            return new List<string> { $"Erreur parsing JSON : {jsonEx.Message}" };
        }

        if (result == null)
        {
            _logger.LogError("Résultat de désérialisation null.");
            return new List<string> { "Réponse JSON invalide ou vide." };
        }

        if (!result.Message.Success)
        {
            _logger.LogWarning("Import partiellement ou totalement échoué. Message : {Message}", result.Message);
            return result.Message.Errors?.Any() == true
                ? result.Message.Errors
                : new List<string> { result.Message.Message };
        }

        _logger.LogInformation("Importation réussie sans erreur.");
        return new List<string>();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de l'importation des données.");
        return new List<string> { $"Erreur système: {ex.Message}" };
    }
}

}