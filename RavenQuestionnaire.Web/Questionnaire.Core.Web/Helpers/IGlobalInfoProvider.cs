using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Helpers
{
    public interface IGlobalInfoProvider
    {
        UserLight GetCurrentUser();
    }
}
