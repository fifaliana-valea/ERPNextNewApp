using ERPNextNewApp.Models;
using ERPNextNewApp.Services.Departement;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.Gender;
using ERPNextNewApp.Services.Utile;
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
    private readonly IUtileService _utileService;

    public EmployeesController(
        IEmployeeService employeeService, 
        IDepartementService departementService, 
        IGenderService genderService,
        IUtileService utileService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _departementService = departementService;
        _genderService = genderService;
        _logger = logger;
        _utileService = utileService;
    }

    public async Task<IActionResult> Index(
        string? nom,
        string? departement,
        string? status,
        string? genre,
        DateTime? dateEmbaucheDebut,
        DateTime? dateEmbaucheFin,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            // Appel du service pour récupérer les employés filtrés
            var result = await _employeeService.GetEmployeesAsync(
                nom,
                departement,
                status,
                genre,
                dateEmbaucheDebut,
                dateEmbaucheFin,
                page,
                pageSize);

            // Récupération sécurisée des départements
            List<Department> departments = new();
            try
            {
                departments = await _departementService.GetAllDepartmentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des départements");
            }

            // Récupération sécurisée des genres
            List<Gender> genders = new();
            try
            {
                genders = await _genderService.GetAllGendersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des genres");
            }

            // Transmission des listes à la vue via ViewData
            ViewData["Departement"] = departments;
            ViewData["Gender"] = genders;

            // Construction du ViewModel pour la vue
            var viewModel = new EmployeesListViewModel
            {
                Employees = result.Employees,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
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

    public async Task<IActionResult> Edit(string employeeId)
    {
        try
        {
            var departments = await _departementService.GetAllDepartmentsAsync();
            var genders = await _genderService.GetAllGendersAsync();
            var designations = await _utileService.GetDocumentNamesByDoctypeAsync("Designation");
            var company = await _utileService.GetDocumentNamesByDoctypeAsync("Company");

            var viewModel = new EmployeeViewModels
            {
                Departments = departments,
                Designations = designations,
                Genders = genders,
                CompanyNames = company,
                Employee = await _employeeService.GetEmployeeByIdAsync(employeeId)
            };

            // ✅ Affiche la vue "Create" en réutilisant le même modèle
            return View("Create", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans le chargement du formulaire d'insertion.");
            TempData["ErrorMessage"] = "Impossible de charger le formulaire.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EmployeeViewModels employeeViewModels)
    {
        try
        {
            bool resultat = await _employeeService.UpdateEmployeeAsync(employeeViewModels.Employee);

            if (resultat)
            {
                TempData["Success"] = "Modification réussie !";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "Échec de la modification de l'employé.";
                // Recharger les listes pour la vue Edit
                employeeViewModels.Departments = await _departementService.GetAllDepartmentsAsync();
                employeeViewModels.Designations = await _utileService.GetDocumentNamesByDoctypeAsync("Designation");
                employeeViewModels.CompanyNames = await _utileService.GetDocumentNamesByDoctypeAsync("Company");
                employeeViewModels.Genders = await _genderService.GetAllGendersAsync();
                return View(employeeViewModels);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la soumission du formulaire.");
            ViewBag.Error = "Une erreur est survenue lors de la modification.";
            return View(employeeViewModels);
        }
    }



    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var departments = await _departementService.GetAllDepartmentsAsync();
            var genders = await _genderService.GetAllGendersAsync();
            var designations = await _utileService.GetDocumentNamesByDoctypeAsync("Designation");
            var company = await _utileService.GetDocumentNamesByDoctypeAsync("Company");

            var viewModel = new EmployeeViewModels
            {
                Departments = departments,
                Designations = designations,
                Genders = genders,
                CompanyNames = company,
                Employee = new Employee()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans le chargement du formulaire d'insertion.");
            TempData["ErrorMessage"] = "Impossible de charger le formulaire.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeViewModels employeeViewModels)
    {
        try
        {
            bool resultat = await _employeeService.InsertEmployeeAsync(employeeViewModels.Employee);

            if (resultat)
                ViewBag.Success = "Insertion réussie !";
            else
                ViewBag.Error = "Échec de l'insertion de l'employé.";
            
            employeeViewModels.Departments = await _departementService.GetAllDepartmentsAsync();
            employeeViewModels.Genders = await _genderService.GetAllGendersAsync();
            employeeViewModels.Designations = await _utileService.GetDocumentNamesByDoctypeAsync("Designation");
            employeeViewModels.CompanyNames = await _utileService.GetDocumentNamesByDoctypeAsync("Company");
            return View(employeeViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la soumission du formulaire.");
            ViewBag.Error = "Une erreur est survenue lors de l'insertion.";
            return View(employeeViewModels);
        }
    }
    
    public async Task<IActionResult> Delete(string employeeId)
    {
        try
        {
            bool resultat = await _employeeService.DeleteEmployeeAsync(employeeId);

            if (resultat)
                ViewBag.Success = "L'employé a été supprimé avec succès.";
            else
                ViewBag.Error = "Échec de la suppression de l'employé.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'employé.");
            ViewBag.Error = "Une erreur est survenue lors de la suppression.";
            return RedirectToAction("Index");
        }
    }
}