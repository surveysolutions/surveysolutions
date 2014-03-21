using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurvisorLoginsDenormalizerTests
{
    internal class when_handling_supervisor_regstered_event : SurvisorLoginsDenormalizerTestContext
    {
        Establish context = () =>
        {
            repositoryWriterMock = new Mock<IReadSideRepositoryWriter<SupervisorLoginView>>();
            denormalizer = CreateSurveyDetailsViewDenormalizer(repositoryWriter: repositoryWriterMock.Object);
            var surveyId = Guid.Parse("11111111111111111111111111111111");
            evnt = Create.PublishedEvent(new SupervisorRegistered(login, passwordHash), surveyId);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        private It should_persist_login_view_using_login_as_id = () =>
            repositoryWriterMock.Verify(x => x.Store(Moq.It.Is<SupervisorLoginView>(loginView => loginView != null), login));

        private static SupervisorLoginsDenormalizer denormalizer;
        private static IPublishedEvent<SupervisorRegistered> evnt;
        private static Mock<IReadSideRepositoryWriter<SupervisorLoginView>> repositoryWriterMock;
        private static string login = "Sidor";
        private static string passwordHash = "SidorsPassword";
    }
}