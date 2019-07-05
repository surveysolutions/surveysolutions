using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_searching_for_non_locked_users : UserViewFactoryTestContext
    {
        [Test]
        public void should_not_show_locked_by_supervisor_accounts()
        {
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "b_locked_super", isLockedByHQ: true, role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g2, userName: "b_locked_by_hq_in", isLockedByHQ: true, role: UserRoles.Interviewer),
                Create.Entity.HqUser(Id.g3, userName: "b_locked_by_sv_in", lockedBySupervisor: true, role: UserRoles.Interviewer),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            ResponsibleView responsibles = teamFactory.GetAllResponsibles(20, null, false, false);

            Assert.That(responsibles.Users, Is.Empty);
        }

        [Test]
        public void should_show_start_with_user_firstly()
        {
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "super_for_inter", role: UserRoles.Supervisor),
                Create.Entity.HqUser(Id.g2, userName: "inter", role: UserRoles.Interviewer),
                Create.Entity.HqUser(Id.g3, userName: "interviewer", role: UserRoles.Interviewer),
            });

            var teamFactory = CreateInterviewersViewFactory(readerWithUsers);

            var queryResult = teamFactory.GetAllResponsibles(20, "inter", false, false);

            var responsibles = queryResult.Users.ToList();
            Assert.That(queryResult.TotalCountByQuery, Is.EqualTo(3));
            Assert.That(responsibles, Is.Not.Empty);
            Assert.That(responsibles.Count, Is.EqualTo(3));
            Assert.That(responsibles[0].ResponsibleId, Is.EqualTo(Id.g2));
            Assert.That(responsibles[1].ResponsibleId, Is.EqualTo(Id.g3));
            Assert.That(responsibles[2].ResponsibleId, Is.EqualTo(Id.g1));
        }
    }
}
