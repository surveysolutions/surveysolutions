using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_searching_for_supervisor : UserViewFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var supervisor = Create.Entity.HqUser(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), userName: "Supervisor1", role: UserRoles.Supervisor);
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(supervisor);

            supervisorsViewFactory = CreateInterviewersViewFactory(readerWithUsers);
            BecauseOf();
        }

        public void BecauseOf() => searchResult = supervisorsViewFactory.GetSupervisors(0, 20, null, "sup", false);

        [NUnit.Framework.Test] public void should_use_case_insensative_search () => searchResult.Items.Count().Should().Be(1);

        static UserViewFactory supervisorsViewFactory;
        static SupervisorsView searchResult;
    }
}
