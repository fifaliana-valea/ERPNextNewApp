using ERPNextNewApp.Models;
using ERPNextNewApp.Services.SalarySlip;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.Utile;
using ERPNextNewApp.ViewModels.SalarySlips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPNextNewApp.Controllers
{
    [Authorize]
    public class SalarySlipsController : Controller
    {
        private readonly ISalarySlipService _salarySlipService;
        private readonly IEmployeeService _employeeService;
        private readonly IUtileService _utileService;
        private readonly ILogger<SalarySlipsController> _logger;

        public SalarySlipsController(
            ISalarySlipService salarySlipService,
            IEmployeeService employeeService,
            IUtileService utileService,
            ILogger<SalarySlipsController> logger)
        {
            _salarySlipService = salarySlipService;
            _employeeService = employeeService;
            _utileService = utileService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string employeeId = null, int mois = 0, int annee = 0)
        {
            try
            {
                _logger.LogInformation("Appel Index avec paramètres : employeeId = {EmployeeId}, mois = {Mois}, annee = {Annee}", employeeId, mois, annee);

                // Récupérer les fiches de paie filtrées
                var salarySlips = await _salarySlipService.GetSalarySlipsAsync(employeeId, mois, annee);

                // Récupérer les infos de l'employé si un ID est spécifié
                Employee employee = null;
                if (!string.IsNullOrEmpty(employeeId))
                {
                    employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                }

                // Préparer le ViewModel
                var viewModel = new SalarySlipIndexViewModel
                {
                    Employee = employee,
                    SalarySlips = salarySlips,
                    SelectedEmployeeId = employeeId,
                    SelectedMonth = mois,
                    SelectedYear = annee,
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
        
        public async Task<IActionResult> ListeSalarySlips(int mois = 0, int annee = 0, string employeeId = null)
        {
            // Récupérer les données
            var salaryData = await _utileService.GetSalaryDisplayAsync(mois, annee, employeeId);
            var employees = await _employeeService.GetEmployeesAllAsync();

            // Préparer les données pour la vue
            var vm = new SalarySlipViewModel
            {
                SalaryRows = salaryData,
                Employees = employees,
                SelectedMonth = mois,
                SelectedYear = annee,
                SelectedEmployeeId = employeeId
            };

            // Liste des mois pour le dropdown
            ViewBag.Months = Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m),
                    Value = m.ToString()
                })
                .ToList();

            // Liste des années pour le dropdown (5 dernières années)
            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 4, 5)
                .Select(y => new SelectListItem
                {
                    Text = y.ToString(),
                    Value = y.ToString()
                })
                .OrderByDescending(y => y.Value)
                .ToList();

            return View(vm);
        }
    
    }
    
}