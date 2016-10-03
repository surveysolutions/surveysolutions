using ApiUtil.Export;
using ApiUtil.StatusChange;
using NConsole;

namespace ApiUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor(new ConsoleHost());
            processor.RegisterCommand<ApproveInterviewsCommand>("ApproveInterviews");
            processor.RegisterCommand<RejectInterviewsCommand>("RejectInterviews");
            processor.RegisterCommand<ExportDataCommand>("Export");
            processor.Process(args);
        }
    }
}
