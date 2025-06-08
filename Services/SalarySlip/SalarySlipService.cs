using System.Text.Json;
using ERPNextNewApp.Services.Login;
using System.Net;
using ERPNextNewApp.Models.Salary;
using ERPNextNewApp.ViewModels.SalarySlips;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ERPNextNewApp.Services.SalarySlip;

public class SalarySlipService : ISalarySlipService
{
    private readonly ILoginService _loginService;
    private readonly ILogger<SalarySlipService> _logger;

    public SalarySlipService(ILoginService loginService, ILogger<SalarySlipService> logger)
    {
        _loginService = loginService;
        _logger = logger;
    }
    
    public async Task<Models.Salary.SalarySlip> GetSalarySlipDetailAsync(string slipId)
    {
        if (string.IsNullOrWhiteSpace(slipId))
            throw new ArgumentException("L'identifiant du Salary Slip est requis.");

        string endpoint = $"/api/resource/Salary Slip/{slipId}?" +
                          "fields=[\"name\",\"employee\",\"employee_name\",\"salary_structure\",\"company\",\"posting_date\",\"start_date\",\"end_date\"," +
                          "\"net_pay\",\"gross_pay\",\"currency\",\"status\",\"earnings\",\"deductions\"]";

        var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Erreur lors de la récupération du détail du Salary Slip {SlipId}: {StatusCode}", slipId, response.StatusCode);
            throw new Exception("Erreur API: " + response.ReasonPhrase);
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var salarySlip = new Models.Salary.SalarySlip
        {
            Name = data.GetProperty("name").GetString(),
            Employee = data.GetProperty("employee").GetString(),
            Structure_salary = data.GetProperty("salary_structure").GetString(),
            EmployeeName = data.GetProperty("employee_name").GetString(),
            Company = data.GetProperty("company").GetString(),
            PostingDate = data.GetProperty("posting_date").GetString(),
            StartDate = data.GetProperty("start_date").GetString(),
            EndDate = data.GetProperty("end_date").GetString(),
            NetPay = data.GetProperty("net_pay").GetDecimal(),
            GrossPay = data.GetProperty("gross_pay").GetDecimal(),
            Currency = data.GetProperty("currency").GetString(),
            Status = data.GetProperty("status").GetString(),
            Earnings = JsonSerializer.Deserialize<List<SalaryComponent>>(data.GetProperty("earnings").ToString()),
            Deductions = JsonSerializer.Deserialize<List<SalaryComponent>>(data.GetProperty("deductions").ToString())
        };

        return salarySlip;
    }
    
    
    public async Task<List<Models.Salary.SalarySlip>> GetSalarySlipsMonthYearsAsync(string employeeId = null, int mois = 0, int annee = 0)
    {
        try
        {
            string baseUrl = "/api/resource/Salary Slip?";
            string fieldsPart = "fields=[\"name\",\"employee\",\"employee_name\",\"start_date\"]";

            var filters = new List<object>();

            if (!string.IsNullOrEmpty(employeeId))
            {
                filters.Add(new[] { "employee", "=", employeeId });
            }

            string filtersPart = filters.Count > 0
                ? "&filters=" + Uri.EscapeDataString(JsonSerializer.Serialize(filters))
                : "";

            string limitPart = "limit_page_length=0";
            string endpoint = $"{baseUrl}{fieldsPart}{filtersPart}&{limitPart}";

            var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erreur API: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Erreur API: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
            {
                _logger.LogWarning("Réponse API ne contient pas la propriété 'data'.");
                return new List<Models.Salary.SalarySlip>();
            }

            var slipDetailTasks = data.EnumerateArray()
                .Select(async item =>
                {
                    string slipId = item.GetProperty("name").GetString();
                    return await GetSalarySlipDetailAsync(slipId);
                });

            var slips = await Task.WhenAll(slipDetailTasks);

            var result = slips.Where(s =>
            {
                if (!DateTime.TryParse(s.StartDate, out var d))
                    return false;

                bool matchMois = mois > 0 ? d.Month == mois : true;
                bool matchAnnee = annee > 0 ? d.Year == annee : true;

                return matchMois && matchAnnee;
            }).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans GetSalarySlipsMonthYearsAsync - employeeId={employeeId}, mois={mois}, annee={annee}", employeeId, mois, annee);
            throw new ApplicationException("Une erreur est survenue lors de la récupération des fiches de paie. Veuillez réessayer.", ex);
        }
    }
    
