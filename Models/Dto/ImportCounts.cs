using System.Text.Json.Serialization;

namespace ERPNextNewApp.Models.Dto;

public class ImportCounts
{
    public int Employees { get; set; }
    public int Structures { get; set; }
    public int Slips { get; set; }
}