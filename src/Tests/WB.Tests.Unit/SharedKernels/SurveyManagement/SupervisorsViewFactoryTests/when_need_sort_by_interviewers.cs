using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Supervisor;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorsViewFactoryTests
{
    internal class when_need_sort_by_interviewers : SupervisorsViewFactoryTestContext
    {
        Establish context = () =>
        {
            supervisor1 = CreateSupervisor(supervisor1Id, "supervisor1");
            UserDocument interviewer11 = CreateInterviewer(interviewer11Id, supervisor1, "interviewer11", "device11");
            UserDocument interviewer12 = CreateInterviewer(interviewer12Id, supervisor1, "interviewer12", null);

            supervisor2 = CreateSupervisor(supervisor2Id, "supervisor2");
            UserDocument interviewer21 = CreateInterviewer(interviewer21Id, supervisor2, "interviewer21", "device21");

            supervisor3 = CreateSupervisor(supervisor3Id, "supervisor3");
            UserDocument interviewer31 = CreateInterviewer(interviewer31Id, supervisor3, "interviewer31", "device31");
            UserDocument interviewer32 = CreateInterviewer(interviewer32Id, supervisor3, "interviewer32", null);
            UserDocument interviewer33 = CreateInterviewer(interviewer33Id, supervisor3, "interviewer33", null);
            UserDocument interviewer34 = CreateInterviewer(interviewer34Id, supervisor3, "interviewer34", null, true);

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                supervisor1, supervisor2, supervisor3,
                interviewer11, interviewer12,
                interviewer21,
                interviewer31, interviewer32, interviewer33, interviewer34);

            supervisorsViewFactory = CreateSupervisorsViewFactory(readerWithUsers);

            supervisorsInputModel = new SupervisorsInputModel()
            {
                Order = "NotConnectedToDeviceInterviewersCount",
                Page = 0,
                PageSize = 20
            };
        };

        Because of = () =>
            result = supervisorsViewFactory.Load(supervisorsInputModel);

        It should_return_3_supervisors = () =>
        {
            result.TotalCount.ShouldEqual(3);
            result.Items.Count().ShouldEqual(3);
        };

        It should_return_supervisors_in_correct_order = () =>
        {
            result.Items.Skip(0).First().UserId.ShouldEqual(supervisor2.PublicKey);
            result.Items.Skip(1).First().UserId.ShouldEqual(supervisor1.PublicKey);
            result.Items.Skip(2).First().UserId.ShouldEqual(supervisor3.PublicKey);
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


        private static UserDocument supervisor1;
        private static UserDocument supervisor2;
        private static UserDocument supervisor3;
        private static SupervisorsInputModel supervisorsInputModel;
        private static SupervisorsView result;
        private static ISupervisorsViewFactory supervisorsViewFactory;
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