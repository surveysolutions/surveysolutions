using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_getting_supervisors_across_workspaces : UserViewFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var mapsSupervisor = Create.Entity.HqUser(Id.g2, userName: "ms1", role: UserRoles.Supervisor, workspaces: new[] { "maps" });
            mapsSupervisor.FullName = "Maps Supervisor";

            teamFactory = CreateInterviewersViewFactory(CreateQueryableReadSideRepositoryReaderWithUsers(
                Create.Entity.HqUser(Id.g1, userName: "primary-supervisor", role: UserRoles.Supervisor, workspaces: new[] { "primary" }),
                mapsSupervisor,
                Create.Entity.HqUser(Id.g3, userName: "maps-interviewer", supervisorId: Id.g2, workspaces: new[] { "maps" })));

            result = teamFactory.GetUsersByRole(1, 10, null, "maps supervisor", false, UserRoles.Supervisor, acrossAllWorkspaces: true);
        }

        [NUnit.Framework.Test]
        public void should_return_supervisor_from_another_workspace()
        {
            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle(x => x.UserId == Id.g2 && x.UserName == "ms1");
        }

        private static IUserViewFactory teamFactory = null!;
        private static UserListView result = null!;
    }
}
