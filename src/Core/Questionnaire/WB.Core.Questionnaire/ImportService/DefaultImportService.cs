using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.View;
using Ncqrs;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using WB.Core.Questionnaire.ImportService.Commands;

namespace WB.Core.Questionnaire.ImportService
{
    public class DefaultImportService : CommandExecutorBase<ImportQuestionnaireCommand>
    {
        private readonly IEventStore store;
        private readonly IEventBus bus;

        public DefaultImportService()
        {
            store = NcqrsEnvironment.Get<IEventStore>();
            bus = NcqrsEnvironment.Get<IEventBus>();
        }

        protected override void ExecuteInContext(IUnitOfWorkContext context, ImportQuestionnaireCommand command)
        {
            var document = command.Source as QuestionnaireDocument;
            if (document == null)
                throw new ArgumentException("only QuestionnaireDocuments are supported for now");
            
            var questionnsire = context.GetById<QuestionnaireAR>(command.CommandIdentifier);
            if (questionnsire != null)
                throw new ArgumentException("Questionnair with the same key present in system");

            #warning Nastya:redo on create snapshot after merge with next branch
            document.CreatedBy = command.CreatedBy;
            var eventVersion = GetType().Assembly.GetName().Version;
            var singleEvent = new UncommittedEvent(Guid.NewGuid(), command.CommandIdentifier, 1, 1, DateTime.Now,
                                                   new SnapshootLoaded()
                                                       {
                                                           Template =
                                                               new Snapshot(command.CommandIdentifier, 1, command.Source)
                                                       },
                                                   eventVersion);
            var stream = new UncommittedEventStream(Guid.NewGuid());
            stream.Append(singleEvent);

            store.Store(stream);
            bus.Publish(singleEvent);

        }

    }
}
