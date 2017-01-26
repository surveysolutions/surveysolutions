using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    public class When_WebInterviewNotifications_Receive_Refresh: TestSpecification
    {
        private static readonly Identity GroupA = Create.Entity.Identity(101);
        private static readonly Identity GroupB = Create.Entity.Identity(202);
        private static readonly Identity StaticText = Create.Entity.Identity(333);
        private static readonly Identity Question1 = Create.Entity.Identity(111);
        private static readonly Identity Question2 = Create.Entity.Identity(222);
        private static readonly Identity PrefilledQuestion = Create.Entity.Identity(444);
        private static readonly Identity RemovedAnswer = Create.Entity.Identity(555);
        
        private StatefulInterview interview;
        private WebInterviewNotificationService Subject;

        protected override void Establish()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Group(GroupA.Id, children: new List<IComposite>
                {
                    Create.Entity.StaticText(StaticText.Id),
                    Create.Entity.Group(GroupB.Id, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(Question1.Id),
                        Create.Entity.TextQuestion(Question2.Id),
                        Create.Entity.TextQuestion(PrefilledQuestion.Id, preFilled: true),
                    })
                }));

            this.interview = Setup.StatefulInterview(questionnaire);

            this.Subject = new WebInterviewNotificationService(
                Setup.StatefulInterviewRepository(this.interview), 
                Create.Fake.HubContextWithGroupMock((name, excludedConnection) => this.NewGroupMock(name).Object));
        }

        protected override void Because()
        {
            this.Subject.RefreshEntities(this.interview.Id, Question1, Question2, PrefilledQuestion, StaticText, RemovedAnswer);
        }

        [Test]
        public void Should_send_AtLeastOne_Notification()
        {
            Assert.That(Groups.Count, Is.GreaterThan(0));
        }

        [Test]
        public void should_send_NotificationOnChange_forPrefilledQuestion_ToClient_WithoutSection()
        {
            this.AssertThatCalledRefreshEntitiesForQuestionsPerSpecifiedSection(null, PrefilledQuestion);
        }

        [Test]
        public void Should_Send_NotificationOnChange_Questions_InGroupB()
        {
            this.AssertThatCalledRefreshEntitiesForQuestionsPerSpecifiedSection(GroupB, Question1, Question2);
        }

        [Test]
        public void Should_Send_Notification_OnChangeForStaticText_InGroupA()
        {
            this.AssertThatCalledRefreshEntitiesForQuestionsPerSpecifiedSection(GroupA, StaticText);
        }

        [Test]
        public void Should_Send_Send_InterviewWideNotification_ToRefreshSection_ForUnexpectedQuestions()
        {
            var mock = Groups[this.interview.Id.FormatGuid()];
            mock.Verify(g => g.refreshSection(), Times.AtLeastOnce);
        }

        private void AssertThatCalledRefreshEntitiesForQuestionsPerSpecifiedSection(Identity section, params Identity[] questions)
        {
            var prefilledKey = UI.Headquarters.API.WebInterview.WebInterview.GetConnectedClientSectionKey(section?.ToString(), this.interview.Id.FormatGuid());

            Assert.That(this.Groups, new DictionaryContainsKeyConstraint(prefilledKey));
            var groupMock = this.Groups[prefilledKey];

            var questionsThatShouldBeCalled = questions.Select(q => q.Id.FormatGuid()).ToArray();

            groupMock.Verify(g => g.refreshEntities(It.Is<string[]>(
                    ids => AssertCollectionsEqual(ids, questionsThatShouldBeCalled))
                ),
                Times.Once);
        }

        // asert that source collection has all items from target collection
        private bool AssertCollectionsEqual<T>(IEnumerable<T> source, IEnumerable<T> target)
        {
            return !source.Except(target).Any();
        }

        private Dictionary<string, Mock<IHubGroup>> Groups { get; set; } = new Dictionary<string, Mock<IHubGroup>>();

        private Mock<IHubGroup> NewGroupMock(string name)
        {
            var group = new Mock<IHubGroup>();
            Groups.Add(name, group);
            return group;
        }

        /// <summary>
        /// Specific for this test onlyinternal interface for hub group mock
        /// Cannot be marked as internal, due to Moq restrictions
        /// </summary>
        public interface IHubGroup
        {
            void refreshSection();
            void refreshEntities(string[] questions);
        }
    }
}