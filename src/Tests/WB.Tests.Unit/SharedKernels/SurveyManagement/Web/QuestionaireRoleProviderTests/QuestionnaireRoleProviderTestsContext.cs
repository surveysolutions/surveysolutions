using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.QuestionaireRoleProviderTests
{
    internal class QuestionnaireRoleProviderTestsContext
    {
        public static QuestionnaireRoleProvider CreateProvider()
        {
            return  new QuestionnaireRoleProvider();
        }
    }
}
