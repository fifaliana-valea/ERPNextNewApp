using ERPNextNewApp.Services.Utile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers;

[Authorize]
public class UtileController : Controller
{
    private readonly IUtileService _utileService;

    public UtileController(IUtileService utileService)
    {
        _utileService = utileService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetData()
    {
        var message = await _utileService.ResetSelectedPayrollDataAsync();
        TempData["ResetMessage"] = message;
        return RedirectToAction("Index");
    }
}