using System;
using System.Linq;
using System.Threading;
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
        public void ExecuteInSingleScope<T>(T command) where T : ICommand
        {
            Action _delegate = () =>
                                   {
                                       var documentSession = this.container.Get<IDocumentSession>();
                                       var handler = container.Get<ICommandHandler<T>>();
                                       handler.Handle(command);
                                       SaveEvent(handler.GetType(), command);
                                       documentSession.SaveChanges();
                                   };
            bool areWeThere = false;
            IAsyncResult ar = _delegate.BeginInvoke((result) =>
                                                        {
                                                            areWeThere = true;
                                                        }

                                                    , null);
            while (!areWeThere)
            {

            }
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
            InvokeSubscribers(command);
        }
        protected void InvokeSubscribers<T>(T command) where T : ICommand
        {
            WaitCallback callback = (state) =>
                                        {
                                            var subscribers = container.GetAll<IEventSubscriber<T>>();
                                            foreach (IEventSubscriber<T> eventSubscriber in subscribers)
                                            {
                                                eventSubscriber.Invoke(command);
                                            }
                                        };
            ThreadPool.QueueUserWorkItem(callback, command);

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
            InvokeSubscribers(command);
        }
    }
}
