using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Code.Authentication
{
    public interface ITokenProvider
    {
        bool CanGenerate { get; }

        Task<string> GetOrCreateBearerTokenAsync(HqUser user);
        
        Task InvalidateBearerTokenAsync(HqUser user);
        
        Task<bool> DoesTokenExist(HqUser user);
        
        Task<bool> ValidateJtiAsync(HqUser user, string jti);
    }
}
