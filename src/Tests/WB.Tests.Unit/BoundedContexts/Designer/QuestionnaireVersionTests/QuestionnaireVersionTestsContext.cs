using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    public class QuestionnaireVersionTestsContext
    {
        protected static QuestionnaireVersion CreateQuestionnaireVersion(int major =1, int minor =2, int patch =3 )
        {
            return new QuestionnaireVersion(major, minor, patch);
        }
    }
}