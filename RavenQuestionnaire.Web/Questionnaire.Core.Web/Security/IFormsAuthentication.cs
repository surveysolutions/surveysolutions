using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Security
{
    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool rememberMe);
        void SignOut();
        string GetUserIdForCurrentUser();
        UserLight GetCurrentUser();
    }
}
