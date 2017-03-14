using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IHashCompatibilityProvider
    {
        bool IsSHA1Required(HqUser user);
        string GetSHA1HashFor(HqUser user, string password);
    }
}