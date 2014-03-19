using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;
namespace WB.Core.BoundedContexts.Headquarters.Tests.SurvisorLoginsDenormalizerTests
{
    internal class when_handling_supervisor_regstered_event : SurvisorLoginsDenormalizerTestContext
    {
        Establish context = () =>
        {
            repositoryWriterMock = new Mock<IReadSideRepositoryWriter<SupervisorLoginView>>();
            repositoryWriterMock
                .Setup(x => x.Store(it.IsAny<SupervisorLoginView>(), login))
                .Callback((SupervisorLoginView lw, string l) => passedView = lw);
            denormalizer = CreateSurveyDetailsViewDenormalizer(repositoryWriter: repositoryWriterMock.Object);
            var surveyId = Guid.Parse("11111111111111111111111111111111");
            evnt = ToPublishedEvent(surveyId, new SupervisorRegistered(login, passwordHash));
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_pass_not_empty_supervisor_login_view = () => 
            passedView.ShouldNotBeNull();

        It should_persist_login_view_using_login_as_id = () => 
            repositoryWriterMock.Verify(x => x.Store(Moq.It.IsAny<SupervisorLoginView>(), login));

        private static SupervisorLoginView passedView;
        private static SupervisorLoginsDenormalizer denormalizer;
        private static IPublishedEvent<SupervisorRegistered> evnt;
        private static Mock<IReadSideRepositoryWriter<SupervisorLoginView>> repositoryWriterMock;
        private static string login = "Sidor";
        private static string passwordHash = "SidorsPassword";
    }
}