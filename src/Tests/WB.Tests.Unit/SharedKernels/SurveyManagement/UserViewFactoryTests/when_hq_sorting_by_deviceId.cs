using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_hq_sorting_by_deviceId : UserViewFactoryTestContext
    {
        Establish context = () =>
        {
            headquarter1 = CreateUser(headquarter1Id, null, "headquarter1");

            supervisor1 = CreateUser(supervisor1Id, null, "supervisor1");
            var interviewer11 = CreateUser(interviewer11Id, supervisor1Id, "interviewer11", "device11");
            var interviewer12 = CreateUser(interviewer12Id, supervisor1Id, "interviewer12", null);

            supervisor2 = CreateUser(supervisor2Id, null, "supervisor2");
            var interviewer21 = CreateUser(interviewer21Id, supervisor2Id, "interviewer21", "device21");

            supervisor3 = CreateUser(supervisor3Id, null, "supervisor3");
            var interviewer31 = CreateUser(interviewer31Id, supervisor3Id, "interviewer31", "device31", true);
            var interviewer32 = CreateUser(interviewer32Id, supervisor3Id, "interviewer32", null);
            var interviewer33 = CreateUser(interviewer33Id, supervisor3Id, "interviewer33", "device33");
            var interviewer34 = CreateUser(interviewer34Id, supervisor3Id, "interviewer34", null, true);

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


        private static ApplicationUser headquarter1;
        private static ApplicationUser supervisor1;
        private static ApplicationUser supervisor2;
        private static ApplicationUser supervisor3;
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