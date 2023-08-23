using System;
using System.Linq;
using AutoMapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.API.WebInterview.Services.Overview;
using WB.UI.Headquarters.Services.Impl;

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

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g9,
                Create.Entity.TextQuestion(hiddenQuestionIdentity.Id, scope: QuestionScope.Hidden),
                Create.Entity.TextQuestion(identifyingIdentity.Id, scope: QuestionScope.Headquarter),
                Create.Entity.TextQuestion(supervisorQuestionIdentity.Id, scope: QuestionScope.Supervisor),
                Create.Entity.TextQuestion(interviewerQuestionIdentity.Id, scope: QuestionScope.Interviewer));
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var statefullInterview = SetUp.StatefulInterview(questionnaireDocument);

            var service = CreateInterviewOverviewService();

            // act
            var overviewNodes = service.GetOverview(statefullInterview, plainQuestionnaire, true).ToArray();
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

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(hiddenQuestionIdentity.Id, scope: QuestionScope.Hidden),
                Create.Entity.TextQuestion(identifyingIdentity.Id, scope: QuestionScope.Headquarter),
                Create.Entity.TextQuestion(supervisorQuestionIdentity.Id, scope: QuestionScope.Supervisor),
                Create.Entity.TextQuestion(interviewerQuestionIdentity.Id, scope: QuestionScope.Interviewer));
            var statefullInterview = SetUp.StatefulInterview(questionnaireDocument);
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var service = CreateInterviewOverviewService();

            // act
            var overviewNodes = service.GetOverview(statefullInterview, plainQuestionnaire, false).ToArray();
            // assert
            Assert.That(overviewNodes.Length, Is.EqualTo(1));
            Assert.That(overviewNodes[0].Id, Is.EqualTo(interviewerQuestionIdentity.ToString()));
        }

        [Test]
        [SetCulture("en-US")]
        public void when_creating_gps_overview_node_with_answer()
        {
            // arrange
            var identity = Identity.Create(Id.g1, RosterVector.Empty);

            var statefulInterview = SetUp.StatefulInterview(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.GpsCoordinateQuestion(identity.Id)));

            statefulInterview.AnswerGeoLocationQuestion(Id.gA, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, 11.11, 22.22, 5, 6, DateTimeOffset.Now);

            // act
            var overviewNode = new OverviewWebQuestionNode(statefulInterview.GetQuestion(identity), statefulInterview);

            // assert
            Assert.That(overviewNode.Answer, Is.EqualTo(@"{ ""latitude"": 11.11, ""longitude"": 22.22 }"));
            Assert.That(overviewNode.ControlType, Is.EqualTo("map"));
        }

        [Test]
        public void when_creating_multimedia_overview_node_with_answer()
        {
            // arrange
            var identity = Identity.Create(Id.g1, RosterVector.Empty);

            var statefulInterview = SetUp.StatefulInterview(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.MultimediaQuestion(identity.Id)));

            statefulInterview.AnswerPictureQuestion(Id.gA, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "file.name");

            // act
            var overviewNode = new OverviewWebQuestionNode(statefulInterview.GetQuestion(identity), statefulInterview);

            // assert
            Assert.That(overviewNode.Answer, Is.EqualTo($@"?interviewId={statefulInterview.Id}&questionId=11111111111111111111111111111111&filename=file.name"));
            Assert.That(overviewNode.ControlType, Is.EqualTo("image"));
        }

        [Test]
        public void when_creating_display_plain_roster_on_overview()
        {
            // arrange
            var sectionId     = Guid.Parse("11111111111111111111111111111111");
            var groupId       = Guid.Parse("22222222222222222222222222222222");
            var intQuestionId = Guid.Parse("33333333333333333333333333333333");
            var rosterId      = Guid.Parse("44444444444444444444444444444444");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId, displayMode: RosterDisplayMode.Flat, fixedTitles: new FixedRosterTitle[]
                    {
                        Create.Entity.FixedTitle(1, "1"),
                        Create.Entity.FixedTitle(2, "2"),
                    },
                    children: new IComposite[]
                    {
                        Create.Entity.Group(groupId, children:new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(intQuestionId)
                        })
                    })
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var statefullInterview = SetUp.StatefulInterview(questionnaireDocument);

            var service = CreateInterviewOverviewService();

            // act
            var overviewNodes = service.GetOverview(statefullInterview, plainQuestionnaire, true).ToArray();

            // assert
            Assert.That(overviewNodes.Length, Is.EqualTo(7));
            Assert.That(overviewNodes[0].Id, Is.EqualTo(Create.Identity(sectionId).ToString()));
            Assert.That(overviewNodes[1].Id, Is.EqualTo(Create.Identity(rosterId, 1).ToString()));
            Assert.That(overviewNodes[2].Id, Is.EqualTo(Create.Identity(groupId, 1).ToString()));
            Assert.That(overviewNodes[3].Id, Is.EqualTo(Create.Identity(intQuestionId, 1).ToString()));
            Assert.That(overviewNodes[4].Id, Is.EqualTo(Create.Identity(rosterId, 2).ToString()));
            Assert.That(overviewNodes[5].Id, Is.EqualTo(Create.Identity(groupId, 2).ToString()));
            Assert.That(overviewNodes[6].Id, Is.EqualTo(Create.Identity(intQuestionId, 2).ToString()));
        }

        private static InterviewOverviewService CreateInterviewOverviewService()
        {
            var webInterviewInterviewEntityFactory = Web.Create.Service.WebInterviewInterviewEntityFactory();
            return new InterviewOverviewService(webInterviewInterviewEntityFactory,
                Web.Create.Service.WebNavigationService());
        }
    }
}
