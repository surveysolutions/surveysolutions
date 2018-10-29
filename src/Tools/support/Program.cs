using System;
using System.Collections.Generic;
using System.Linq;
using NConsole;
using Ninject;
using NLog;

namespace support
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("support");
            logger.Info($"Support tool started {DateTime.Now}.");
            var ninjectKernel = new StandardKernel();
            ninjectKernel.Bind<INetworkService>().To<NetworkService>();
            ninjectKernel.Bind<IDatabaseService>().To<PostgresDatabaseService>();
            ninjectKernel.Bind<IConfigurationManagerSettings>().To<ConfigurationManagerSettings>().InSingletonScope();
            ninjectKernel.Bind<ILogger>().ToConstant(logger);

            var processor = new CommandLineProcessor(new ConsoleHost(), new ConsoleDependencyResolver(ninjectKernel));
            ((Dictionary<string, Type>)processor.Commands).Clear();

            processor.RegisterCommand<ResetPasswordCommand>("reset-password");
            processor.RegisterCommand<CheckAccessCommand>("health-check");
            processor.RegisterCommand<ArchiveLogsCommand>("archive-logs");
            processor.RegisterCommand<CustomHelpCommand>("help");
            processor.RegisterCommand<CustomHelpCommand>("");

            try
            {
                processor.Process(args);
            }
            catch (AggregateException e)
            {
                var invalidCommandException = e.InnerExceptions.OfType<InvalidOperationException>().FirstOrDefault();

                if (invalidCommandException != null)
                    Console.WriteLine(invalidCommandException.Message);
                else
                    throw;
            }
            catch (Exception e)
            {
                logger.Error(e);
                Console.WriteLine("Unexpected exception. See logs for more details");
            }

            logger.Info($"Support tool finished {DateTime.Now}.");
        }
    }
}