    public async Task<AllSalarySlips> GetSalaryDisplayAsync(int page, int pageSize, int mois = 0, int annee = 0, string employeeId = null)
    {
        var allSlips = await GetSalarySlipsMonthYearsAsync(employeeId, mois, annee);
        int totalItems = allSlips.Count;
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // ✅ Pagination
        var pagedSlips = allSlips
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new List<SalaryDisplayRow>();

        // ✅ Totaux globaux (sur allSlips)
        decimal totalNet = allSlips.Sum(slip => slip.NetPay);
        decimal totalBrut = allSlips.SelectMany(slip => slip.Earnings).Sum(e => e.Amount);
        decimal totalDeduction = allSlips.SelectMany(slip => slip.Deductions).Sum(d => d.Amount);

        // ✅ Pour la page courante seulement
        var totalEarnings = new Dictionary<string, decimal>();
        var totalDeductions = new Dictionary<string, decimal>();
        var allEarningKeys = new HashSet<string>();
        var allDeductionKeys = new HashSet<string>();

        // Trouver tous les composants présents dans la page actuelle
        foreach (var slip in pagedSlips)
        {
            foreach (var earning in slip.Earnings)
            {
                allEarningKeys.Add(earning.SalaryComponentName);
            }
            foreach (var deduction in slip.Deductions)
            {
                allDeductionKeys.Add(deduction.SalaryComponentName);
            }
        }

        foreach (var slip in pagedSlips)
        {
            var row = new SalaryDisplayRow
            {
                EmployeeName = slip.EmployeeName,
                Net = slip.NetPay,
                SlipId = slip.Name,
                StartDate = slip.StartDate,
                Earnings = new Dictionary<string, decimal>(),
                DeductionsTotal = new Dictionary<string, decimal>()
            };

            foreach (var key in allEarningKeys)
                row.Earnings[key] = 0;

            foreach (var earning in slip.Earnings)
            {
                row.Earnings[earning.SalaryComponentName] += earning.Amount;

                if (!totalEarnings.ContainsKey(earning.SalaryComponentName))
                    totalEarnings[earning.SalaryComponentName] = 0;

                totalEarnings[earning.SalaryComponentName] += earning.Amount;
            }

            foreach (var key in allDeductionKeys)
                row.DeductionsTotal[key] = 0;

            foreach (var deduction in slip.Deductions)
            {
                row.DeductionsTotal[deduction.SalaryComponentName] += deduction.Amount;

                if (!totalDeductions.ContainsKey(deduction.SalaryComponentName))
                    totalDeductions[deduction.SalaryComponentName] = 0;

                totalDeductions[deduction.SalaryComponentName] += deduction.Amount;
            }

            result.Add(row);
        }

        return new AllSalarySlips
        {
            Rows = result,
            TotalItems = totalItems,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,

            // 👇 Totaux globaux (inchangés sur toutes les pages)
            TotalNet = totalNet,
            TotalBrut = totalBrut,
            TotalDeduction = totalDeduction,

            // 👇 Totaux locaux (pour la page en cours)
            TotalEarnings = totalEarnings,
            TotalDeductions = totalDeductions
        };
    }
    
