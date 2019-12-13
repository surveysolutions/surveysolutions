using System.Security.Claims;
using System.Threading.Tasks;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Designer.Code.Attributes
{
    public interface IBasicAuthenticationService
    {
        Task<ClaimsPrincipal> AuthenticateAsync(BasicCredentials credentials);
    }
}
