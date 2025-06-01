namespace ERPNextNewApp.Models.Dto;

public class SalaryStructureDto
{
    public string StructureCode { get; set; }
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public string Type { get; set; } // earning/deduction
    public string Value { get; set; } // could be %, number, etc.
    public string Remark { get; set; }
}
