using Ncqrs;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Web.App_Start
{
    public static class NCQRSInit
    {
        public static void Init(string repositoryPath)
        {
            NcqrsEnvironment.SetDefault(InitializeEventStore(repositoryPath));
            NcqrsEnvironment.SetDefault(InitializeCommandService());
            NcqrsEnvironment.SetDefault(new InMemoryEventStore());
            NcqrsEnvironment.SetDefault(new SimpleSnapshottingPolicy(1));

        }

        private static ICommandService InitializeCommandService()
        {
            var mapper = new AttributeBasedCommandMapper();
            var service = new CommandService();

            //add assembly scan to register executors 
            service.RegisterExecutor(typeof(CreateQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateLocationCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(CreateCompleteQuestionnaireCommand), new UoWMappedCommandExecutor(mapper));

            service.RegisterExecutor(typeof(AddGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(AddQuestionCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(SetAnswerCommand), new UoWMappedCommandExecutor(mapper));

            service.RegisterExecutor(typeof(AddPropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            service.RegisterExecutor(typeof(DeletePropagatableGroupCommand), new UoWMappedCommandExecutor(mapper));
            

            return service;
        }

        private static IEventStore InitializeEventStore(string storePath)
        {
            var eventStore = new RavenDBEventStore(storePath);
            return eventStore;
        }
    }
}