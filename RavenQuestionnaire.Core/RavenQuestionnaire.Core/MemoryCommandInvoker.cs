using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            SaveEvent(handler.GetType(), command, container.Get<IDocumentSession>(), Guid.NewGuid(), clientSettingsProvider.ClientSettings.PublicKey);
            InvokeSubscribers(command);
        }

        //TODO remove that spike after event soursing implementation
        //protected void SaveEvent<T>(Type handlerType, T command, IDocumentSession documentSession) where T : ICommand
        protected void SaveEvent<T>(Type handlerType, T command, IDocumentSession documentSession, Guid eventPublicKey, Guid clientPublicKey) where T : ICommand
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
            //documentSession.Store(new EventDocument(command, Guid.NewGuid(),
            //                                        clientSettingsProvider.ClientSettings.PublicKey));
            documentSession.Store(new EventDocument(command, eventPublicKey, clientPublicKey));
        }
        protected Type GetGenericObject(Type generic, ICommand command)
        {
            var commandHandler = generic;
            Type[] typeArgs = {command.GetType()};
            return commandHandler.MakeGenericType(typeArgs);
        }

        public void Execute(ICommand command, Guid eventPublicKey, Guid clientPublicKey)
        {
            var reflectionGeneric = GetGenericObject(typeof (ICommandHandler<>), command);

            var handler = container.Get(GetGenericObject(typeof (ICommandHandler<>), command));
            reflectionGeneric.GetMethod("Handle").Invoke(handler, new object[] {command});
            //handler.Handle(command);
            //store the command in the store
            SaveEvent(handler.GetType(), command, container.Get<IDocumentSession>(), eventPublicKey, clientPublicKey);
            InvokeSubscribers(command);
        }

        protected void InvokeSubscribers<T>(T command) where T : ICommand
        {
            /*   WaitCallback callback = (state) =>
                                           {*/
            var reflectionGeneric = GetGenericObject(typeof (IEventSubscriber<>), command);
            var subscribers =
                container.GetAll(GetGenericObject(typeof (IEventSubscriber<>), command));

            foreach (object eventSubscriber in subscribers)
            {
                reflectionGeneric.GetMethod("Invoke").Invoke(eventSubscriber,
                                                             new object[] {command});
            }
            /*    };
ThreadPool.QueueUserWorkItem(callback, command);*/

        }

        public void Flush()
        {
            container.Get<IDocumentSession>().SaveChanges();
        }
    }
}
