using System;
using System.Linq;
using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core
{
    public class CommandInvoker : ICommandInvoker
    {
        private IKernel container;
        private IDocumentSession documentSession;
        private IClientSettingsProvider clientSettingsProvider;
        public CommandInvoker(IKernel container/*, IDocumentSession documentSession*/, IClientSettingsProvider clientSettingsProvider)
        {
            this.container = container;
            this.documentSession = container.Get<IDocumentSession>();
            this.clientSettingsProvider=clientSettingsProvider;
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
            SaveEvent(handler.GetType(), command);
            documentSession.SaveChanges();
        }
        //TODO remove that spike after event soursing implementation
        protected void SaveEvent<T>(Type handlerType, T command) where T : ICommand
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(handlerType); // Reflection.

            // Displaying output.
            foreach (System.Attribute attr in attrs)
            {
                if (attr is CommandHandlerAttribute)
                {
                    CommandHandlerAttribute a = (CommandHandlerAttribute) attr;
                    if (a.IgnoreAsEvent)
                        //store the command in the store
                        return;

                }
            }
            documentSession.Store(new EventDocument(command, Guid.NewGuid(),
                                                    clientSettingsProvider.ClientSettings.PublicKey));
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
            SaveEvent(handler.GetType(), command);
            documentSession.SaveChanges();

        }
    }
}
