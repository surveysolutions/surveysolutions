using System;
using System.Runtime.InteropServices;
using NConsole;
using Ninject;
using NLog;

namespace support
{
    class Program
    {
        const int StdOutputHandle = -11;
        const uint EnableVirtualTerminalProcessing = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        static void Main(string[] args)
        {
            var handle = GetStdHandle(StdOutputHandle);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= EnableVirtualTerminalProcessing;
            SetConsoleMode(handle, mode);

            var ninjectKernel = new StandardKernel();
            ninjectKernel.Bind<INetworkService>().To<NetworkService>();
            ninjectKernel.Bind<IDatabaseSevice>().To<PostgresDatabaseService>();
            ninjectKernel.Bind<IConfigurationManagerSettings>().To<ConfigurationManagerSettings>().InSingletonScope();
            ninjectKernel.Bind<ILogger>().ToConstant(LogManager.GetLogger("support"));

            var processor = new CommandLineProcessor(new ConsoleHost(), new ConsoleDependencyResolver(ninjectKernel));
            processor.RegisterCommand<CheckAccessCommand>("health-check");
            processor.RegisterCommand<ArchiveLogsCommand>("archive-logs");
            processor.RegisterCommand<CustomHelpCommand>("");
            processor.Process(args);
        }
    }
}
