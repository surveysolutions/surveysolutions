using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Code.Authentication
{
    public interface ITokenProvider
    {
        bool CanGenerate { get; }

        //TokenProviderOptions TokenProviderOptions { get; }
        string GetBearerToken(HqUser user);
    }
}
