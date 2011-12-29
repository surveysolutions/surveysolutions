using RavenQuestionnaire.Core;

namespace Questionnaire.Core.Web.Helpers
{
    public interface IBagManager
    {
        void AddUsersToBag(dynamic bag, IViewRepository viewRepository);
    }
}
