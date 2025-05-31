using ERPNextNewApp.Models.Request;
using ERPNextNewApp.Models.Response;

namespace ERPNextNewApp.Services.Login;

public interface ILoginService
{
    Task<AuthResponse?> LoginAsync(AuthRequest authRequest);
    
    Task<HttpResponseMessage> MakeAuthenticatedRequest(
        HttpMethod method, 
        string endpoint, 
        HttpContent? content = null);
}