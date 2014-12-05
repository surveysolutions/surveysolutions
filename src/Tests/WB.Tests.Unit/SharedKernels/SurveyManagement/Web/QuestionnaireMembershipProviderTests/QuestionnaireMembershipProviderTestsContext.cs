using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.QuestionnaireMembershipProviderTests
{
    internal class QuestionnaireMembershipProviderTestsContext
    {
        public static QuestionnaireMembershipProvider CreateProvider()
        {
            return new QuestionnaireMembershipProvider();
        }
    }
}
