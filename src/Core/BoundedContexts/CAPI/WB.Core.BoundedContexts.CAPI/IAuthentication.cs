using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Capi
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        bool IsLoggedIn { get; }
        bool LogOn(string userName, string password);
        void LogOff();
    }
}