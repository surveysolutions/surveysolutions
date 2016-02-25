﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class InterviewEventHandlerFunctionalTestContext
    {
        protected static InterviewEventHandlerFunctional CreateInterviewEventHandlerFunctional(QuestionnaireRosterStructure rosterStructure = null, UserDocument user = null)
        {
            var questionnaireRosterStructureMockStorage = new Mock<IQuestionnaireProjectionsRepository>();
            questionnaireRosterStructureMockStorage.Setup(x => x.GetQuestionnaireRosterStructure(It.IsAny<QuestionnaireIdentity>())).Returns(rosterStructure);

            var userDocumentMockStorage = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            userDocumentMockStorage.Setup(x => x.GetById(It.IsAny<string>())).Returns(user);

            return new InterviewEventHandlerFunctional(
                userDocumentMockStorage.Object,
                new Mock<IReadSideKeyValueStorage<InterviewData>>().Object,
                questionnaireRosterStructureMockStorage.Object);
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(Guid scopeId,
            Dictionary<Guid, Guid?> rosterGroupsWithTitleQuestionPairs)
        {
            var rosterStructure = new QuestionnaireRosterStructure();
            var scopeVector = new ValueVector<Guid>(new[] { scopeId });
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed,
                rosterGroupsWithTitleQuestionPairs.ToDictionary(roster => roster.Key,
                    roster => roster.Value.HasValue ? new RosterTitleQuestionDescription(roster.Value.Value) : null));

            rosterStructure.RosterScopes.Add(scopeVector, rosterDescription);
            return rosterStructure;
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(Guid scopeId,
           Dictionary<Guid, RosterTitleQuestionDescription> rosterGroupsWithTitleQuestionPairs)
        {
            var rosterStructure = new QuestionnaireRosterStructure();
            var scopeVector = new ValueVector<Guid>(new [] { scopeId });
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed, rosterGroupsWithTitleQuestionPairs);

            rosterStructure.RosterScopes.Add(scopeVector, rosterDescription);
            return rosterStructure;
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(Guid scopeId, params Guid[] groupIdsFromScope)
        {
            var rosterStructure = new QuestionnaireRosterStructure();
            var scopeVector = new ValueVector<Guid>(new[] { scopeId });
            var rosterGroupsWithTitleQuestionPairs = groupIdsFromScope.ToDictionary<Guid, Guid, RosterTitleQuestionDescription>(groupId => groupId, groupId => null);
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed, rosterGroupsWithTitleQuestionPairs);
            rosterStructure.RosterScopes.Add(scopeVector, rosterDescription);
            return rosterStructure;
        }

        protected static InterviewData CreateViewWithSequenceOfInterviewData()
        {
            var result = new InterviewData();
            result.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            return result;
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(T payload)
            where T: IEvent
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();
            publishableEventMock.Setup(x => x.Payload).Returns(payload);
            return publishableEventMock.Object;
        }

        protected static TextQuestionAnswered CreateTextQuestionAnsweredEvent(Guid questionId, decimal[] propagationVector, string answer)
        {
            return new TextQuestionAnswered(Guid.NewGuid(), questionId, propagationVector, DateTime.Now, answer);
        }

        protected static InterviewQuestion GetQuestion(Guid questionId, InterviewData viewData)
        {
            return viewData.Levels["#"].QuestionsSearchCache.Values.First(q => q.Id == questionId);
        }
    }
}
