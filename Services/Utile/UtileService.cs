using System.Text.Json;
using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.Services.Login;
using ERPNextNewApp.Services.SalarySlip;
using Newtonsoft.Json;

namespace ERPNextNewApp.Services.Utile;

public class UtileService : IUtileService
{
    private readonly ISalarySlipService _salaryService;
    private readonly ILoginService _loginService;
    private readonly ILogger<UtileService> _logger;

    public UtileService(ISalarySlipService salaryService,ILoginService loginService, ILogger<UtileService> logger)
    {
        _salaryService = salaryService;
        _loginService = loginService;
        _logger = logger;
    }
    
    public async Task<List<string>> GetDocumentNamesByDoctypeAsync(string doctype)
    {
        var names = new List<string>();

        if (string.IsNullOrWhiteSpace(doctype))
            return names;

        try
        {
            // Construction de l'endpoint avec le doctype et champ "name"
            string endpoint = $"/api/resource/{doctype}?fields=[\"name\"]&limit_page_length=0";

            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erreur API : {StatusCode} - {Content}", response.StatusCode, errorContent);
                return names;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
                return names;

            foreach (var item in data.EnumerateArray())
            {
                if (item.TryGetProperty("name", out var nameProp))
                {
                    names.Add(nameProp.GetString());
                }
            }

            return names;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des noms du DocType {DocType}", doctype);
            return names;
        }
    }

    
    public async Task<string> ResetSelectedPayrollDataAsync()
    {
        try
        {
            var url = "/api/method/custom_reset.api.reset_data.reset_data";
            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Post, url, null);

            if (!response.IsSuccessStatusCode)
                return "Erreur de connexion à l'API.";

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(responseBody);

            if (json.status == "success")
            {
                var details = string.Join("<br>", json.details);
                return $"{json.message}<br><br>Détails:<br>{details}";
            }
            return $"Erreur: {json.message}";
        }
        catch (Exception ex)
        {
            return $"Erreur critique: {ex.Message}";
        }
    }

    public List<DateTime> GetListeDate(DateTime? dateDebut, DateTime? dateFin)
    {
        List<DateTime> liste = new List<DateTime>();

        if (dateDebut == null || dateFin == null)
            return liste;

        DateTime debut = dateDebut.Value;
        DateTime fin = dateFin.Value;

        DateTime current = debut;

        while (true)
        {
            if (current.AddMonths(1) > fin)
            {
                liste.Add(fin);
                break;
            }

            liste.Add(current);
            current = current.AddMonths(1);
        }

        return liste;
    }
    
    public List<DateTime> GetDate(DateTime date)
    {
        List<DateTime> liste = new List<DateTime>();

        DateTime premierJour = new DateTime(date.Year, date.Month, 1);
        liste.Add(premierJour);

        DateTime dernierJour = premierJour.AddMonths(1).AddDays(-1);
        liste.Add(dernierJour);

        return liste;
    }


}