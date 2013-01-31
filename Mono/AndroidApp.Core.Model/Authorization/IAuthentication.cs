using Main.Core.Entities.SubEntities;

namespace AndroidApp.Core.Model.Authorization
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        bool IsLoggedIn { get; }
        bool LogOn(string userName, string password);
        void LogOff();
    }
}