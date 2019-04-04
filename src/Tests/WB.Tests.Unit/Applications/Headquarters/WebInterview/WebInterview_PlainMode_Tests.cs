﻿using System;
using System.Dynamic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
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
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview
{
    [TestOf(typeof(WebInterviewHub))]
    public class WebInterview_PlainMode_Tests
    {
        [Test]
        public void GetSectionEntities_for_entities_with_cascading_question_that_should_be_shown_as_combobox()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, answerCodes: new decimal[] { 1, 2 }, variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g3, cascadeFromQuestionId: Id.g2, answerCodes: new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, parentCodes: new decimal[] { 1, 1, 2, 2, 2, 2, 2, 2, 2 }, variable: "q2", showAsListThreshold: 5));
            
            var options = new[]
            {
                Create.Entity.CategoricalQuestionOption(3, title: "3", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(4, title: "4", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(5, title: "5", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(6, title: "6", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(7, title: "7", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(8, title: "8", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(9, title: "9", parentValue: 2)
            };
            var optionsRepository = Mock.Of<IQuestionOptionsRepository>(x => x.GetOptionsForQuestion(It.IsAny<IQuestionnaire>(), Id.g3, 2, null, null) == options);

            var interview = Create.AggregateRoot.StatefulInterview(userId: Id.gA, questionnaire: questionnaire, optionsRepository: optionsRepository);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g2, RosterVector.Empty, DateTime.UtcNow, 2);

            var webInterview = CreateWebInterview(interview, interview.ServiceLocatorInstance.GetInstance<IQuestionnaireStorage>());
            
            //act
            var entities = webInterview.GetSectionEntities(Id.g1.FormatGuid());

            //assert

            Assert.That(entities.Length, Is.EqualTo(3));
            Assert.That(entities[0].EntityType, Is.EqualTo(InterviewEntityType.CategoricalSingle.ToString()));
            Assert.That(entities[1].EntityType, Is.EqualTo(InterviewEntityType.Combobox.ToString()));
        }

        [Test]
        public void GetSectionEntities_for_entities_with_cascading_question_that_should_be_shown_as_list()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, answerCodes: new decimal[] { 1, 2 }, variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g3, cascadeFromQuestionId: Id.g2, answerCodes: new decimal[] { 1, 2, 3, 4 }, parentCodes: new decimal[] { 1, 1, 2, 2 }, variable: "q2", showAsListThreshold: 3));

            var options = new[]
            {
                Create.Entity.CategoricalQuestionOption(3, title: "3", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(4, title: "4", parentValue: 2)
            };
            var optionsRepository = Mock.Of<IQuestionOptionsRepository>(x => x.GetOptionsForQuestion(It.IsAny<IQuestionnaire>(), Id.g3, 2, null, null) == options);

            var interview = Create.AggregateRoot.StatefulInterview(userId: Id.gA, questionnaire: questionnaire, optionsRepository: optionsRepository);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g2, RosterVector.Empty, DateTime.UtcNow, 2);

            var webInterview = CreateWebInterview(interview, interview.ServiceLocatorInstance.GetInstance<IQuestionnaireStorage>());
            
            //act
            var entities = webInterview.GetSectionEntities(Id.g1.FormatGuid());

            //assert

            Assert.That(entities.Length, Is.EqualTo(3));
            Assert.That(entities[0].EntityType, Is.EqualTo(InterviewEntityType.CategoricalSingle.ToString()));
            Assert.That(entities[1].EntityType, Is.EqualTo(InterviewEntityType.CategoricalSingle.ToString()));
        }

        [Test]
        public void GetSectionEntities_for_entities_with_comments_placed_in_plain_roster_should_return_correct_parent_link()
        {
            var sectionId = Guid.NewGuid();
            var rosterId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId, isFlatMode: true, fixedTitles: new FixedRosterTitle[]
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

            Assert.That(entities.Length, Is.EqualTo(5));
            Assert.That(entities[0].EntityType, Is.EqualTo(InterviewEntityType.GroupTitle.ToString()));
            Assert.That(entities[1].EntityType, Is.EqualTo(InterviewEntityType.Integer.ToString()));
            Assert.That(entities[2].EntityType, Is.EqualTo(InterviewEntityType.GroupTitle.ToString()));
            Assert.That(entities[3].EntityType, Is.EqualTo(InterviewEntityType.Integer.ToString()));
            Assert.That(entities[0].Identity, Is.EqualTo(Create.Identity(rosterId, Create.RosterVector(1)).ToString()));
            Assert.That(entities[1].Identity, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(1)).ToString()));
            Assert.That(entities[2].Identity, Is.EqualTo(Create.Identity(rosterId, Create.RosterVector(2)).ToString()));
            Assert.That(entities[3].Identity, Is.EqualTo(Create.Identity(intQuestionId, Create.RosterVector(2)).ToString()));
        }

        [Test]
        public void GetNavigationButtonState_for_group_inside_plain_roster_shoud_return_to_parent_of_plain_roster()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.FixedRoster(isFlatMode: true, fixedTitles: new FixedRosterTitle[]
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
                    Create.Entity.FixedRoster(isFlatMode: true, fixedTitles: new FixedRosterTitle[]
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

        private WebInterviewHub CreateWebInterview(IStatefulInterview statefulInterview, IQuestionnaireStorage questionnaire, string sectionId = null)
        {
            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(statefulInterview);
            var questionnaireStorage = questionnaire;
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory();

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
