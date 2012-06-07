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
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Syntax;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;
using SynchronizationMessages.CompleteQuestionnaire;

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
            {
                return 0;
            }
            try
            {

                var kernel =
                    new StandardKernel(new CoreRegistry(ConfigurationManager.AppSettings["Raven.DocumentStore"]));
                kernel.Bind<IChanelFactoryWrapper>().ToMethod((c) => new ChanelFactoryWrapper(args[0]));

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
