using System;
using Main.Core.Entities.Composite;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.NotificationService
{
    public class WebInterviewNotificationServiceTests
    {
        private StatefulInterview interview;
        private Mock<IClientContract> coverGroup;
        private Mock<IClientContract> sectionGroup;
        private Mock<IClientContract> interviewGroup;
        
        private WebInterviewNotificationService Subj { get; set; }

        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.TextQuestion(Id.g1),
                Create.Entity.TextQuestion(Id.g2, variable:"text2", 
                    preFilled: true)
            });

            this.interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            this.coverGroup = new Mock<IClientContract>();
            this.sectionGroup = new Mock<IClientContract>();
            this.interviewGroup = new Mock<IClientContract>();

            var hubMock = new Mock<IHubContext>();
            hubMock.Setup(h => h.Clients).Returns(mockClients.Object);

            coverGroup.Setup(m => m.refreshEntities(It.IsAny<string[]>())).Verifiable();
            sectionGroup.Setup(m => m.refreshEntities(It.IsAny<string[]>())).Verifiable();
            interviewGroup.Setup(m => m.refreshEntities(It.IsAny<string[]>())).Verifiable();

            var prefilledSectionnid = WB.UI.Headquarters.API.WebInterview.WebInterview
                .GetConnectedClientPrefilledSectionKey(interview.Id.FormatGuid());


            mockClients.Setup(m => m.Group(prefilledSectionnid)).Returns(coverGroup.Object);
            mockClients.Setup(m => m.Group(It.Is<string>(s => s != prefilledSectionnid && s != interview.Id.FormatGuid())))
                .Returns(sectionGroup.Object);

            mockClients.Setup(m => m.Group(It.Is<string>(s => s == interview.Id.FormatGuid())))
                .Returns(interviewGroup.Object);

            var repo = Mock.Of<IStatefulInterviewRepository>(s => s.Get(interview.Id.FormatGuid()) == interview);
            
            this.Subj = new WebInterviewNotificationService(repo, null, hubMock.Object);
        }

        public class WebInterviewHubMock<TClientContract> where TClientContract: class
        {
            private Mock<TClientContract> coverGroup;
            public Mock<TClientContract> GetCoverGroup(Guid interviewId) => 
                coverGroup ?? (coverGroup = new Mock<TClientContract>());
        }

        [Test]
        public void should_notify_on_prefilled_question_cover_section()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            this.coverGroup.Verify(g => g.refreshEntities(new[] { Id.Identity2.ToString()}), Times.Once);
        }

        [Test]
        public void should_notify_on_prefilled_question_sections_with_affected_question()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            this.sectionGroup.Verify(g => g.refreshEntities(new[] { Id.Identity1.ToString(), Id.Identity2.ToString() }), Times.Once);
        }

        [Test]
        public void should_notify_clients()
        {
            this.Subj.RefreshEntities(interview.Id, Id.Identity1, Id.Identity2);

            this.interviewGroup.Verify(g => g.refreshSection(), Times.Once);
        }


        public interface IClientContract
        {
            void refreshEntities(string[] identities);
            void refreshSection();
        }
    }
}