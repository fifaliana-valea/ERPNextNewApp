using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.Employees;

public class EmployeesController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}