using ERPNextNewApp.Models;
using ERPNextNewApp.Services.SalarySlip;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.SalaryComponent;
using ERPNextNewApp.Services.SalaryStructureAssignment;
using ERPNextNewApp.Services.Utile;
using ERPNextNewApp.ViewModels.SalarySlips;
using ERPNextNewApp.ViewModels.SalaryStructureAssignment;
using Microsoft.AspNetCore.Authorization;

namespace ERPNextNewApp.Controllers
{
    [Authorize]
    public class SalarySlipsController : Controller
    {
        private readonly ISalarySlipService _salarySlipService;
        private readonly IEmployeeService _employeeService;
        private readonly IUtileService _utileService;
        private readonly ISalaryComponentService _salaryComponentService;
        private readonly ISalaryStructureAssignmentService  _salaryStructureAssignmentService;
        private readonly ILogger<SalarySlipsController> _logger;

        public SalarySlipsController(
            ISalarySlipService salarySlipService,
            IEmployeeService employeeService,
            IUtileService utileService,
            ISalaryStructureAssignmentService salaryStructureAssignmentService,
            ISalaryComponentService salaryComponentService,
            ILogger<SalarySlipsController> logger)
        {
            _salarySlipService = salarySlipService;
            _employeeService = employeeService;
            _utileService = utileService;
            _salaryComponentService = salaryComponentService;
            _salaryStructureAssignmentService = salaryStructureAssignmentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string employeeId = null, int mois = 0, int annee = 0, int page = 1, int pageSize = 5)
        {
            try
            {
                _logger.LogInformation("Appel Index avec paramètres : employeeId = {EmployeeId}, mois = {Mois}, annee = {Annee}, page = {Page}, pageSize = {PageSize}",
                    employeeId, mois, annee, page, pageSize);

                var salarySlips = await _salarySlipService.GetSalarySlipsMonthYearsAsync(employeeId, mois, annee);

                Employee employee = null;
                if (!string.IsNullOrEmpty(employeeId))
                {
                    employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                }

                var totalItems = salarySlips.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var paginatedSlips = salarySlips
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new SalarySlipIndexViewModel
                {
                    Employee = employee,
                    SalarySlips = paginatedSlips,
                    SelectedEmployeeId = employeeId,
                    SelectedMonth = mois,
                    SelectedYear = annee,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPage = totalPages,
                    Months = Enumerable.Range(1, 12)
                        .Select(m => new { Id = m, Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                        .ToDictionary(m => m.Id, m => m.Name),
                    Years = Enumerable.Range(DateTime.Now.Year - 5, 10)
                        .OrderByDescending(y => y)
                        .ToDictionary(y => y, y => y.ToString())
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des fiches de paie");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des fiches de paie.";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                // Récupérer les détails de la fiche de paie (vous devrez implémenter cette méthode dans le service)
                var salarySlip = await _salarySlipService.GetSalarySlipDetailAsync(id);
                
                if (salarySlip == null)
                {
                    return NotFound();
                }

                return View(salarySlip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des détails de la fiche de paie {id}");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des détails de la fiche de paie.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> ListeSalarySlips(
            string employeeId = null, int mois = 0, int annee = 0, int page = 1, int pageSize = 10)
        {
            try
            {
                // Récupérer les fiches de paie avec les détails pour affichage
                var salarySlipsData = await _salarySlipService.GetSalaryDisplayAsync(page, pageSize, mois, annee, employeeId);
                ViewData["employees"] = await _employeeService.GetEmployeesAllAsync();
                return View(salarySlipsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des fiches de paie.");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des fiches de paie.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Statistique(int annee = 0)
        {
            try
            {
                var model = await _salarySlipService.GetSalaryStatistiqueAsync(annee);
                ViewBag.SelectedYear = annee; // Envoie l'année à la vue
                return View(model);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erreur lors de la récupération  statistiques.");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération statistiques.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        public async Task<IActionResult> GrapheStatistique(int annee = 0)
        {
            try
            {
                var model = await _salarySlipService.GetSalaryStatistiqueAsync(annee);
                ViewBag.SelectedYear = annee;
                return View(model); // Assure-toi que `model` contient les valeurs mensuelles
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erreur lors de la récupération Graphe statistiques.");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération Graphe statistiques.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        public async Task<IActionResult> DetailsSalaire(string mois, int annee = 0, int page = 1, int pageSize = 10)
        {
            try
            {
                // Conversion du nom du mois en numéro (1-12)
                int month = DateTime.TryParseExact(mois, "MMMM", new CultureInfo("fr-FR"), DateTimeStyles.None, out var date)
                    ? date.Month
                    : 0;

                if (month == 0)
                {
                    TempData["ErrorMessage"] = "Le mois fourni est invalide.";
                    return RedirectToAction(nameof(Index));
                }

                var salarySlips = await _salarySlipService.GetSalaryDisplayAsync(page, pageSize, month, annee, null);
        
                // Vérification des données reçues
                if (salarySlips == null || salarySlips.Rows == null || !salarySlips.Rows.Any())
                {
                    TempData["WarningMessage"] = "Aucune donnée disponible pour les critères sélectionnés.";
                }

                ViewBag.Mois = mois;
                ViewBag.Annee = annee;
                ViewBag.PageSize = pageSize;

                return View(salarySlips);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erreur lors de la récupération des détails de salaire.");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des détails de salaire.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        public async Task<IActionResult> DownloadSalarySlip(string slipId)
        {
            try
            {
                var salarySlip = await _salarySlipService.GetSalarySlipDetailAsync(slipId);
                var pdfBytes = await _salarySlipService.CreateProfessionalPdf(salarySlip);
        
                return File(pdfBytes, "application/pdf", $"FicheDePaie_{salarySlip.Name}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du PDF");
                return StatusCode(500, "Erreur lors de la génération du PDF");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ModifWithFilters()
        {
            try
            {
                var components = await _salaryComponentService.GetAllSalaryComponentsAsync();
                var model = new ModifViews
                {
                    SalaireSalaire = components
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des salary components");
                return StatusCode(500, "Erreur serveur.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ModifWithFilters(ModifViews model)
        {
            try
            {
                model.SalaireSalaire = await _salaryComponentService.GetAllSalaryComponentsAsync();

                var salarySlips = await _salarySlipService.GetSalarySlipsWithConditionAsync(
                    model.Salary, model.Condition, model.Component);

                model.SalarySlips = salarySlips;

                var resultat = await _salaryStructureAssignmentService.ModificationWithCondition(
                    model.Salary, model.Condition, model.Component, model.Pourcentage, model.Action);

                if (resultat.resultat)
                {
                    ViewBag.Success = resultat.message;  
                }
                else
                {
                    ViewBag.Error = resultat.message;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification des salary slips");
                ViewBag.Error = "Erreur inattendue lors du traitement.";
                return View(model);
            }
        }
        
    }
    
}