using NConsole;

namespace dbup
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor(new ConsoleHost());
            processor.RegisterCommand<HqUpdateReadSide>("hq-up-rs");
            processor.RegisterCommand<HqUpdatePlain>("hq-up-plain");
            processor.RegisterCommand<DesignUpdateReadSide>("des-up-rs");
            processor.RegisterCommand<DesignUpdatePlain>("des-up-plain");
            processor.Process(args);
        }
    }
}
