using System;
using Ninject;
using System.Threading;
using System.Configuration;
using Ninject.Parameters;
using RavenQuestionnaire.Core;
using Ninject.Extensions.Conventions;
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
                var kernel = new StandardKernel(new CoreRegistry(ConfigurationManager.AppSettings["Raven.DocumentStore"]));
                kernel.Bind<IChanelFactoryWrapper>().ToMethod((c) => new ChanelFactoryWrapper(args[0]));
                RegisterServices(kernel);
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

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
          

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(ICommandHandler<>)));
            });

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IViewFactory<,>)));
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IExpressionExecutor<,>)));
            });
          
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new RegisterFirstInstanceOfInterface());
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(Iterator<>)));
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IEntitySubscriber<>)));
            });
        }
       
    }
}
