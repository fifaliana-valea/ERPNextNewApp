namespace ERPNextNewApp.Models.Dto;

public class EmployeeDto
{
    public string Id { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Gender { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime BirthDate { get; set; }
    public string Company { get; set; }
}
