using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;

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

        protected static ApplicationPasswordPolicySettings CreateApplicationPasswordPolicySettings(int? minPasswordLength = null, string passwordPattern = null)
        {
            return new ApplicationPasswordPolicySettings
            {
                MinPasswordLength = minPasswordLength ?? 5,
                PasswordPattern = passwordPattern ?? "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$"
            };
        }
    }
}