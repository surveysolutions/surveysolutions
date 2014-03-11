using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    [Subject(typeof(Survey))]
    internal class SurveyTestsContext
    {
        protected static Survey CreateSurvey()
        {
            return new Survey();
        }
    }
}