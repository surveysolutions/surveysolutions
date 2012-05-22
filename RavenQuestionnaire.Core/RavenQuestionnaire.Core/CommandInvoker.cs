using System;
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
            var handler = container.TryGet<ICommandHandler<T>>();
            if (handler == null)
            {
                Execute(command as ICommand);
                return;
            }
            handler.Handle(command);
            //store the command in the store
            documentSession.Store(new EventDocument(command));
            documentSession.SaveChanges();
        }

        protected void Execute(ICommand command) 
        {
            var commandHandler = typeof(ICommandHandler<>);
            Type[] typeArgs = { command.GetType() };
            var reflectionGeneric = commandHandler.MakeGenericType(typeArgs);

            var handler = container.Get(reflectionGeneric);
            reflectionGeneric.GetMethod("Handle").Invoke(handler, new object[] {command});
            //handler.Handle(command);
            //store the command in the store
            documentSession.Store(new EventDocument(command));
            documentSession.SaveChanges();

        }
    }
}
