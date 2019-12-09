using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.TeamViewFactoryTests
{
    [TestOf(typeof(TeamViewFactory))]
    internal class TeamViewFactory_GetAssigneeSupervisors_Tests : TeamViewFactoryContext
    {
        [Test]
        public void when_get_supervisors_should_return_correct_count()
        {
            var teamLeadId = Guid.NewGuid();
            var teamLeadName = "teamLeadName";
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 7);

            var users = SetupUserRepositoryWithSupervisor(teamLeadId);

            for (int i = 0; i < 25; i++)
            {
                var interviewSummary = Create.Entity.InterviewSummary(teamLeadId: teamLeadId, teamLeadName: teamLeadName);
                StoreInterviewSummary(interviewSummary, questionnaireIdentity);
            }

            var teamViewFactory = CreateTeamViewFactory(users);

            var assigneeSupervisors = teamViewFactory.GetAssigneeSupervisors(10, "");

            Assert.That(assigneeSupervisors.TotalCountByQuery, Is.EqualTo(1));
        }
    }
}
