namespace ERPNextNewApp.Services.Gender;

public interface IGenderService
{
    Task<List<Models.Gender>> GetAllGendersAsync();
}