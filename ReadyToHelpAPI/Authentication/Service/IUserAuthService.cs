using readytohelpapi.Authentication.Models;

namespace readytohelpapi.Authentication.Service;

public interface IUserAuthService
{
    string UserLoginMobile(Models.Authentication authentication);
    string UserLoginWeb(Models.Authentication authentication);
}