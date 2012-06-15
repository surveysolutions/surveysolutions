using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using DataEntryClient.CompleteQuestionnaire;
using DataEntryClient.WcfInfrastructure;
using Ninject;
using System.Threading;
using Ninject.Parameters;
using Raven.Client;
using RavenQuestionnaire.Core;
using DataEntryClient.WcfInfrastructure;
using RavenQuestionnaire.Core.Conventions;
using DataEntryClient.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ClientSettingsProvider;

namespace DataEntryClient
{
    internal class Program
    {
        static Mutex mSingleton;
        private static int result = 1;
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            bool created;
            mSingleton = new Mutex(true, "e622fa4b-7b23-4ee7-8bd6-09e8be84cb5d", out created);
            if (!created)
                return 0;
            try
            {
                var kernel = new StandardKernel(new CoreRegistry(ConfigurationManager.AppSettings["Raven.DocumentStore"], false));
                kernel.Bind<IChanelFactoryWrapper>().ToMethod((c) => new ChanelFactoryWrapper(args[0]));
                kernel.Bind<IDocumentSession>().ToMethod(
                    context => context.Kernel.Get<IDocumentStore>().OpenSession()).InThreadScope();
                new CompleteQuestionnaireSync(kernel,
                                              Guid.Parse(args[1]))
                    .
                    Execute();
            }
            catch
            {
                return 0;
            }
            return result;
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            result = 0;
        }
       
    }
}
