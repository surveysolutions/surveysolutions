using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_getting_all_supervisors : UserViewFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new []
            {
                Create.Entity.HqUser(Id.g1, userName: superBName, isLockedByHQ: true, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g2, userName: superAName, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g3, userName: superCName, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g4, userName: "inter1", supervisorId: Id.g2),
                Create.Entity.HqUser(Id.g5, userName: "inter2", supervisorId: Id.g2)
            });


            teamFactory = CreateInterviewersViewFactory(readerWithUsers);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = teamFactory.GetAllSupervisors(12, "", showLocked: true);

        [NUnit.Framework.Test] public void should_return_3_supervisors () =>
            result.TotalCountByQuery.Should().Be(3);

        [NUnit.Framework.Test] public void should_return_supervisor_with_specified_properties_at_position_0 () 
        {
            var user = result.Users.ElementAt(0);
            user.UserId.Should().Be(Id.g2);
            user.UserName.Should().Be(superAName);
        }

        [NUnit.Framework.Test] public void should_return_supervisor_with_specified_properties_at_position_1 () 
        {
            var user = result.Users.ElementAt(1);
            user.UserId.Should().Be(Id.g1);
            user.UserName.Should().Be(superBName);
        }

        [NUnit.Framework.Test] public void should_return_supervisor_with_specified_properties_at_position_2 () 
        {
            var user = result.Users.ElementAt(2);
            user.UserId.Should().Be(Id.g3);
            user.UserName.Should().Be(superCName);
        }

        private static IUserViewFactory teamFactory;
        private static UsersView result;
        private static readonly IPlainStorageAccessor<UserView> usersStorage = new TestPlainStorage<UserView>();
        private static readonly string superAName = "a_super1";
        private static readonly string superBName = "b_locked_super";
        private static readonly string superCName = "c_super2";
    }
}
