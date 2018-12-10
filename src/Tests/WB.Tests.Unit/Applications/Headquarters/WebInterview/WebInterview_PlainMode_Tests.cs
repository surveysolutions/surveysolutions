using System;
using System.Dynamic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(WebInterviewHub))]
    public class WebInterview_PlainMode_Tests
    {
        [Test]
        public void GetSectionEntities_for_entities_with_comments_placed_in_plain_roster_shoud_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(isPlainMode: true, fixedTitles: new FixedRosterTitle[]
                {
                    Create.Entity.FixedTitle(1, "1"),
                    Create.Entity.FixedTitle(2, "2"),
                }, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(intQuestionId)
                })
            });
            var statefulInterview = Setup.StatefulInterview(questionnaireDocument);
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

            var entities = webInterview.GetSectionEntities(sectionId.FormatGuid());

            Assert.That(entities.Length, Is.EqualTo(3));
            Assert.That(entities.First().EntityType, Is.EqualTo(InterviewEntityType.Integer.ToString()));
            Assert.That(entities.Second().EntityType, Is.EqualTo(InterviewEntityType.Integer.ToString()));
            Assert.That(entities.First().Identity, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(1)).ToString()));
            Assert.That(entities.Second().Identity, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(2)).ToString()));
        }

        [Test]
        public void GetNavigationButtonState_for_group_inside_plain_roster_shoud_return_to_parent_of_plain_roster()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(isPlainMode: true, fixedTitles: new FixedRosterTitle[]
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
            var statefulInterview = Setup.StatefulInterview(questionnaireDocument);
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument, Create.Identity(groupId, Create.RosterVector(1)).ToString());

            var navigationButtonState = webInterview.GetNavigationButtonState(Create.Identity(groupId, Create.RosterVector(1)).ToString(), plainQuestionnaire);

            Assert.That(navigationButtonState.Type, Is.EqualTo(ButtonType.Parent));
            Assert.That(navigationButtonState.Target, Is.EqualTo(Create.Identity(sectionId, Create.RosterVector()).ToString()));
        }


        [Test]
        public void GetCoverInfo_for_entities_with_comments_placed_in_plain_roster_shoud_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.Group(groupId, children: new IComposite[]
                {
                    Create.Entity.FixedRoster(isPlainMode: true, fixedTitles: new FixedRosterTitle[]
                    {
                        Create.Entity.FixedTitle(1, "1"),
                        Create.Entity.FixedTitle(2, "2"),
                    }, children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(intQuestionId)
                    })
                })
            });
            var statefulInterview = Setup.StatefulInterview(questionnaireDocument);
            statefulInterview.CommentAnswer(currentUserId, intQuestionId, Create.RosterVector(1), DateTimeOffset.UtcNow, "comment");
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

            var coverInfo = webInterview.GetCoverInfo();

            Assert.That(coverInfo.EntitiesWithComments.Length, Is.EqualTo(1));
            Assert.That(coverInfo.EntitiesWithComments.Single().Id, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(1)).ToString()));
            Assert.That(coverInfo.EntitiesWithComments.Single().ParentId, Is.EqualTo(groupId.FormatGuid()));
        }

        private WebInterviewHub CreateWebInterview(IStatefulInterview statefulInterview, 
            QuestionnaireDocument questionnaireDocument,
            string sectionId = null)
        {
            var statefulInterviewRepository = Setup.StatefulInterviewRepository(statefulInterview);
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory();

            var webInterviewHub = new WebInterviewHub(statefulInterviewRepository, 
                Mock.Of<ICommandService>(),
                questionnaireStorage,
                Mock.Of<IWebInterviewNotificationService>(),
                webInterviewInterviewEntityFactory,
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IInterviewBrokenPackagesService>(),
                Mock.Of<IAudioFileStorage>(),
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IChangeStatusFactory>(),
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IStatefullInterviewSearcher>(),
                Mock.Of<IInterviewOverviewService>());

            webInterviewHub.Context = Mock.Of<HubCallerContext>(h =>
                h.QueryString == Mock.Of<INameValueCollection>(p => 
                    p["interviewId"] == statefulInterview.Id.FormatGuid()
                )
            );

            if (!string.IsNullOrEmpty(sectionId))
            {
                dynamic mockCaller = new ExpandoObject();
                mockCaller.sectionId = sectionId;
                var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
                mockClients.Setup(m => m.Caller).Returns((ExpandoObject)mockCaller);
                webInterviewHub.Clients = mockClients.Object;
            }

            return webInterviewHub;
        }
    }
}
