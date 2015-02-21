using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.EventHandlers.ReadyToSendToHeadquartersInterviewDenormalizerTests
{
    [Subject(typeof(ReadyToSendToHeadquartersInterviewDenormalizer))]
    internal class ReadyToSendToHeadquartersInterviewDenormalizerTestsContext
    {
        protected static ReadyToSendToHeadquartersInterviewDenormalizer CreateReadyToSendToHeadquartersInterviewDenormalizer(
            IReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview> readSideRepositoryWriter = null)
        {
            return new ReadyToSendToHeadquartersInterviewDenormalizer(
                readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>());
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid? eventSourceId = null)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("55555555555555555555555555555555")));
        }
    }
}
