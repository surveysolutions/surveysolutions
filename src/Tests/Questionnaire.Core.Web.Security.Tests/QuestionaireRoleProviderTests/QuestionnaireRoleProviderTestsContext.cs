using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;

namespace Questionnaire.Core.Web.Security.Tests.QuestionaireRoleProviderTests
{
    internal class QuestionnaireRoleProviderTestsContext
    {
        public static QuestionnaireRoleProvider CreateProvider()
        {
            return  new QuestionnaireRoleProvider();
        }
    }
}
