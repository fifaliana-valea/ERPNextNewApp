using ERPNextNewApp.Services.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.Import;

[Authorize]
public class ImportController : Controller
{

    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile employeeFile, IFormFile structureFile, IFormFile salaryFile)
    {
        if (employeeFile == null || structureFile == null || salaryFile == null)
        {
            ViewBag.Errors = new List<string> { "Tous les fichiers sont requis." };
            return View();
        }

        // Sauvegarder temporairement les fichiers
        var employeePath = Path.GetTempFileName();
        var structurePath = Path.GetTempFileName();
        var salaryPath = Path.GetTempFileName();

        using (var stream = new FileStream(employeePath, FileMode.Create)) await employeeFile.CopyToAsync(stream);
        using (var stream = new FileStream(structurePath, FileMode.Create)) await structureFile.CopyToAsync(stream);
        using (var stream = new FileStream(salaryPath, FileMode.Create)) await salaryFile.CopyToAsync(stream);

        var errors = await _importService.ImportAllAsync(employeePath, structurePath, salaryPath);

        if (errors.Count > 0)
        {
            ViewBag.Errors = errors;
        }
        else
        {
            ViewBag.Success = "Importation réussie !";
        }

        return View();
    }
}