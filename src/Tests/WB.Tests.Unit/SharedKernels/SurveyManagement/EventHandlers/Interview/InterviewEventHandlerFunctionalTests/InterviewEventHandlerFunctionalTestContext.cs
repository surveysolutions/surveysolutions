using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class InterviewEventHandlerFunctionalTestContext
    {
        protected static InterviewEventHandlerFunctional CreateInterviewEventHandlerFunctional(Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes = null, UserDocument user = null)
        {

            var scopes = rosterScopes ?? new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
            var questionnaireMock = new Mock<IQuestionnaire>();

            var rostrerStructureService = new Mock<IRostrerStructureService>();
            rostrerStructureService.Setup(x => x.GetRosterScopes(Moq.It.IsAny<QuestionnaireDocument>())).Returns(scopes);
            
            var questionnaireRosterStructureMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireRosterStructureMockStorage.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>())).Returns(questionnaireMock.Object);

            var userDocumentMockStorage = new Mock<IPlainStorageAccessor<UserDocument>>();
            userDocumentMockStorage.Setup(x => x.GetById(It.IsAny<string>())).Returns(user);

            return new InterviewEventHandlerFunctional(
                userDocumentMockStorage.Object,
                new Mock<IReadSideKeyValueStorage<InterviewData>>().Object,
                questionnaireRosterStructureMockStorage.Object, 
                rostrerStructureService.Object);
        }

        protected static Dictionary<ValueVector<Guid>, RosterScopeDescription> CreateQuestionnaireRosterScopes(Guid scopeId,
            Dictionary<Guid, Guid?> rosterGroupsWithTitleQuestionPairs)
        {
            var rosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
            var scopeVector = new ValueVector<Guid>(new[] { scopeId });
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed,
                rosterGroupsWithTitleQuestionPairs.ToDictionary(roster => roster.Key,
                    roster => roster.Value.HasValue ? new RosterTitleQuestionDescription(roster.Value.Value) : null));

            rosterScopes.Add(scopeVector, rosterDescription);
            return rosterScopes;
        }

        protected static Dictionary<ValueVector<Guid>, RosterScopeDescription> CreateQuestionnaireRosterScopes(Guid scopeId,
           Dictionary<Guid, RosterTitleQuestionDescription> rosterGroupsWithTitleQuestionPairs)
        {
            var rosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
            var scopeVector = new ValueVector<Guid>(new [] { scopeId });
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed, rosterGroupsWithTitleQuestionPairs);

            rosterScopes.Add(scopeVector, rosterDescription);
            return rosterScopes;
        }

        protected static Dictionary<ValueVector<Guid>, RosterScopeDescription> CreateQuestionnaireRosterScopes(Guid scopeId, params Guid[] groupIdsFromScope)
        {
            var rosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
            var scopeVector = new ValueVector<Guid>(new[] { scopeId });
            var rosterGroupsWithTitleQuestionPairs = groupIdsFromScope.ToDictionary<Guid, Guid, RosterTitleQuestionDescription>(groupId => groupId, groupId => null);
            var rosterDescription = new RosterScopeDescription(scopeVector, string.Empty, RosterScopeType.Fixed, rosterGroupsWithTitleQuestionPairs);
            rosterScopes.Add(scopeVector, rosterDescription);
            return rosterScopes;
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
