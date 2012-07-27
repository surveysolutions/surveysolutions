using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Config;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Web.App_Start;

namespace RavenQuestionnaire.Core.Tests.Domain
{
    /// <summary>
    /// Don't do this! Instead, use an IoC container and one of the extension projects to configure your environment
    /// </summary>
    public class Configuration : IEnvironmentConfiguration
    {

        public static void Configure()
        {
            if (NcqrsEnvironment.IsConfigured) return;
            var cfg = new Configuration();
            NcqrsEnvironment.Configure(cfg);
        }

        private static ICommandService InitializeCommandService()
        {
           

            var service = new CommandService();
            InitializeInternalCommandService(service);
          //  service.AddInterceptor(new ThrowOnExceptionInterceptor());

            return service;
        }
        private static void InitializeInternalCommandService(CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
            foreach (var type in typeof(NCQRSInit).Assembly.GetTypes().Where(ImplementsAtLeastOneICommand))
            {
                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
        }
        private static bool ImplementsAtLeastOneICommand(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                      type.GetInterfaces().Any(IsICommandInterface);
        }
        private static bool IsICommandInterface(Type type)
        {
            return type.IsInterface &&
                   typeof(ICommand).IsAssignableFrom(type);
        }
        private readonly ICommandService _commandService;

        private Configuration()
        {
            _commandService = InitializeCommandService();
        }

        public bool TryGet<T>(out T result) where T : class
        {
            result = null;
            if (typeof(T) == typeof(ICommandService))
                result = (T)_commandService;
            return result != null;
        }

    }
}
