using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_hq_sorting_by_deviceId : UserViewFactoryTestContext
    {
        Establish context = () =>
        {
            headquarter1 = Create.Entity.HqUser(headquarter1Id, userName:"headquarter1", role:UserRoles.Headquarter);

            supervisor1 = Create.Entity.HqUser(supervisor1Id, userName:"supervisor1", role: UserRoles.Supervisor);
            var interviewer11 = Create.Entity.HqUser(interviewer11Id, supervisor1Id, userName: "interviewer11", deviceId: "device11");
            var interviewer12 = Create.Entity.HqUser(interviewer12Id, supervisor1Id, userName: "interviewer12");

            supervisor2 = Create.Entity.HqUser(supervisor2Id, null, userName: "supervisor2", role: UserRoles.Supervisor);
            var interviewer21 = Create.Entity.HqUser(interviewer21Id, supervisor2Id, userName: "interviewer21", deviceId: "device21");

            supervisor3 = Create.Entity.HqUser(supervisor3Id, null, userName: "supervisor3", role: UserRoles.Supervisor);
            var interviewer31 = Create.Entity.HqUser(interviewer31Id, supervisor3Id, userName: "interviewer31", isArchived: true, deviceId: "device31");
            var interviewer32 = Create.Entity.HqUser(interviewer32Id, supervisor3Id, userName: "interviewer32");
            var interviewer33 = Create.Entity.HqUser(interviewer33Id, supervisor3Id, userName: "interviewer33", deviceId: "device33");
            var interviewer34 = Create.Entity.HqUser(interviewer34Id, supervisor3Id, userName: "interviewer34", isArchived: true);

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                headquarter1,
                supervisor1, supervisor2, supervisor3,
                interviewer11, interviewer12,
                interviewer21,
                interviewer31, interviewer32, interviewer33, interviewer34);

            interviewersViewFactory = CreateInterviewersViewFactory(readerWithUsers);
        };

        Because of = () =>
            result = interviewersViewFactory.GetInterviewers(0, 20, "DeviceId", null, false, true, null);

        It should_return_3_interviewers = () =>
        {
            result.TotalCount.ShouldEqual(3);
            result.Items.Count().ShouldEqual(3);
        };

        It should_return_all_interviewers_in_correct_order = () =>
        {
            result.Items.Skip(0).First().DeviceId.ShouldEqual("device11");
            result.Items.Skip(1).First().DeviceId.ShouldEqual("device21");
            result.Items.Skip(2).First().DeviceId.ShouldEqual("device33");
        };


        private static HqUser headquarter1;
        private static HqUser supervisor1;
        private static HqUser supervisor2;
        private static HqUser supervisor3;
        private static InterviewersView result;
        private static IUserViewFactory interviewersViewFactory;
        private static Guid headquarter1Id = Guid.Parse("77777777777777777777777777777777");
        private static Guid supervisor1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid supervisor2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid supervisor3Id = Guid.Parse("33333333333333333333333333333333");
        private static Guid interviewer11Id = Guid.Parse("01111111111111111111111111111111");
        private static Guid interviewer12Id = Guid.Parse("01222222222222222222222222222222");
        private static Guid interviewer21Id = Guid.Parse("02111111111111111111111111111111");
        private static Guid interviewer31Id = Guid.Parse("03111111111111111111111111111111");
        private static Guid interviewer32Id = Guid.Parse("03222222222222222222222222222222");
        private static Guid interviewer33Id = Guid.Parse("03333333333333333333333333333333");
        private static Guid interviewer34Id = Guid.Parse("03444444444444444444444444444444");
    }
}