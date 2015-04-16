using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    public class QuestionnaireVersionTestsContext
    {
        protected static EngineVersion CreateQuestionnaireVersion(int major =1, int minor =2, int patch =3 )
        {
            return new EngineVersion(major, minor, patch);
        }
    }
}