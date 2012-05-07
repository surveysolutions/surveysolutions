using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core
{
    public class CommandInvoker : ICommandInvoker
    {
        private IKernel container;
        private IDocumentSession documentSession;

        public CommandInvoker(IKernel container, IDocumentSession documentSession)
        {
            this.container = container;
            this.documentSession = documentSession;
        }

        public void Execute<T>(T command) where T : ICommand
        {
            var handler = container.Get<ICommandHandler<T>>();
            handler.Handle(command);
            //store the command in the store
            documentSession.Store(new EventDocument(command));
            documentSession.SaveChanges();
        }
    }
}
