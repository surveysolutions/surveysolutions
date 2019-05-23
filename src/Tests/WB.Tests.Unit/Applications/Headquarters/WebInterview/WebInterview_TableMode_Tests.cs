using System;
using System.Dynamic;
using System.Linq;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(WebInterviewHub))]
    public class WebInterview_TableMode_Tests
    {
        [Test]
        public void GetSectionEntities_for_entities_with_comments_placed_in_table_roster_should_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var rosterId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId, displayMode: RosterDisplayMode.Table, fixedTitles: new FixedRosterTitle[]
                {
                    Create.Entity.FixedTitle(1, "1"),
                    Create.Entity.FixedTitle(2, "2"),
                }, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(intQuestionId)
                })
            });
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

            var entities = webInterview.GetSectionEntities(sectionId.FormatGuid());

            Assert.That(entities.Length, Is.EqualTo(2));
            Assert.That(entities[0].EntityType, Is.EqualTo(InterviewEntityType.TableRoster.ToString()));
            Assert.That(entities[0].Identity, Is.EqualTo(rosterId.FormatGuid()));
            Assert.That(entities[1].EntityType, Is.EqualTo(InterviewEntityType.NavigationButton.ToString()));
        }      
               
        [Test]
        public void GetSectionEntities_for_entities_with_supervisor_questions_in_table_roster_should_return_correct_questions_set()
        {
            var sectionId = Guid.NewGuid();
            var rosterId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId, displayMode: RosterDisplayMode.Table, fixedTitles: new FixedRosterTitle[]
                {
                    Create.Entity.FixedTitle(1, "1"),
                    Create.Entity.FixedTitle(2, "2"),
                }, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(intQuestionId),
                    Create.Entity.TextQuestion(scope: QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(scope: QuestionScope.Hidden),
                    Create.Entity.TextQuestion(scope: QuestionScope.Headquarter),
                })
            });
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument);

            var entities = webInterview.GetSectionEntities(sectionId.FormatGuid());

            Assert.That(entities.Length, Is.EqualTo(2));
            Assert.That(entities[0].EntityType, Is.EqualTo(InterviewEntityType.TableRoster.ToString()));
            Assert.That(entities[0].Identity, Is.EqualTo(rosterId.FormatGuid()));
            Assert.That(entities[1].EntityType, Is.EqualTo(InterviewEntityType.NavigationButton.ToString()));
        }      
        
        [Test]
        public void GetEntitiesDetails_for_entities_section_with__table_roster_should_return_correct_entities()
        {
            var sectionId = Guid.NewGuid();
            var rosterId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId, displayMode: RosterDisplayMode.Table, fixedTitles: new FixedRosterTitle[]
                {
                    Create.Entity.FixedTitle(1, "1"),
                    Create.Entity.FixedTitle(2, "2"),
                }, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(intQuestionId)
                })
            });
            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument));
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var mapper = SetupMapper();
            var webInterview = CreateWebInterview(statefulInterview, questionnaireStorage, mapper: mapper);
            var ids = Identity.Create(rosterId, RosterVector.Empty).ToString().ToEnumerable().ToArray();

            var entities = webInterview.GetEntitiesDetails(ids);

            Assert.That(entities.Length, Is.EqualTo(3));
            Assert.That(entities[0].Id, Is.EqualTo(rosterId.FormatGuid()));
            Assert.That(entities[1].Id, Is.EqualTo(Identity.Create(intQuestionId, Create.RosterVector(1)).ToString()));
            Assert.That(entities[2].Id, Is.EqualTo(Identity.Create(intQuestionId, Create.RosterVector(2)).ToString()));
        }

        [Test]
        public void GetNavigationButtonState_for_group_inside_table_roster_should_return_to_parent_of_plain_roster()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Table, fixedTitles: new FixedRosterTitle[]
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
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var webInterview = CreateWebInterview(statefulInterview, questionnaireDocument, Create.Identity(groupId, Create.RosterVector(1)).ToString());

            var navigationButtonState = webInterview.GetNavigationButtonState(Create.Identity(groupId, Create.RosterVector(1)).ToString(), plainQuestionnaire);

            Assert.That(navigationButtonState.Type, Is.EqualTo(ButtonType.Parent));
            Assert.That(navigationButtonState.Target, Is.EqualTo(Create.Identity(sectionId, Create.RosterVector()).ToString()));
        }


        [Test]
        public void GetCoverInfo_for_entities_with_comments_placed_in_table_roster_should_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.Group(groupId, children: new IComposite[]
                {
                    Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Table, fixedTitles: new FixedRosterTitle[]
                    {
                        Create.Entity.FixedTitle(1, "1"),
                        Create.Entity.FixedTitle(2, "2"),
                    }, children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(intQuestionId)
                    })
                })
            });
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
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
            return CreateWebInterview(statefulInterview,  SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument)), sectionId);
        }      
        
        private IMapper SetupMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            }).CreateMapper();
        }

        private WebInterviewHub CreateWebInterview(IStatefulInterview statefulInterview, IQuestionnaireStorage questionnaire, string sectionId = null, IMapper mapper = null)
        {
            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(statefulInterview);
            var questionnaireStorage = questionnaire;
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory(autoMapper: mapper);

            var serviceLocator = Mock.Of<IServiceLocator>(sl =>
                sl.GetInstance<IStatefulInterviewRepository>() == statefulInterviewRepository
                && sl.GetInstance<IQuestionnaireStorage>() == questionnaireStorage
                && sl.GetInstance<IWebInterviewInterviewEntityFactory>() == webInterviewInterviewEntityFactory
                && sl.GetInstance<IAuthorizedUser>() == Mock.Of<IAuthorizedUser>());

            var webInterviewHub = new WebInterviewHub();
            webInterviewHub.SetServiceLocator(serviceLocator);

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
