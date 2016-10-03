using System.Threading.Tasks;
using NConsole;

namespace ApiUtil.StatusChange
{
    internal class RejectInterviewsCommand : ChangeInterviewStatusCommand, IConsoleCommand
    {
        private static string RejectUrl => "reject";
        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            await base.RunAsync(RejectUrl);
        }
    }
}