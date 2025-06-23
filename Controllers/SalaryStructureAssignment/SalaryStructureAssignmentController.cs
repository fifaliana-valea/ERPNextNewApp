using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.SalaryStructureAssignment;
using ERPNextNewApp.Services.Utile;
using ERPNextNewApp.ViewModels.SalaryStructureAssignment;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.SalaryStructureAssignment;

public class SalaryStructureAssignmentController : Controller
{
    public readonly ILogger<SalaryStructureAssignmentController> _logger;
    public readonly ISalaryStructureAssignmentService _salaryStructureAssignmentService;
    public readonly IEmployeeService _employeeService;
    public readonly IUtileService  _utileService;

    public SalaryStructureAssignmentController(ILogger<SalaryStructureAssignmentController> logger,
        ISalaryStructureAssignmentService salaryStructureAssignmentService,IEmployeeService employeeService,IUtileService utileService)
    {
        _logger = logger;
        _salaryStructureAssignmentService = salaryStructureAssignmentService;
        _employeeService = employeeService;
        _utileService = utileService;
    }
    // GET
    public async Task<IActionResult> Index()
    {
        var model = new InsertSalaryStructureAssignment
        {
            Employees = await _employeeService.GetEmployeesAllAsync()
        };

        if (model.Employees == null || !model.Employees.Any())
        {
            TempData["Error"] = "Aucun employé trouvé dans la base de données.";
        }

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Index(InsertSalaryStructureAssignment model)
    {
        if (!ModelState.IsValid)
        {
            model.Employees = await _employeeService.GetEmployeesAllAsync();
            TempData["Error"] = "Veuillez corriger les erreurs du formulaire.";
            return View(model);
        }

        if (model.DateDebut == null || model.DateFin == null || model.DateDebut > model.DateFin)
        {
            ModelState.AddModelError("", "Les dates sont invalides.");
            model.Employees = await _employeeService.GetEmployeesAllAsync();
            return View(model);
        }

        var dateTimes = _utileService.GetListeDate(model.DateDebut.Value, model.DateFin.Value);

        var resultat = await _salaryStructureAssignmentService.InsertWithConditionAsync(
            dateTimes, model.EmployeeId, model.Salary
        );

        if (resultat.resultat)
        {
            TempData["Success"] = resultat.message;
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = resultat.message;
        return RedirectToAction(nameof(Index));
    }

}