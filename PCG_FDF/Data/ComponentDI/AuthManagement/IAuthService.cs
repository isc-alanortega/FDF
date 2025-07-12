using PCG_ENTITIES.PCG.Session;

namespace PCG_FDF.Data.ComponentDI.AuthManagement
{
    public interface IAuthService
    {
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
        Task<RegisterResult> Register(RegisterModel registerModel);
    }
}
