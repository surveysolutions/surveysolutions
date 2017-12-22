using System;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;

using static WB.UI.Headquarters.API.WebInterview.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.NotificationService
{
    public class WebInterviewNotificationServiceTests
    {
        private StatefulInterview interview;
        private Mock<IWebInterviewInvoker> hubMock;
        private string prefilledSectionId;
        private WebInterviewNotificationService Subj { get; set; }

        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.Group(Id.Identity3.Id, children: new IComposite[] {
                    Create.Entity.TextQuestion(Id.Identity1.Id),
                    Create.Entity.TextQuestion(Id.Identity2.Id, variable:"text2",
                    preFilled: true)})
            });

            this.interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            this.hubMock = new Mock<IWebInterviewInvoker>();

            this.prefilledSectionId = GetConnectedClientPrefilledSectionKey(interview.Id);

            var repo = Mock.Of<IStatefulInterviewRepository>(s => s.Get(interview.Id.FormatGuid()) == interview);

            this.Subj = new WebInterviewNotificationService(repo, null, hubMock.Object);
        }

        public class WebInterviewHubMock<TClientContract> where TClientContract : class
        {
            private Mock<TClientContract> coverGroup;
            public Mock<TClientContract> GetCoverGroup(Guid interviewId) =>
                coverGroup ?? (coverGroup = new Mock<TClientContract>());
        }

        [Test]
        public void should_notify_on_prefilled_question_cover_section()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            this.hubMock.Verify(v => v.RefreshEntities(prefilledSectionId, new[] { Id.Identity2.ToString() }), Times.Once);
        }

        [Test]
        public void should_notify_on_prefilled_question_sections_with_affected_question()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            var sectionKey = GetConnectedClientSectionKey(Id.Identity3, interview.Id);

            this.hubMock.Verify(g => g.RefreshEntities(
                sectionKey,
                new[] { Id.Identity1.ToString(), Id.Identity2.ToString() }
            ), Times.Once);
        }

        [Test]
        public void should_notify_client_section_state()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            this.hubMock.Verify(g => g.RefreshSection(interview.Id), Times.Once);
        }
    }
}