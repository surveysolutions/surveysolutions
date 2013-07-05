namespace Main.Core.Tests.Domain
{
    using System;
    using System.Linq;

    using Main.Core.Domain;

    using Ncqrs;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Config;

    /// <summary>
    /// Don't do this! Instead, use an IoC container and one of the extension projects to configure your environment
    /// </summary>
    public class Configuration : IEnvironmentConfiguration
    {
        #region Fields

        /// <summary>
        /// The _command service.
        /// </summary>
        private readonly ICommandService commandService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="Configuration"/> class from being created.
        /// </summary>
        private Configuration()
        {
            this.commandService = InitializeCommandService();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The configure.
        /// </summary>
        public static void Configure()
        {
            if (NcqrsEnvironment.IsConfigured)
            {
                return;
            }

            var cfg = new Configuration();
            NcqrsEnvironment.Configure(cfg);
        }

        /// <summary>
        /// The try get.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool TryGet<T>(out T result) where T : class
        {
            result = null;
            if (typeof(T) == typeof(ICommandService))
            {
                result = (T)this.commandService;
            }

            return result != null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The implements at least one i command.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool ImplementsAtLeastOneICommand(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(IsICommandInterface);
        }

        /// <summary>
        /// The initialize command service.
        /// </summary>
        /// <returns>
        /// The Ncqrs.Commanding.ServiceModel.ICommandService.
        /// </returns>
        private static ICommandService InitializeCommandService()
        {
            var service = new CommandService();
            InitializeInternalCommandService(service);

            // service.AddInterceptor(new ThrowOnExceptionInterceptor());
            return service;
        }

        /// <summary>
        /// The initialize internal command service.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        private static void InitializeInternalCommandService(CommandService service)
        {
            var mapper = new AttributeBasedCommandMapper();
            foreach (Type type in typeof(QuestionnaireAR).Assembly.GetTypes().Where(ImplementsAtLeastOneICommand))
            {
                service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            }
        }

        /// <summary>
        /// The is i command interface.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private static bool IsICommandInterface(Type type)
        {
            return type.IsInterface && typeof(ICommand).IsAssignableFrom(type);
        }

        #endregion
    }
}