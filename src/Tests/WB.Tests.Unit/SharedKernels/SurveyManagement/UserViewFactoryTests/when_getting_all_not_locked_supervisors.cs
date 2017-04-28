using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_getting_all_not_locked_supervisors : UserViewFactoryTestContext
    {
        Establish context = () =>
        {
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new []
            {
                Create.Entity.HqUser(Id.g1, userName: "b_locked_super", isLockedByHQ: true, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g2, userName: superAName, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g3, userName: superCName, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g4, userName: "inter1", supervisorId: Id.g2),
                Create.Entity.HqUser(Id.g5, userName: "inter2", supervisorId: Id.g2)
            });
            

            teamFactory = CreateInterviewersViewFactory(readerWithUsers);
        };

        Because of = () =>
            result = teamFactory.GetAllSupervisors(12, "", showLocked : false);

        It should_return_2_active_supervisors = () =>
            result.TotalCountByQuery.ShouldEqual(2);

        It should_return_supervisor_with_specified_properties_at_position_0 = () =>
        {
            var user = result.Users.ElementAt(0);
            user.UserId.ShouldEqual(Id.g2);
            user.UserName.ShouldEqual(superAName);
        };

        It should_return_supervisor_with_specified_properties_at_position_1 = () =>
        {
            var user = result.Users.ElementAt(1);
            user.UserId.ShouldEqual(Id.g3);
            user.UserName.ShouldEqual(superCName);
        };

        private static IUserViewFactory teamFactory;
        private static UsersView result;
        private static readonly IPlainStorageAccessor<UserView> usersStorage = new TestPlainStorage<UserView>();
        private static readonly string superAName = "a_super1";
        private static readonly string superCName = "c_super2";
    }
}