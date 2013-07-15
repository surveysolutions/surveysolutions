using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Config;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    /// <summary>
    /// Don't do this! Instead, use an IoC container and one of the extension projects to configure your environment
    /// </summary>
    public class NcqrsEnvironmentConfiguration : IEnvironmentConfiguration
    {
        private readonly ICommandService commandService;

        private NcqrsEnvironmentConfiguration()
        {
            this.commandService = InitializeCommandService();
        }

        public static void Configure()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            if (NcqrsEnvironment.IsConfigured)
            {
                return;
            }

            var cfg = new NcqrsEnvironmentConfiguration();
            NcqrsEnvironment.Configure(cfg);
        }

        public bool TryGet<T>(out T result) where T : class
        {
            result = null;
            if (typeof(T) == typeof(ICommandService))
            {
                result = (T)this.commandService;
            }

            return result != null;
        }

        private static bool ImplementsAtLeastOneICommand(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(IsICommandInterface);
        }

        private static ICommandService InitializeCommandService()
        {
            var service = new CommandService();
            InitializeInternalCommandService(service);

            // service.AddInterceptor(new ThrowOnExceptionInterceptor());
            return service;
        }

        private static void InitializeInternalCommandService(CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
            foreach (Type type in ScanForTypes().Where(ImplementsAtLeastOneICommand))
            {
                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
        }

        private static IEnumerable<Type> ScanForTypes()
        {
            Assembly[] assemblies =
            {
                typeof(ImportQuestionnaireCommand).Assembly,
            };

            return assemblies.SelectMany(assembly => assembly.GetTypes());
        }

        private static bool IsICommandInterface(Type type)
        {
            return type.IsInterface && typeof(ICommand).IsAssignableFrom(type);
        }
    }
}