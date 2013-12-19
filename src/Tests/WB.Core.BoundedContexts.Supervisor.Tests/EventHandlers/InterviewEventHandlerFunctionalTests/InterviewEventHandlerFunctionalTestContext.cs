﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewEventHandlerFunctionalTests
{
    internal class InterviewEventHandlerFunctionalTestContext
    {
        protected static InterviewEventHandlerFunctional CreateInterviewEventHandlerFunctional(QuestionnaireRosterStructure rosterStructure=null)
        {
            var questionnaireRosterStructureMockStorage = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>();
            questionnaireRosterStructureMockStorage.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(rosterStructure);
            questionnaireRosterStructureMockStorage.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<long>())).Returns(rosterStructure);
            return new InterviewEventHandlerFunctional(
                new Mock<IReadSideRepositoryWriter<UserDocument>>().Object,
                questionnaireRosterStructureMockStorage.Object,
                new Mock<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>().Object);
        }

        protected static QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(Guid scopeId,
            Dictionary<Guid, Guid> rosterGroupsWithTitleQuestionPairs = null, params Guid[] groupIdsFromScope)
        {
            var rosterStructure = new QuestionnaireRosterStructure();
            var rosterDescription = new RosterDescription(scopeId, rosterGroupsWithTitleQuestionPairs ?? new Dictionary<Guid, Guid>());
            groupIdsFromScope.ToList().ForEach(groupId => rosterDescription.RosterGroupsId.Add(groupId));
            rosterStructure.RosterScopes.Add(scopeId, rosterDescription);
            return rosterStructure;
        }

        protected static ViewWithSequence<InterviewData> CreateViewWithSequenceOfInterviewData()
        {
            return new ViewWithSequence<InterviewData>(new InterviewData(), 1);
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(T payload)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();
            publishableEventMock.Setup(x => x.Payload).Returns(payload);
            return publishableEventMock.Object;
        }
    }
}
