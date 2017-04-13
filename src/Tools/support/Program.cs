using System;
using NConsole;
using Ninject;

namespace support
{
    class Program
    {
        static void Main(string[] args)
        {
            var ninjectKernel = new StandardKernel();
            ninjectKernel.Bind<INetworkService>().To<NetworkService>();
            ninjectKernel.Bind<IDatabaseSevice>().To<PostgresDatabaseService>();
            ninjectKernel.Bind<IConfigurationManagerSettings>().To<ConfigurationManagerSettings>().InSingletonScope();

            var processor = new CommandLineProcessor(new ConsoleHost(), new ConsoleDependencyResolver(ninjectKernel));
            processor.RegisterCommand<CheckAccessCommand>("health-check");
            processor.RegisterCommand<ArchiveLogsCommand>("archive-logs");
            processor.RegisterCommand<HelpCommand>("");
            processor.Process(args);
        }
    }
}
