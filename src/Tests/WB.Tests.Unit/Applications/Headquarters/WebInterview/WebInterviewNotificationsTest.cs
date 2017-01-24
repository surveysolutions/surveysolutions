using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    public class WebInterviewNotificationsTest : NUnitTestSpecification
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

        protected override void Context()
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
            var repository = Mock.Of<IStatefulInterviewRepository>(sir => sir.Get(It.IsAny<string>()) == this.interview);

            this.Subject = new WebInterviewNotificationService(
                repository, 
                Create.Fake.HubContextWithGroupMock((name, excludedConnection) => this.NewGroupMock(name).Object));
        }

        protected override void Because()
        {
            this.Subject.RefreshEntities(this.interview.Id, Question1, Question2, PrefilledQuestion, StaticText, RemovedAnswer);
        }

        [Test]
        public void ShouldSendAtLeastOneNotification()
        {
            Assert.That(Groups.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ShouldSendNotificationOnChangeForPrefilledQuestionToClientWithoutSection()
        {
            AssertThatRefreshEntitiesCalledForQuestions(null, PrefilledQuestion);
        }

        [Test]
        public void ShouldSendNotificationOnChangeQuestionsInGroupB()
        {
            AssertThatRefreshEntitiesCalledForQuestions(GroupB, Question1, Question2);
        }

        [Test]
        public void ShouldSendNotificationOnChangeForStaticTextInGroupA()
        {
            AssertThatRefreshEntitiesCalledForQuestions(GroupA, StaticText);
        }

        [Test]
        public void ShouldSendSendInterviewWideNotificationToRefreshSectionForUnexpectedQuestions()
        {
            var mock = Groups[this.interview.Id.FormatGuid()];
            mock.Verify(g => g.refreshSection(), Times.AtLeastOnce);
        }

        private void AssertThatRefreshEntitiesCalledForQuestions(Identity section = null, params Identity[] questions)
        {
            var prefilledKey = UI.Headquarters.API.WebInterview.WebInterview.GetConnectedClientSectionKey(section?.ToString() ?? string.Empty, this.interview.Id.FormatGuid());

            Assert.That(this.Groups, new DictionaryContainsKeyConstraint(prefilledKey));
            var groupMock = this.Groups[prefilledKey];

            groupMock.Verify(g => g.refreshEntities(It.Is<string[]>(
                    ids => AssertCollectionsEqual(ids, questions.Select(q => q.Id.FormatGuid()).ToArray())
                )),
                Times.Once);
        }

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
        /// Specific interface for hub group mock
        /// </summary>
        public interface IHubGroup
        {
            void refreshSection();
            void refreshEntities(string[] questions);
        }
    }
}