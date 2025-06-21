using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.Login;
using ERPNextNewApp.Services.SalarySlip;
using ERPNextNewApp.Services.SalaryStructure;
using ERPNextNewApp.Services.Utile;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ERPNextNewApp.Services.SalaryStructureAssignment;

public class SalaryStructureAssignmentService : ISalaryStructureAssignmentService
{
    public readonly ILogger<SalaryStructureAssignmentService> _logger;
    public readonly ILoginService _loginService;
    public readonly IEmployeeService _employeeService;
    public readonly ISalarySlipService  _salarySlipService;
    public readonly IUtileService _utileService;
    public readonly ISalaryStructureService  _salaryStructureService;

    public SalaryStructureAssignmentService(ILogger<SalaryStructureAssignmentService> logger, ILoginService loginService
        , IEmployeeService employeeService, ISalarySlipService salarySlipService, IUtileService utileService, ISalaryStructureService salaryStructureService)
    {
        _logger = logger;
        _loginService = loginService;
        _employeeService = employeeService;
        _salarySlipService = salarySlipService;
        _utileService = utileService;
        _salaryStructureService = salaryStructureService;
    }
    
    public async Task<List<Models.Salary.SalaryStructureAssignment>> GetSalaryAssigmentAsync(string? employeId = null, DateTime? dateDebut = null,DateTime? dateFin = null)
    {
        try
        {
            var filters = new List<object[]>();
            
            // if (!string.IsNullOrWhiteSpace(company))
            //     filters.Add(new object[] { "company", "=", company });

            if (!string.IsNullOrWhiteSpace(employeId))
                filters.Add(new object[] { "employee", "=", employeId });

            if (dateDebut.HasValue)
                filters.Add(new object[] { "from_date", ">=", dateDebut.Value.ToString("yyyy-MM-dd") });
            
            if (dateFin.HasValue)
                filters.Add(new object[] { "from_date", "<=", dateFin.Value.ToString("yyyy-MM-dd") });
            
            filters.Add(new object[] { "docstatus", "=", 1 });

            var filtersJson = JsonSerializer.Serialize(filters);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fields"] = JsonSerializer.Serialize(new[]
            {
                "name", "employee", "employee_name", "salary_structure", "from_date", "base", "company", "docstatus"
            });
            query["limit_page_length"] = "0";
            query["filters"] = filtersJson;

            var endpoint = $"/api/resource/Salary Structure Assignment?{query}&order_by=from_date desc";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
            response.EnsureSuccessStatusCode();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var salaryStructureAssignments = JsonSerializer.Deserialize<List<Models.Salary.SalaryStructureAssignment>>(jsonDoc.RootElement.GetProperty("data")) ?? new();

            return salaryStructureAssignments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des salary structure assignment avec filtre");
            throw;
        }
    }

    
    public async Task<Models.Salary.SalaryStructureAssignment?> GetSalaryAssigmentByIdAsync(string salaryAssigmentId)
    {
        if (string.IsNullOrEmpty(salaryAssigmentId))
            return null;

        try
        {
            var endpoint = $"/api/resource/Salary Structure Assignment/{salaryAssigmentId}?fields=[\"name\", \"employee\",\"employee_name\", \"salary_structure\",\"from_date\", \"base\", \"company\",\"docstatus\"]";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Échec lors de la récupération du salaire structure assigment {SalaryAssigmentId} - Statut: {Status}", salaryAssigmentId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var salaryAssigmentJson = doc.RootElement.GetProperty("data");

            var salaryStructureAssignment = JsonSerializer.Deserialize<Models.Salary.SalaryStructureAssignment>(salaryAssigmentJson.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return salaryStructureAssignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à GetSalaryAssigmentByIdAsync pour {SalaryAssigmentId}", salaryAssigmentId);
            return null;
        }
    }
    
    public async Task<bool> InsertSalaryStructureAssigmentAsync(Models.Salary.SalaryStructureAssignment salaryStructureAssignment)
    {
        if (salaryStructureAssignment == null)
            throw new ArgumentNullException(nameof(salaryStructureAssignment));

        try
        {
            var json = JsonSerializer.Serialize(salaryStructureAssignment, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            });
            _logger.LogInformation("Payload JSON généré : {Json}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _loginService.MakeAuthenticatedRequest(
                HttpMethod.Post,
                "/api/resource/Salary Structure Assignment",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la création du salary structure assigment : {Error}", error);
                return false;
            }

            _logger.LogInformation("Salary structure assigment créé avec succès : {Name}", salaryStructureAssignment.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du salary structure assigment");
            return false;
        }
    }

    public async Task<bool> UpdateSalaryStructureAssigmentAsync(Models.Salary.SalaryStructureAssignment salaryStructureAssignment)
    {
        if (salaryStructureAssignment == null)
            throw new ArgumentNullException(nameof(salaryStructureAssignment));

        try
        {
            var json = JsonSerializer.Serialize(salaryStructureAssignment, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            });
            _logger.LogInformation("Payload JSON généré : {Json}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _loginService.MakeAuthenticatedRequest(
                HttpMethod.Put,
                $"/api/resource/Salary Structure Assignment/{salaryStructureAssignment.Name}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la modification du salary structure assigment : {Error}", error);
                return false;
            }

            _logger.LogInformation("Salary structure assigment modifie avec succès : {Name}", salaryStructureAssignment.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification du salary structure assigment");
            return false;
        }
    }
    public async Task<(bool resultat,string message)> InsertWithConditionAsync(List<DateTime> dateTimes, string employee, decimal salary)
    {
        string message;
        if (dateTimes == null || dateTimes.Count == 0)
            return (false,"Aucun date trouver");
        
        Employee emp = new Employee();
        if (string.IsNullOrWhiteSpace(employee))
        {
            message = $"l employe ne doit pas etre null";
            return (false,message);
        }
        else
        {
            emp = await _employeeService.GetEmployeeByIdAsync(employee);
            if (emp == null)
            {
                _logger.LogWarning("Aucun employé trouvé avec l'ID {Employee}", employee);
                message = $"Aucun employé trouvé avec l'ID {employee}";
                return (false,message);
            }
        }
       
        
        Models.Salary.SalaryStructureAssignment baseAssignment = new Models.Salary.SalaryStructureAssignment();
        if (salary > 0)
        {
            var salaryStructure = await _salaryStructureService.GetSalaryStructureCompanyAsync(emp.Company);
            baseAssignment.Company = salaryStructure.CompanyName;
            baseAssignment.SalaryStructure = salaryStructure.Name;
            baseAssignment.BaseSalary = salary;
        }
        else
        {
            var listSalaryAssigment = await GetSalaryAssigmentAsync(employee, null, dateTimes[0]);
            if (listSalaryAssigment == null || listSalaryAssigment.Count == 0)
            {
                _logger.LogWarning("Aucun SalaryStructureAssignment trouvé pour l'employé {Employee} à la date {Date}", employee, dateTimes[0]);
                message = $"Aucun SalaryStructureAssignment trouvé pour l'employé {employee} à la date {dateTimes[0]}";
                return (false,message);
            }
            baseAssignment = listSalaryAssigment[0];
        }


        foreach (var date in dateTimes)
        {
            var verifDates = _utileService.GetDate(date);
            var verifAssigment = await GetSalaryAssigmentAsync(employee, verifDates[0], verifDates[1]);
            if (verifAssigment == null || verifAssigment.Count == 0)
            {
                var newAssigment = new Models.Salary.SalaryStructureAssignment
                {
                    Employee = emp.Id,
                    BaseSalary = baseAssignment.BaseSalary,
                    Company = emp.Company,
                    SalaryStructure = baseAssignment.SalaryStructure,
                    Docstatus = 1,
                    FromDate = verifDates[0]
                };

                var newSlips = new Models.Salary.SalarySlip
                {
                    Employee = emp.Id,
                    Structure_salary = baseAssignment.SalaryStructure,
                    StartDate = verifDates[0],
                    EndDate = verifDates[1],
                    PostingDate = verifDates[0],
                    Company = emp.Company
                };

                var successAssigment = await InsertSalaryStructureAssigmentAsync(newAssigment);
                if (!successAssigment)
                {
                    _logger.LogError("Échec d'insertion du salary strutcure assigment pour {Emp}", emp.Id);
                    message = $"<UNK>chec d'insertion du salary strutcure assigment pour {emp.Id}";
                    return (false,message);
                }

                var successSlips = await _salarySlipService.InsertSalarySlipsAsync(newSlips);
                if (!successSlips)
                {
                    _logger.LogError("Échec d'insertion du salary slips pour {Emp}", emp.Id);
                    message = $"<UNK>chec d'insertion salary slips pour {emp.Id}";
                    return (false,message);
                }
            }
        }
        return (true, "insertion avec success");
    }
    
    public async Task<bool> DeleteAssigmentsAsync(string assigment)
    {
        if (string.IsNullOrWhiteSpace(assigment))
            throw new ArgumentNullException(nameof(assigment));

        try
        {
            // Étape 1 : Annuler le Salary Structure Assignment (passage à docstatus = 2)
            var cancelEndpoint = $"/api/resource/Salary Structure Assignment/{assigment}";
            var cancelPayload = new
            {
                docstatus = 2
            };

            var cancelContent = new StringContent(
                JsonSerializer.Serialize(cancelPayload),
                Encoding.UTF8,
                "application/json");

            var cancelResponse = await _loginService.MakeAuthenticatedRequest(HttpMethod.Put, cancelEndpoint, cancelContent);

            if (!cancelResponse.IsSuccessStatusCode)
            {
                var error = await cancelResponse.Content.ReadAsStringAsync();
                _logger.LogError("Échec de l'annulation du salary assignment : {Error}", error);
                return false;
            }

            _logger.LogInformation("Salary assignment annulé avec succès : {Id}", assigment);

            // Étape 2 : Supprimer le Salary Structure Assignment
            var deleteEndpoint = $"/api/resource/Salary Structure Assignment/{assigment}";
            var deleteResponse = await _loginService.MakeAuthenticatedRequest(HttpMethod.Delete, deleteEndpoint, null);

            if (!deleteResponse.IsSuccessStatusCode)
            {
                var error = await deleteResponse.Content.ReadAsStringAsync();
                _logger.LogError("Échec de la suppression du salary assignment : {Error}", error);
                return false;
            }

            _logger.LogInformation("Salary assignment supprimé avec succès : {Id}", assigment);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'annulation ou de la suppression du salary assignment");
            return false;
        }
    }

    public async Task<bool> ModificationAssigmentsAsync(Models.Salary.SalaryStructureAssignment assignment)
    {
        if (assignment == null || string.IsNullOrWhiteSpace(assignment.Name))
        {
            _logger.LogWarning("Tentative de modification avec un assignment invalide.");
            return false;
        }

        try
        {
            _logger.LogInformation("Début de la modification de Salary Assignment : {Id}", assignment.Name);

            bool deleted = await DeleteAssigmentsAsync(assignment.Name);

            if (!deleted)
            {
                _logger.LogWarning("Échec de la suppression de l'ancien Salary Assignment : {Id}", assignment.Name);
                return false;
            }

            bool inserted = await InsertSalaryStructureAssigmentAsync(assignment);

            if (!inserted)
            {
                _logger.LogWarning("Échec de l'insertion du nouveau Salary Assignment : {Id}", assignment.Name);
                return false;
            }

            _logger.LogInformation("Modification du Salary Assignment réussie : {Id}", assignment.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors de la modification du Salary Assignment : {Id}", assignment?.Name);
            return false;
        }
    }


    public async Task<(bool resultat, string message)> ModificationWithCondition(decimal salary, int condition, string componentName, int pourcentage, int action)
    {
        var messages = new StringBuilder();
        bool allSuccess = true;

        try
        {
            var slips = await _salarySlipService.GetSalarySlipsWithConditionAsync(salary, condition, componentName);
            if (slips == null || slips.Count == 0)
            {
                const string errorsMessage = "Aucun éléments trouvé.";
                _logger.LogInformation(errorsMessage);
                return (false, errorsMessage);
            }

            foreach (var slip in slips)
            {
                var assignments = await GetSalaryAssigmentAsync(slip.Employee, slip.StartDate, slip.StartDate);

                if (assignments == null || !assignments.Any())
                {
                    var msg = $"Aucune salary assignment correspondant au salary slip : {slip.Name}.";
                    _logger.LogWarning(msg);
                    messages.AppendLine(msg);
                    allSuccess = false;
                    continue; 
                }

                var assignment = assignments[0];

                if (action == 1)
                {
                    assignment.BaseSalary += (assignment.BaseSalary * pourcentage) / 100;
                }
                else
                {
                    assignment.BaseSalary -= (assignment.BaseSalary * pourcentage) / 100;
                }

                bool assignmentResult = await ModificationAssigmentsAsync(assignment);
                if (!assignmentResult)
                {
                    var msg = $"Échec de modification du Salary Assignment : {assignment.Name}.";
                    _logger.LogWarning(msg);
                    messages.AppendLine(msg);
                    allSuccess = false;
                    continue;
                }

                bool slipResult = await _salarySlipService.ModificationSlipsAsync(slip);
                if (!slipResult)
                {
                    var msg = $"Échec de modification du Salary Slip : {slip.Name}.";
                    _logger.LogWarning(msg);
                    messages.AppendLine(msg);
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                const string successMessage = "Modification avec succès pour tous les éléments.";
                _logger.LogInformation(successMessage);
                return (true, successMessage);
            }
            else
            {
                return (false, messages.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors de la modification des salary slips/assignments.");
            return (false, "Erreur inattendue lors de la modification.");
        }
    }


}