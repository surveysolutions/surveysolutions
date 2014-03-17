using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
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

        protected static void SetupInstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }
    }
}