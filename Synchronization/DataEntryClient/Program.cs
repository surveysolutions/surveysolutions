using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DataEntryClient.CompleteQuestionnaire;
using DataEntryClient.WcfInfrastructure;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Syntax;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
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

        private static void Main()
        {
            var kernel = new StandardKernel(new CoreRegistry(ConfigurationManager.AppSettings["Raven.DocumentStore"]));
            kernel.Bind<IChanelFactoryWrapper>().To<ChanelFactoryWrapper>();
            RegisterServices(kernel);

            new CompleteQuestionnaireSync(kernel.Get<ICommandInvoker>(), kernel.Get<IViewRepository>(), kernel.Get<IChanelFactoryWrapper>()).Execute();
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
