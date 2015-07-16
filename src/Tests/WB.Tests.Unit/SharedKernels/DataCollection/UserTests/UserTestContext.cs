using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    [Subject(typeof(User))]
    class UserTestContext
    {
        protected static User CreateUser()
        {
            return new User();
        }

        protected static void SetupInstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Setup.InstanceToMockedServiceLocator(instance);
        }
    }
}
