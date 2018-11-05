using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(InterviewOverviewService))]
    internal class InterviewOverviewServiceTests
    {
        [Test]
        public void when_GetOverview_in_review_mode_should_getting_answers_by_all_scopes_of_questions()
        {
            // arrange
            var hiddenQuestionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
            var identifyingIdentity =    Identity.Create(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);
            var supervisorQuestionIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);
            var interviewerQuestionIdentity = Identity.Create(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var statefullInterview = Setup.StatefulInterview(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g9,
                    Create.Entity.TextQuestion(hiddenQuestionIdentity.Id, scope: QuestionScope.Hidden),
                    Create.Entity.TextQuestion(identifyingIdentity.Id, scope: QuestionScope.Headquarter),
                    Create.Entity.TextQuestion(supervisorQuestionIdentity.Id, scope: QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(interviewerQuestionIdentity.Id, scope: QuestionScope.Interviewer)));

            var service = CreateInterviewOverviewService();

            // act
            var overviewNodes = service.GetOverview(statefullInterview, true).ToArray();
            // assert
            Assert.That(overviewNodes.Length, Is.EqualTo(5));
            Assert.That(overviewNodes[0].Id, Is.EqualTo(Id.g9.FormatGuid()));
            Assert.That(overviewNodes[1].Id, Is.EqualTo(hiddenQuestionIdentity.ToString()));
            Assert.That(overviewNodes[2].Id, Is.EqualTo(identifyingIdentity.ToString()));
            Assert.That(overviewNodes[3].Id, Is.EqualTo(supervisorQuestionIdentity.ToString()));
            Assert.That(overviewNodes[4].Id, Is.EqualTo(interviewerQuestionIdentity.ToString()));
        }

        [Test]
        public void when_GetOverview_and_not_review_mode_should_getting_answers_by_interviewer_questions_only()
        {
            // arrange
            var hiddenQuestionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
            var identifyingIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);
            var supervisorQuestionIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);
            var interviewerQuestionIdentity = Identity.Create(Guid.Parse("44444444444444444444444444444444"), RosterVector.Empty);

            var statefullInterview = Setup.StatefulInterview(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(hiddenQuestionIdentity.Id, scope: QuestionScope.Hidden),
                    Create.Entity.TextQuestion(identifyingIdentity.Id, scope: QuestionScope.Headquarter),
                    Create.Entity.TextQuestion(supervisorQuestionIdentity.Id, scope: QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(interviewerQuestionIdentity.Id, scope: QuestionScope.Interviewer)));

            var service = CreateInterviewOverviewService();

            // act
            var overviewNodes = service.GetOverview(statefullInterview, false).ToArray();
            // assert
            Assert.That(overviewNodes.Length, Is.EqualTo(1));
            Assert.That(overviewNodes[0].Id, Is.EqualTo(interviewerQuestionIdentity.ToString()));
        }

        private static InterviewOverviewService CreateInterviewOverviewService() => new InterviewOverviewService();
    }
}
