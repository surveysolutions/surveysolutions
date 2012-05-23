using System;
using System.Linq;
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
         /*   if (handler == null)
            {
                Execute(command as ICommand);
                return;
            }*/
            handler.Handle(command);
            //store the command in the store
            var clientPublicKey = this.documentSession.Query<ClientSettingsDocument>().FirstOrDefault().PublicKey;
            documentSession.Store(new EventDocument(command, Guid.NewGuid(), clientPublicKey));
            documentSession.SaveChanges();
        }

        public void Execute(ICommand command, Guid eventPublicKey, Guid clientPublicKey) 
        {
            var commandHandler = typeof(ICommandHandler<>);
            Type[] typeArgs = { command.GetType() };
            var reflectionGeneric = commandHandler.MakeGenericType(typeArgs);

            var handler = container.Get(reflectionGeneric);
            reflectionGeneric.GetMethod("Handle").Invoke(handler, new object[] {command});
            //handler.Handle(command);
            //store the command in the store
            documentSession.Store(new EventDocument(command, eventPublicKey, clientPublicKey));
            documentSession.SaveChanges();

        }
    }
}
