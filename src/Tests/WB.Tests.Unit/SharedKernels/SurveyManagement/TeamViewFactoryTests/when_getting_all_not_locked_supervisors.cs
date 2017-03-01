using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamViewFactoryTests
{
    internal class when_getting_all_not_locked_supervisors
    {
        Establish context = () =>
        {
            new List<UserDocument>()
            {
                Create.Entity.UserDocument(Id.g1, userName: "b_locked_super", isLockedByHQ: true),
                Create.Entity.UserDocument(Id.g2, userName: superAName),
                Create.Entity.UserDocument(Id.g3, userName: superCName),
                Create.Entity.UserDocument(Id.g4, userName: "inter1", supervisorId: Id.g2),
                Create.Entity.UserDocument(Id.g5, userName: "inter2", supervisorId: Id.g2)
            }.ForEach(x => usersStorage.Store(x, x.PublicKey.FormatGuid()));
            

            teamFactory = Create.Service.TeamViewFactory(usersReader: usersStorage);
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

        private static TeamViewFactory teamFactory;
        private static UsersView result;
        private static readonly IPlainStorageAccessor<UserDocument> usersStorage = new TestPlainStorage<UserDocument>();
        private static readonly string superAName = "a_super1";
        private static readonly string superCName = "c_super2";
    }
}