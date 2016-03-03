using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewersViewFactoryTests
{
    internal class when_supervisor_filtering : InterviewersViewFactoryTestContext
    {
        Establish context = () =>
        {
            supervisor1 = CreateSupervisor(supervisor1Id, "supervisor1");
            UserDocument interviewer11 = CreateInterviewer(interviewer11Id, supervisor1, "interviewer11", "device11");
            UserDocument interviewer12 = CreateInterviewer(interviewer12Id, supervisor1, "interviewer12", null);

            supervisor2 = CreateSupervisor(supervisor2Id, "supervisor2");
            UserDocument interviewer21 = CreateInterviewer(interviewer21Id, supervisor2, "interviewer21", "device21");

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                supervisor1, supervisor2, 
                interviewer11, interviewer12,
                interviewer21);

            interviewersViewFactory = CreateInterviewersViewFactory(readerWithUsers);

            interviewersInputModel = new InterviewersInputModel()
            {
                ViewerId = supervisor1Id,
                SearchBy = "interviewer",
                Page = 0,
                PageSize = 20
            };
        };

        Because of = () =>
            result = interviewersViewFactory.Load(interviewersInputModel);

        It should_return_2_interviewers = () =>
        {
            result.TotalCount.ShouldEqual(2);
            result.Items.Count().ShouldEqual(2);
        };

        It should_return_only_own_interviewers_in_correct_order = () =>
        {
            result.Items.Skip(0).First().UserName.ShouldEqual("interviewer11");
            result.Items.Skip(1).First().UserName.ShouldEqual("interviewer12");
        };

        private static UserDocument supervisor1;
        private static UserDocument supervisor2;
        private static InterviewersInputModel interviewersInputModel;
        private static InterviewersView result;
        private static IInterviewersViewFactory interviewersViewFactory;
        private static Guid supervisor1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid supervisor2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewer11Id = Guid.Parse("01111111111111111111111111111111");
        private static Guid interviewer12Id = Guid.Parse("01222222222222222222222222222222");
        private static Guid interviewer21Id = Guid.Parse("02111111111111111111111111111111");
    }
}