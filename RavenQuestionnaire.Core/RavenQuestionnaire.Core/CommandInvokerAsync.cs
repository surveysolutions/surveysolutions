using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core
{
    public interface ICommandInvokerAsync
    {
        IAsyncResult BeginExecute<T>(T command, AsyncCallback callback, object state) where T : ICommand;
        void EndExecute(IAsyncResult asyncResult);
        void Execute<T>(T command) where T : ICommand;
    }

    public class CommandInvokerAsync : ICommandInvokerAsync
    {
        private IKernel container;
       // private IDocumentSession documentSession;
        private Action _delegate;
        private AutoResetEvent _asyncActiveEvent;
        public CommandInvokerAsync(IKernel container)
        {
            this.container = container;
         //   this.documentSession = this.container.Get<IDocumentSession>();
        }
        #region Implementation of ICommandInvokerAsync

        public IAsyncResult BeginExecute<T>(T command, AsyncCallback callback, object state) where T : ICommand
        {


            if (_asyncActiveEvent == null)
            {
                bool flag = false;
                try
                {
                    Monitor.Enter(this, ref flag);
                    if (_asyncActiveEvent == null)
                    {
                        _asyncActiveEvent = new AutoResetEvent(true);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            _asyncActiveEvent.WaitOne();
            _delegate = () =>
                            {
                                var documentSession = this.container.Get<IDocumentSession>();
                                var handler = container.Get<ICommandHandler<T>>();
                                handler.Handle(command);
                                //store the command in the store
                                /*  documentSession.Store(new EventDocument(command));*/
                                documentSession.SaveChanges();
                            };
            return _delegate.BeginInvoke(callback, state);
        }

        public void EndExecute(IAsyncResult asyncResult)
        {
            try
            {
                _delegate.EndInvoke(asyncResult);
            }
            finally
            {
                _delegate = null;
                _asyncActiveEvent.Set();
            }
        }

        public void Execute<T>(T command) where T : ICommand
        {
         //   ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
            this.BeginExecute(command, (state) => ((ICommandInvokerAsync)state.AsyncState).EndExecute(state), this);
        }

        #endregion
    }

}
