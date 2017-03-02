using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_searching_for_supervisor : UserViewFactoryTestContext
    {
        Establish context = () =>
        {
            var supervisor = CreateUser(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), null, "Supervisor1");
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(supervisor);

            supervisorsViewFactory = CreateInterviewersViewFactory(readerWithUsers);
        };

        Because of = () => searchResult = supervisorsViewFactory.GetSupervisors(0, 20, null, "sup", false);

        It should_use_case_insensative_search = () => searchResult.Items.Count().ShouldEqual(1);

        static IUserViewFactory supervisorsViewFactory;
        static SupervisorsView searchResult;
    }
}