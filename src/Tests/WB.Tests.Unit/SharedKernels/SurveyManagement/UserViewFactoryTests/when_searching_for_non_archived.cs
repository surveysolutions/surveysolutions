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
    }
}