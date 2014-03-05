using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVersionTests
{
    public class QuestionnaireVersionTestsContext
    {
        protected static QuestionnaireVersion CreateQuestionnaireVersion(int major =1, int minor =2, int patch =3 )
        {
            return new QuestionnaireVersion(major, minor, patch);
        }
    }
}