using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IHashCompatibilityProvider
    {
        bool IsInSha1CompatibilityMode();
        string GetSHA1HashFor(HqUser user, string password);
    }
}