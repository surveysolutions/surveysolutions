using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurvisorCredentialsDenormalizerTests
{
    [Subject(typeof(SurvisorCredentialsDenormalizer))]
    public class when_new_supervisor_is_registered
    {
        Establish context = () =>
        {
            writer = Substitute.For<IReadSideRepositoryWriter<SupervisorCredentialsView>>();
            denormalizer = Create.SurvisorCredentialsDenormalizer(writer);
            evnt = Create.PublishedEvent(new SupervisorRegistered("login", "passwordhash"));
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_store_login_and_password_has_as_id = () => writer.Received().Store(Arg.Any<SupervisorCredentialsView>(), "login:passwordhash");
        
        private static SurvisorCredentialsDenormalizer denormalizer;
        private static IReadSideRepositoryWriter<SupervisorCredentialsView> writer;
        private static IPublishedEvent<SupervisorRegistered> evnt;
    }
}