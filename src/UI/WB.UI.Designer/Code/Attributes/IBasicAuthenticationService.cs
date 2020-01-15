using System.Security.Claims;
using System.Threading.Tasks;

namespace WB.UI.Designer.Code.Attributes
{
    public interface IBasicAuthenticationService
    {
        Task<ClaimsPrincipal> AuthenticateAsync(BasicCredentials credentials);
    }
}
