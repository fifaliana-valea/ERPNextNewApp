using ERPNextNewApp.Services.Company;
using ERPNextNewApp.Services.Departement;
using ERPNextNewApp.Services.Employees;
using ERPNextNewApp.Services.Gender;
using ERPNextNewApp.Services.Import;
using ERPNextNewApp.Services.Login;
using ERPNextNewApp.Services.SalaryComponent;
using ERPNextNewApp.Services.SalarySlip;
using ERPNextNewApp.Services.SalaryStructure;
using ERPNextNewApp.Services.SalaryStructureAssignment;
using ERPNextNewApp.Services.Utile;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });


builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartementService, DepartementService>();
builder.Services.AddScoped<IGenderService, GenderService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<ISalarySlipService, SalarySlipService>();
builder.Services.AddScoped<IUtileService, UtileService>();
builder.Services.AddScoped<ISalaryComponentService, SalaryComponentService>();
builder.Services.AddScoped<ISalaryStructureService, SalaryStructureService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ISalaryStructureAssignmentService, SalaryStructureAssignmentService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Home/Error";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "erpnext_auth";
    });

builder.Services.AddHttpClient<LoginService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ErpNext:BaseUrl"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

// Middleware de logging simple
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    Console.WriteLine($"Request: {path}");
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
