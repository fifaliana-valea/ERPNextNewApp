using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Departement;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.Gender;
using ERPNextNewApp.ViewModels.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.Employees;

[Authorize]
public class EmployeesController : Controller
{
     private readonly IEmployeeService _employeeService;
    private readonly IDepartementService _departementService;
    private readonly IGenderService _genderService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IEmployeeService employeeService, 
        IDepartementService departementService, 
        IGenderService genderService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _departementService = departementService;
        _genderService = genderService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        string nom,
        string departement,
        string status,
        string genre,
        DateTime? dateEmbaucheDebut,
        DateTime? dateEmbaucheFin,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var result = await _employeeService.GetEmployeesAsync(
                nom,
                departement,
                status,
                genre,
                dateEmbaucheDebut,
                dateEmbaucheFin,
                page,
                pageSize);

            // Gestion des erreurs de récupération des listes
            List<Department> departments = new();
            List<Gender> genders = new();
            
            try
            {
                departments = await _departementService.GetAllDepartmentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des départements");
            }
            
            try
            {
                genders = await _genderService.GetAllGendersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des genres");
            }

            ViewData["Departement"] = departments;
            ViewData["Gender"] = genders;

            int totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            var viewModel = new EmployeesListViewModel
            {
                Employees = result.Employees,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Filters = new EmployeeFilters
                {
                    Nom = nom,
                    Departement = departement,
                    Status = status,
                    Genre = genre,
                    DateEmbaucheDebut = dateEmbaucheDebut,
                    DateEmbaucheFin = dateEmbaucheFin
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans l'action Index des employés");
            ViewBag.ErrorMessage = "Une erreur est survenue lors du chargement des données";
            return View(new EmployeesListViewModel());
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] List<string> employeeNames)
    {
        if (employeeNames == null || !employeeNames.Any())
        {
            return BadRequest(new { success = false, message = "Aucun employé sélectionné." });
        }

        try
        {
            var result = await _employeeService.DeleteEmployeesAsync(employeeNames);
        
            if (result)
            {
                return Ok(new { 
                    success = true, 
                    message = $"{employeeNames.Count} employé(s) supprimé(s) avec succès !" 
                });
            }
        
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { success = false, message = "Erreur lors de la suppression" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression des employés");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { success = false, message = "Erreur interne du serveur" });
        }
    }
}