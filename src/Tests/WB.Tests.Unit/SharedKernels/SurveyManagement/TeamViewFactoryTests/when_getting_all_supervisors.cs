using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamViewFactoryTests
{
    internal class when_getting_all_supervisors
    {
        Establish context = () =>
        {
            new List<UserDocument>()
            {
                Create.UserDocument(Id.g1, userName: superBName, isLockedByHQ: true),
                Create.UserDocument(Id.g2, userName: superAName),
                Create.UserDocument(Id.g3, userName: superCName),
                Create.UserDocument(Id.g4, userName: "inter1", supervisorId: Id.g2),
                Create.UserDocument(Id.g5, userName: "inter2", supervisorId: Id.g2)
            }.ForEach(x => usersStorage.Store(x, x.PublicKey.FormatGuid()));


            teamFactory = Create.TeamViewFactory(usersReader: usersStorage);
        };

        Because of = () =>
            result = teamFactory.GetAllSupervisors(12, "", showLocked: true);

        It should_return_3_supervisors = () =>
            result.TotalCountByQuery.ShouldEqual(3);

        It should_return_supervisor_with_specified_properties_at_position_0 = () =>
        {
            var user = result.Users.ElementAt(0);
            user.UserId.ShouldEqual(Id.g2);
            user.UserName.ShouldEqual(superAName);
        };

        It should_return_supervisor_with_specified_properties_at_position_1 = () =>
        {
            var user = result.Users.ElementAt(1);
            user.UserId.ShouldEqual(Id.g1);
            user.UserName.ShouldEqual(superBName);
        };

        It should_return_supervisor_with_specified_properties_at_position_2 = () =>
        {
            var user = result.Users.ElementAt(2);
            user.UserId.ShouldEqual(Id.g3);
            user.UserName.ShouldEqual(superCName);
        };

        private static TeamViewFactory teamFactory;
        private static UsersView result;
        private static readonly TestInMemoryWriter<UserDocument> usersStorage = new TestInMemoryWriter<UserDocument>();
        private static readonly string superAName = "a_super1";
        private static readonly string superBName = "b_locked_super";
        private static readonly string superCName = "c_super2";
    }
}
