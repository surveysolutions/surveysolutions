using System.Threading.Tasks;
using NConsole;

namespace ApiUtil.StatusChange
{
    internal class ApproveInterviewsCommand : ChangeInterviewStatusCommand, IConsoleCommand
    {
        private static string ApproveUrl => "approve";

        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            await base.RunAsync(ApproveUrl);
        }
    }
}