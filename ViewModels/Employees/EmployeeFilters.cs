namespace ERPNextNewApp.ViewModels.Employees;

public class EmployeeFilters
{
    public string Nom { get; set; }
    public string Departement { get; set; }
    public string Status { get; set; }
    public string Genre { get; set; }
    public DateTime? DateEmbaucheDebut { get; set; }
    public DateTime? DateEmbaucheFin { get; set; }
}