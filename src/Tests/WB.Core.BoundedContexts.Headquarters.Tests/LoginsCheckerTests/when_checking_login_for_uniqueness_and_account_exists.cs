using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.LoginsCheckerTests
{
    internal class when_checking_login_for_uniqueness_and_account_exists : LoginsCheckerTestContext
    {
        Establish context = () =>
        {
            var supervisorLoginView = new SupervisorLoginView();
            var supervisorLogins = Mock.Of<IQueryableReadSideRepositoryReader<SupervisorLoginView>>(x => x.GetById(notUniqueLogin) == supervisorLoginView);

            checker = CreateLoginsChecker(supervisorLogins);
        };

        Because of = () => 
            result = checker.IsUnique(notUniqueLogin);

        It should_return_false_as_result = () =>
            result.ShouldBeFalse();

        private static LoginsChecker checker;
        private static string notUniqueLogin = "Vasya";
        private static bool result;
    }
}