using ERPNextNewApp.Models.Request;
using ERPNextNewApp.Services.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ERPNextNewApp.Controllers.Login;

public class LoginController : Controller
{
    private readonly ILoginService _loginService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILoginService loginService, ILogger<LoginController> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(AuthRequest authRequest)
    {
        if (!ModelState.IsValid)
            return View("index", authRequest);

        try
        {
            var authResponse = await _loginService.LoginAsync(authRequest);

            if (authResponse == null || authResponse.Message != "Logged In")
            {
                ModelState.AddModelError(string.Empty, "Identifiants invalides");
                return View("index", authRequest);
            }

            _logger.LogInformation("Utilisateur connecté: {FullName}", authResponse.FullName);
            HttpContext.Session.SetString("FullName", authResponse.FullName);
            _logger.LogInformation("Utilisateur connecté 2: {FullName}", HttpContext.Session.GetString("FullName"));
                
            return RedirectToAction("Index","Employees");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            ModelState.AddModelError(string.Empty, "Erreur lors de la connexion");
            return View("index", authRequest);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, "/api/method/logout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion d'ERPNext");
        }
            
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
            
        return RedirectToAction("Index");
    }
}