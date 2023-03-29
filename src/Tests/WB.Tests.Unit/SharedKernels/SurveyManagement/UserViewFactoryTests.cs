using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    [TestOf(typeof(UserViewFactory))]
    internal class UserViewFactoryTest : UserViewFactoryTestContext
    {
        [Test]
        public void when_GetAllResponsibles_should_return_interviewer_supervisor_and_headquarters()
        {
            // arrange
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "int", role: UserRoles.Interviewer),
                Create.Entity.HqUser(Id.g2, userName: "super", role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g3, userName: "head", role: UserRoles.Headquarter),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            // act
            var responsibles = teamFactory.GetAllResponsibles(20, "");

            // assert
            Assert.That(responsibles.Users.Select(x => x.UserName), Is.EquivalentTo(new[] {"int", "super", "head"}));
        }

        [Test]
        public void when_GetInterviewers_should_return_all_interviewers_by_supervisor()
        {
            // arrange
            var supervisorId = Id.g1;
            var otherSupervisor = Id.g2;

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "int", role: UserRoles.Interviewer, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "archivedint", role: UserRoles.Interviewer, isArchived: true, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "lockedint", role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "intindiffteam", role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: otherSupervisor),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            // act
            var responsibles = teamFactory.GetInterviewers(supervisorId);

            // assert
            Assert.That(responsibles, Has.Exactly(3).Items);
            Assert.That(responsibles.Select(x => x.UserName),
                Is.EquivalentTo(new[] {"int", "archivedint", "lockedint"}));
            Assert.That(responsibles.Single(x=>x.UserName == "archivedint").IsLockedBySupervisor, Is.True);
            Assert.That(responsibles.Single(x=>x.UserName == "lockedint").IsLockedBySupervisor, Is.True);
        }
        
        [Test]
        public void when_GetTeamResponsibles_with_equals_rule_should_return_one_interviewer_with_requested_name()
        {
            // arrange
            var supervisorId = Id.g1;

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "intsv", role: UserRoles.Interviewer, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "svint", role: UserRoles.Interviewer, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "int",    role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "asvinta",  role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: supervisorId),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            // act
            var usersView = teamFactory.GetTeamResponsibles(10, "int", supervisorId, true, false, filterRule: QueryFilterRule.Equals);
            var responsibles = usersView.Users.ToList();

            // assert
            Assert.That(responsibles, Has.Exactly(1).Items);
            Assert.That(responsibles.Single().UserName, Is.EqualTo("int"));
        }

        [Test]
        public void when_GetTeamResponsibles_with_contains_rule_should_return_all_interviewer_with_include_text_in_name()
        {
            // arrange
            var supervisorId = Id.g1;

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "intsv", role: UserRoles.Interviewer, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "svint", role: UserRoles.Interviewer, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "asvina", role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: supervisorId),
                Create.Entity.HqUser(Id.g1, userName: "int",   role: UserRoles.Interviewer, lockedBySupervisor: true, supervisorId: supervisorId),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            // act
            var usersView = teamFactory.GetTeamResponsibles(10, "int", supervisorId, true, false, filterRule: QueryFilterRule.Contains);
            var responsibles = usersView.Users.ToList();

            // assert
            Assert.That(responsibles, Has.Exactly(3).Items);
            Assert.That(responsibles.Select(x => x.UserName),
                Is.EquivalentTo(new[] {"intsv", "svint", "int"}));
        }
    }
}
