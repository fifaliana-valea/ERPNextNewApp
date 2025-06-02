namespace ERPNextNewApp.Models.Dto;

public class ImportMessage
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ImportCounts Counts { get; set; }
    public List<string> Errors { get; set; }
}