    public async Task<List<Models.Salary.SalarySlip>> GetSalarySlipsStatistiqueAsync(int annee = 0)
    {
        // Champs de base sans earnings et deductions
        string baseUrl = "/api/resource/Salary Slip?";
        string fieldsPart = "fields=[\"name\",\"employee\",\"employee_name\",\"salary_structure\",\"company\",\"posting_date\",\"start_date\",\"end_date\",\"net_pay\",\"gross_pay\",\"currency\",\"status\"]";

        var filtersArray = new List<string[]>();


        string filtersPart = filtersArray.Count > 0
            ? "&filters=" + WebUtility.UrlEncode(JsonSerializer.Serialize(filtersArray))
            : "";
        

        string endpoint = baseUrl + fieldsPart + filtersPart + "&limit=0";

        var response = await _loginService.MakeAuthenticatedRequest(HttpMethod.Get, endpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var result = new List<Models.Salary.SalarySlip>();
        foreach (var item in data.EnumerateArray())
        {
            var slip = JsonSerializer.Deserialize<Models.Salary.SalarySlip>(item.ToString());
            
            // Récupérer les détails complets pour avoir earnings et deductions
            var fullSlip = await GetSalarySlipDetailAsync(slip.Name);
            
            // Appliquer le filtre mois/année si nécessaire
            if (DateTime.TryParse(fullSlip.StartDate, out var startDate))
            {
                if (annee > 0 && startDate.Year != annee)
                {
                    continue;
                }
            }
            result.Add(fullSlip);
        }
        return result;
    }
    
    public async Task<StatistiqueTotal> GetSalaryStatistiqueAsync(int annee = 0)
    {
        var salarySlips = await GetSalarySlipsStatistiqueAsync(annee);
        var result = new List<StatistiqueSalary>();

        var allEarningKeys = new HashSet<string>();
        var allDeductionKeys = new HashSet<string>();

        decimal totalSalaryNet = 0;
        decimal totalDeductions = 0;
        decimal totalSalaryBut = 0;

        var globalEarnings = new Dictionary<string, decimal>();
        var globalDeductions = new Dictionary<string, decimal>();

        // Collecter tous les types d’éléments (Earnings / Deductions)
        foreach (var slip in salarySlips)
        {
            foreach (var earning in slip.Earnings)
                allEarningKeys.Add(earning.SalaryComponentName);
            foreach (var deduction in slip.Deductions)
                allDeductionKeys.Add(deduction.SalaryComponentName);
        }

        // Initialiser les 12 mois avec MonthLabel
        var monthNames = new[]
        {
            "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
            "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"
        };

        for (int i = 0; i < 12; i++)
        {
            result.Add(new StatistiqueSalary
            {
                MonthLabel = monthNames[i],
                TotalMonthEarnings  = allEarningKeys.ToDictionary(k => k, v => 0m),
                TotalMonthDeductions = allDeductionKeys.ToDictionary(k => k, v => 0m),
                TotalNet = 0
            });
        }

        // Initialiser les totaux globaux à zéro
        foreach (var earningKey in allEarningKeys)
        {
            globalEarnings[earningKey] = 0;
        }
        foreach (var deductionKey in allDeductionKeys)
        {
            globalDeductions[deductionKey] = 0;
        }

        // Remplir les totaux mensuels et globaux
        foreach (var slip in salarySlips)
        {
            if (!DateTime.TryParse(slip.StartDate, out var date))
                continue;

            int monthIndex = date.Month - 1;
            var row = result[monthIndex];

            row.TotalNet += slip.NetPay;
            totalSalaryNet += slip.NetPay;

            foreach (var earning in slip.Earnings)
            {
                row.TotalMonthEarnings[earning.SalaryComponentName] += earning.Amount;
                globalEarnings[earning.SalaryComponentName] += earning.Amount;
                totalSalaryBut += earning.Amount;
            }

            foreach (var deduction in slip.Deductions)
            {
                row.TotalMonthDeductions[deduction.SalaryComponentName] += deduction.Amount;
                globalDeductions[deduction.SalaryComponentName] += deduction.Amount;
                totalDeductions += deduction.Amount;
            }
        }

        return new StatistiqueTotal
        {
            Salaries = result,
            TotalGlobalEarnings = globalEarnings,
            TotalGlobalDeductions = globalDeductions,
            TotalNet = totalSalaryNet,
            TotalBut = totalSalaryBut,
            TotalDeduction = totalDeductions
        };
    }





    public async Task<byte[]> CreateProfessionalPdf(Models.Salary.SalarySlip salarySlip)
    {
        using (var memoryStream = new MemoryStream())
        {
            // 1. Initialisation du document PDF
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new iText.Layout.Document(pdf, PageSize.A4);
            document.SetMargins(40, 40, 40, 40);

            // 2. Configuration des styles
            // Polices
            var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontTitle = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            
            // Couleurs
            var primaryColor = new DeviceRgb(41, 128, 185);
            var secondaryColor = new DeviceRgb(127, 140, 141);
            var successColor = new DeviceRgb(39, 174, 96);
            var dangerColor = new DeviceRgb(231, 76, 60);
            var warningColor = new DeviceRgb(241, 196, 15);
            var lightGray = new DeviceRgb(247, 247, 247);
            var darkGray = new DeviceRgb(64, 64, 64);

            // 3. En-tête du document
            // Logo et en-tête
            var headerTable = new Table(new float[] { 30, 70 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // (Vous pouvez ajouter un logo ici si disponible)
            headerTable.AddCell(new Cell()
                .Add(new Paragraph(" ")
                .SetTextAlignment(TextAlignment.LEFT))
                .SetBorder(Border.NO_BORDER));

            headerTable.AddCell(new Cell()
                .Add(new Paragraph("FICHE DE PAIE").SetFont(fontTitle).SetFontSize(18).SetFontColor(primaryColor))
                .Add(new Paragraph(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SetFont(fontNormal).SetFontSize(10).SetFontColor(darkGray))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(Border.NO_BORDER));

            document.Add(headerTable);

            // 4. Section Informations de base
            var infoSection = new Div()
                .SetBorderBottom(new SolidBorder(1))
                .SetPaddingBottom(15)
                .SetMarginBottom(20);

            // Titre section
            infoSection.Add(new Paragraph("INFORMATIONS")
                .SetFont(fontBold)
                .SetFontSize(12)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            // Tableau d'informations
            var infoTable = new Table(new float[] { 33, 33, 34 })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Colonne Employé
            infoTable.AddCell(CreateInfoCell("EMPLOYÉ", fontBold, 10, primaryColor));
            infoTable.AddCell(CreateInfoCell("PÉRIODE", fontBold, 10, primaryColor));
            infoTable.AddCell(CreateInfoCell("MONTANTS", fontBold, 10, primaryColor));

            infoTable.AddCell(CreateInfoCell($"{salarySlip.EmployeeName}\nID: {salarySlip.Employee}\n{salarySlip.Company}", fontNormal, 10));
            infoTable.AddCell(CreateInfoCell($"Du {salarySlip.StartDate}\nAu {salarySlip.EndDate}\nPublié: {salarySlip.PostingDate}", fontNormal, 10));

            var statusColor = salarySlip.Status switch
            {
                "Draft" => warningColor,
                "Submitted" => primaryColor,
                "Paid" => successColor,
                "Cancelled" => dangerColor,
                _ => secondaryColor
            };

            var statusCell = new Cell()
                .Add(new Paragraph($"Statut: ").SetFont(fontNormal).SetFontSize(10))
                .Add(new Paragraph(salarySlip.Status)
                    .SetFont(fontBold)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetBackgroundColor(statusColor)
                    .SetPaddingLeft(5).SetPaddingRight(5))
                .Add(new Paragraph($"\nBrut: {salarySlip.GrossPay.ToString("N2")} {salarySlip.Currency}").SetFont(fontNormal).SetFontSize(10))
                .Add(new Paragraph($"Net: {salarySlip.NetPay.ToString("N2")} {salarySlip.Currency}").SetFont(fontBold).SetFontSize(10))
                .Add(new Paragraph($"Déductions: {(salarySlip.GrossPay - salarySlip.NetPay).ToString("N2")} {salarySlip.Currency}").SetFont(fontNormal).SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5);

            infoTable.AddCell(statusCell);
            infoSection.Add(infoTable);
            document.Add(infoSection);

            // 5. Section Gains et Déductions
            var earningsDeductionsTable = new Table(new float[] { 50, 50 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Tableau des Gains
            var earningsTable = CreateComponentTable("GAINS", salarySlip.Earnings, salarySlip.GrossPay, salarySlip.Currency, fontBold, fontNormal, primaryColor, lightGray);
            earningsDeductionsTable.AddCell(new Cell().Add(earningsTable).SetBorder(Border.NO_BORDER));

            // Tableau des Déductions
            var deductionsTable = CreateComponentTable("DÉDUCTIONS", salarySlip.Deductions, salarySlip.GrossPay - salarySlip.NetPay, salarySlip.Currency, fontBold, fontNormal, primaryColor, lightGray);
            earningsDeductionsTable.AddCell(new Cell().Add(deductionsTable).SetBorder(Border.NO_BORDER));

            document.Add(earningsDeductionsTable);

            // 6. Section Résumé
            var summaryDiv = new Div()
                .SetBackgroundColor(lightGray)
                .SetPadding(15)
                .SetBorderRadius(new BorderRadius(5))
                .SetMarginBottom(10);

            summaryDiv.Add(new Paragraph("RÉSUMÉ")
                .SetFont(fontBold)
                .SetFontSize(12)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            var summaryTable = new Table(new float[] { 70, 30 })
                .SetWidth(UnitValue.CreatePercentValue(50))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            // Ligne Salaire Brut
            summaryTable.AddCell(CreateSummaryCell("Salaire Brut", fontBold, 10, darkGray));
            summaryTable.AddCell(CreateSummaryCell($"{salarySlip.GrossPay.ToString("N2")} {salarySlip.Currency}", fontNormal, 10, darkGray, TextAlignment.RIGHT));

            // Ligne Total Déductions
            summaryTable.AddCell(CreateSummaryCell("Total Déductions", fontBold, 10, darkGray));
            summaryTable.AddCell(CreateSummaryCell($"{(salarySlip.GrossPay - salarySlip.NetPay).ToString("N2")} {salarySlip.Currency}", fontNormal, 10, darkGray, TextAlignment.RIGHT));

            // Ligne Salaire Net
            summaryTable.AddCell(CreateSummaryCell("SALAIRE NET", fontBold, 12, primaryColor));
            summaryTable.AddCell(CreateSummaryCell($"{salarySlip.NetPay.ToString("N2")} {salarySlip.Currency}", fontBold, 12, primaryColor, TextAlignment.RIGHT));

            summaryDiv.Add(summaryTable);
            document.Add(summaryDiv);

            // 7. Pied de page
            var footer = new Div()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(fontNormal)
                .SetFontSize(8)
                .SetFontColor(secondaryColor)
                .SetMarginTop(20)
                .Add(new Paragraph("Document généré électroniquement - valable sans signature"))
                .Add(new Paragraph($"Ref: {salarySlip.Name} - {DateTime.Now.ToString("yyyyMMddHHmmss")}"));

            document.Add(footer);

            document.Close();
            return memoryStream.ToArray();
        }
    }

    // Méthodes utilitaires pour créer des cellules standardisées
    private Cell CreateInfoCell(string text, PdfFont font, float fontSize, DeviceRgb? fontColor = null)
    {
        var cell = new Cell()
            .Add(new Paragraph(text).SetFont(font).SetFontSize(fontSize))
            .SetBorder(Border.NO_BORDER)
            .SetPadding(5);

        if (fontColor != null)
        {
            ((Paragraph)cell.GetChildren()[0]).SetFontColor(fontColor);
        }

        return cell;
    }

    private Cell CreateSummaryCell(string text, PdfFont font, float fontSize, DeviceRgb fontColor, TextAlignment alignment = TextAlignment.LEFT)
    {
        return new Cell()
            .Add(new Paragraph(text).SetFont(font).SetFontSize(fontSize).SetFontColor(fontColor))
            .SetTextAlignment(alignment)
            .SetBorder(Border.NO_BORDER)
            .SetPaddingTop(5)
            .SetPaddingBottom(5);
    }

    private Table CreateComponentTable(string title, List<SalaryComponent> components, decimal total, string currency, PdfFont fontBold, PdfFont fontNormal, DeviceRgb primaryColor, DeviceRgb lightGray)
    {
        var table = new Table(new float[] { 70, 30 })
            .SetWidth(UnitValue.CreatePercentValue(100));

        // Titre
        table.AddCell(new Cell(1, 2)
            .Add(new Paragraph(title).SetFont(fontBold).SetFontSize(11).SetFontColor(primaryColor))
            .SetBorder(Border.NO_BORDER)
            .SetPaddingBottom(5));

        // En-têtes
        table.AddCell(new Cell()
            .Add(new Paragraph("Composant").SetFont(fontBold).SetFontSize(9))
            .SetBorderBottom(new SolidBorder(1))
            .SetPaddingBottom(3));

        table.AddCell(new Cell()
            .Add(new Paragraph("Montant").SetFont(fontBold).SetFontSize(9))
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetBorderBottom(new SolidBorder(1))
            .SetPaddingBottom(3));

        // Lignes de données
        foreach (var component in components)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(component.SalaryComponentName).SetFont(fontNormal).SetFontSize(9))
                .SetPaddingTop(3).SetPaddingBottom(3)
                .SetBorder(Border.NO_BORDER));

            table.AddCell(new Cell()
                .Add(new Paragraph($"{component.Amount.ToString("N2")} {currency}").SetFont(fontNormal).SetFontSize(9))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPaddingTop(3).SetPaddingBottom(3)
                .SetBorder(Border.NO_BORDER));
        }

        // Ligne de total
        table.AddCell(new Cell()
            .Add(new Paragraph("TOTAL").SetFont(fontBold).SetFontSize(10))
            .SetBackgroundColor(lightGray)
            .SetPaddingTop(5).SetPaddingBottom(5));

        table.AddCell(new Cell()
            .Add(new Paragraph($"{total.ToString("N2")} {currency}").SetFont(fontBold).SetFontSize(10))
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetBackgroundColor(lightGray)
            .SetPaddingTop(5).SetPaddingBottom(5));

        return table;
    }
        
}