using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.Supervisor;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewersViewFactoryTests
{
    internal class when_hq_filtering_by_archived : InterviewersViewFactoryTestContext
    {
        Establish context = () =>
        {
            headquarter1 = CreateHeadquarter(headquarter1Id, "headquarter1");

            supervisor1 = CreateSupervisor(supervisor1Id, "supervisor1");
            UserDocument interviewer11 = CreateInterviewer(interviewer11Id, supervisor1, "interviewer11", "device11");
            UserDocument interviewer12 = CreateInterviewer(interviewer12Id, supervisor1, "interviewer12", null);

            supervisor2 = CreateSupervisor(supervisor2Id, "supervisor2");
            UserDocument interviewer21 = CreateInterviewer(interviewer21Id, supervisor2, "interviewer21", "device21", true);

            supervisor3 = CreateSupervisor(supervisor3Id, "supervisor3");
            UserDocument interviewer31 = CreateInterviewer(interviewer31Id, supervisor3, "interviewer31", "device31", true);
            UserDocument interviewer32 = CreateInterviewer(interviewer32Id, supervisor3, "interviewer32", null);
            UserDocument interviewer33 = CreateInterviewer(interviewer33Id, supervisor3, "interviewer33", "device33");
            UserDocument interviewer34 = CreateInterviewer(interviewer34Id, supervisor3, "interviewer34", null, true);

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                headquarter1,
                supervisor1, supervisor2, supervisor3,
                interviewer11, interviewer12,
                interviewer21,
                interviewer31, interviewer32, interviewer33, interviewer34);

            interviewersViewFactory = CreateInterviewersViewFactory(readerWithUsers);

            interviewersInputModel = new InterviewersInputModel()
            {
                ViewerId = headquarter1Id,
                Archived = true,
                Page = 0,
                PageSize = 20
            };
        };

        Because of = () =>
            result = interviewersViewFactory.Load(interviewersInputModel);

        It should_return_3_interviewers = () =>
        {
            result.TotalCount.ShouldEqual(3);
            result.Items.Count().ShouldEqual(3);
        };

        It should_return_all_interviewers_in_correct_order = () =>
        {
            result.Items.Skip(0).First().UserId.ShouldEqual(interviewer21Id);
            result.Items.Skip(1).First().UserId.ShouldEqual(interviewer31Id);
            result.Items.Skip(2).First().UserId.ShouldEqual(interviewer34Id);
        };

        It should_return_supervisorname_for_each_interviewers = () =>
        {
            result.Items.Skip(0).First().SupervisorName.ShouldEqual("supervisor2");
            result.Items.Skip(1).First().SupervisorName.ShouldEqual("supervisor3");
            result.Items.Skip(2).First().SupervisorName.ShouldEqual("supervisor3");
        };


        private static UserDocument headquarter1;
        private static UserDocument supervisor1;
        private static UserDocument supervisor2;
        private static UserDocument supervisor3;
        private static InterviewersInputModel interviewersInputModel;
        private static InterviewersView result;
        private static IInterviewersViewFactory interviewersViewFactory;
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