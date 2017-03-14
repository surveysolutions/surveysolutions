using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_need_sort_by_interviewers : UserViewFactoryTestContext
    {
        Establish context = () =>
        {
            supervisor1 = Create.Entity.ApplicationUser(supervisor1Id, null, userName: "supervisor1", role: UserRoles.Supervisor);
            var interviewer11 = Create.Entity.ApplicationUser(interviewer11Id, supervisor1Id, userName: "interviewer11", deviceId: "device11");
            var interviewer12 = Create.Entity.ApplicationUser(interviewer12Id, supervisor1Id, userName: "interviewer12");

            supervisor2 = Create.Entity.ApplicationUser(supervisor2Id, null, userName:"supervisor2", role: UserRoles.Supervisor);
            var interviewer21 = Create.Entity.ApplicationUser(interviewer21Id, supervisor2Id, userName:"interviewer21", deviceId: "device21");

            supervisor3 = Create.Entity.ApplicationUser(supervisor3Id, null, userName:"supervisor3", role: UserRoles.Supervisor);
            var interviewer31 = Create.Entity.ApplicationUser(interviewer31Id, supervisor3Id, userName:"interviewer31", deviceId: "device31");
            var interviewer32 = Create.Entity.ApplicationUser(interviewer32Id, supervisor3Id, userName:"interviewer32");
            var interviewer33 = Create.Entity.ApplicationUser(interviewer33Id, supervisor3Id, userName:"interviewer33");
            var interviewer34 = Create.Entity.ApplicationUser(interviewer34Id, supervisor3Id, userName:"interviewer34", isArchived: true);

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                supervisor1, supervisor2, supervisor3,
                interviewer11, interviewer12,
                interviewer21,
                interviewer31, interviewer32, interviewer33, interviewer34);

            supervisorsViewFactory = CreateInterviewersViewFactory(readerWithUsers);
        };

        Because of = () =>
            result = supervisorsViewFactory.GetSupervisors(0, 20, "NotConnectedToDeviceInterviewersCount", null, false);

        It should_return_3_supervisors = () =>
        {
            result.TotalCount.ShouldEqual(3);
            result.Items.Count().ShouldEqual(3);
        };

        It should_return_supervisors_in_correct_order = () =>
        {
            result.Items.Skip(0).First().UserId.ShouldEqual(supervisor2Id);
            result.Items.Skip(1).First().UserId.ShouldEqual(supervisor1Id);
            result.Items.Skip(2).First().UserId.ShouldEqual(supervisor3Id);
        };

        It should_return_correct_count_of_not_connected_interviewers = () =>
        {
            result.Items.Skip(0).First().NotConnectedToDeviceInterviewersCount.ShouldEqual(0);
            result.Items.Skip(1).First().NotConnectedToDeviceInterviewersCount.ShouldEqual(1);
            result.Items.Skip(2).First().NotConnectedToDeviceInterviewersCount.ShouldEqual(2);
        };

        It should_return_correct_count_of_interviewers = () =>
        {
            result.Items.Skip(0).First().InterviewersCount.ShouldEqual(1);
            result.Items.Skip(1).First().InterviewersCount.ShouldEqual(2);
            result.Items.Skip(2).First().InterviewersCount.ShouldEqual(3);
        };


        private static HqUser supervisor1;
        private static HqUser supervisor2;
        private static HqUser supervisor3;
        private static SupervisorsView result;
        private static IUserViewFactory supervisorsViewFactory;
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