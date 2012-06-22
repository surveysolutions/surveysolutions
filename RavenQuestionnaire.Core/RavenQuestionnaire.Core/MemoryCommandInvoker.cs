using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core
{
    public class MemoryCommandInvoker : IMemoryCommandInvoker
    {
        private IKernel container;
     //   private IDocumentSession documentSession;
        private IClientSettingsProvider clientSettingsProvider;

        public MemoryCommandInvoker(IKernel container,
                                    IClientSettingsProvider clientSettingsProvider)
        {
            this.container = container;
         //   this.documentSession = container.Get<IDocumentSession>();
            this.clientSettingsProvider = clientSettingsProvider;
        }

        public void Execute<T>(T command) where T : ICommand
        {
          //  var documentSession = container.Get<IDocumentSession>();
            var handler = container.Get<ICommandHandler<T>>();
            handler.Handle(command);
            SaveEvent(handler.GetType(), command, container.Get<IDocumentSession>());
        }

        //TODO remove that spike after event soursing implementation
        protected void SaveEvent<T>(Type handlerType, T command, IDocumentSession documentSession) where T : ICommand
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
           // var documentSession = container.Get<IDocumentSession>();
            var commandHandler = typeof (ICommandHandler<>);
            Type[] typeArgs = {command.GetType()};
            var reflectionGeneric = commandHandler.MakeGenericType(typeArgs);

            var handler = container.Get(reflectionGeneric);
            reflectionGeneric.GetMethod("Handle").Invoke(handler, new object[] {command});
            //handler.Handle(command);
            //store the command in the store
            SaveEvent(handler.GetType(), command, container.Get<IDocumentSession>());

        }

        public void Flush()
        {
            container.Get<IDocumentSession>().SaveChanges();
        }
    }
}
