using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;



namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_supervisor_filtering : UserViewFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            supervisor1 = Create.Entity.HqUser(supervisor1Id, userName:"supervisor1", role: UserRoles.Supervisor);
            var interviewer11 = Create.Entity.HqUser(interviewer11Id, supervisor1Id, userName: "interviewer11");
            var interviewer12 = Create.Entity.HqUser(interviewer12Id, supervisor1Id, userName: "interviewer12");

            supervisor2 = Create.Entity.HqUser(supervisor2Id, null, userName: "supervisor2", role: UserRoles.Supervisor);
            var interviewer21 = Create.Entity.HqUser(interviewer21Id, supervisor2Id, userName: "interviewer21");

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                supervisor1, supervisor2, 
                interviewer11, interviewer12,
                interviewer21)
                .WithDeviceInfo();

            interviewersViewFactory = CreateInterviewersViewFactory(readerWithUsers);
            BecauseOf();
        }

        public void BecauseOf() => result = interviewersViewFactory.GetInterviewers(0, 20, null, "interviewer", false, null, supervisor1Id);

        [NUnit.Framework.Test] public void should_return_2_interviewers () 
        {
            result.TotalCount.Should().Be(2);
            result.Items.Count().Should().Be(2);
        }

        [NUnit.Framework.Test] public void should_return_only_own_interviewers_in_correct_order () 
        {
            result.Items.Skip(0).First().UserName.Should().Be("interviewer11");
            result.Items.Skip(1).First().UserName.Should().Be("interviewer12");
        }

        [NUnit.Framework.Test] public void should_return_correct_is_archived_status () 
        {
            result.Items.Skip(0).First().IsArchived.Should().Be(false);
            result.Items.Skip(1).First().IsArchived.Should().Be(false);
        }


        private static HqUser supervisor1;
        private static HqUser supervisor2;
        private static InterviewersView result;
        private static IUserViewFactory interviewersViewFactory;
        private static Guid supervisor1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid supervisor2Id = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewer11Id = Guid.Parse("01111111111111111111111111111111");
        private static Guid interviewer12Id = Guid.Parse("01222222222222222222222222222222");
        private static Guid interviewer21Id = Guid.Parse("02111111111111111111111111111111");
    }
}
