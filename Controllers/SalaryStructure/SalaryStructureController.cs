using ERPNextNewApp.Services.Company;
using ERPNextNewApp.Services.SalaryComponent;
using ERPNextNewApp.Services.SalaryStructure;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.SalaryStructure;

public class SalaryStructureController : Controller
{
    public readonly ISalaryStructureService _salaryStructureService;
    public readonly ILogger<SalaryStructureController> _logger;
    public readonly ICompanyService _companyService;
    public readonly ISalaryComponentService _salaryComponentService;

    public SalaryStructureController(ILogger<SalaryStructureController> logger, ICompanyService companyService,
        ISalaryComponentService salaryComponentService, ISalaryStructureService salaryStructureService)
    {
        _logger = logger;
        _companyService = companyService;
        _salaryComponentService = salaryComponentService;
        _salaryStructureService = salaryStructureService;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var components = await _salaryComponentService.GetAllSalaryComponentsAsync();
            var companies = await _companyService.GetAllCompanysAsync();

            var earnings = components.Where(c => c.Type == "Earning").ToList();
            var deductions = components.Where(c => c.Type == "Deduction").ToList();

            ViewBag.Companies = companies;
            ViewBag.Earnings = earnings;
            ViewBag.Deductions = deductions;

            return View();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "Erreur lors du chargement des données.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Insert([FromForm] Models.Salary.SalaryStructure structure)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
            
                _logger.LogError("Validation errors: {Errors}", string.Join(", ", errors));
            
                // Pour le débogage, retournez les données reçues
                var formData = await Request.ReadFormAsync();
                _logger.LogInformation("Received form data: {FormData}", formData);
            
                return BadRequest(new {
                    Message = "Invalid form data",
                    Errors = errors,
                    ReceivedData = structure
                });
            }

            var result = await _salaryStructureService.UpsertSalaryStructureAsync(structure);

            if (!result)
            {
                _logger.LogError("Failed to insert salary structure");
                TempData["Error"] = "Erreur lors insertion salary structure";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Insertion avec sucess";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Insert action");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}