using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    public class QuestionnaireVersionTestsContext
    {
        protected static ExpressionsEngineVersion CreateQuestionnaireVersion(int major =1, int minor =2, int patch =3 )
        {
            return new ExpressionsEngineVersion(major, minor, patch);
        }
    }
}