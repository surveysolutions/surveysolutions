using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyDetailsViewDenormalizerTests
{
    [Subject(typeof(SurveyDetailsViewDenormalizer))]
    internal class SurveyDetailsViewDenormalizerTestsContext
    {
        protected static SurveyDetailsViewDenormalizer CreateSurveyDetailsViewDenormalizer(
            IReadSideRepositoryWriter<SurveyDetailsView> repositoryWriter = null)
        {
            return new SurveyDetailsViewDenormalizer(repositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<SurveyDetailsView>>());
        }

        protected static SurveyDetailsView CreateSurveyDetailsView(List<SupervisorView> supervisors = null)
        {
            return new SurveyDetailsView
            {
                Supervisors = supervisors ?? new List<SupervisorView>()
            };
        }

        protected static SupervisorView CreateSupervisorView(string login)
        {
            return new SupervisorView
            {
                Login = login
            };
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(Guid? eventSourceId = null, T @event = null) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("1234567890abcdef0101010102020304"))
                && publishedEvent.EventSequence == 1);
        }
    }
}