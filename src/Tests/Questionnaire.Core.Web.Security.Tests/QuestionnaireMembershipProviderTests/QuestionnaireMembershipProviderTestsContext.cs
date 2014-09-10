using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;

namespace Questionnaire.Core.Web.Security.Tests.QuestionnaireMembershipProviderTests
{
    internal class QuestionnaireMembershipProviderTestsContext
    {
        public static QuestionnaireMembershipProvider CreateProvider()
        {
            return new QuestionnaireMembershipProvider();
        }
    }
}
