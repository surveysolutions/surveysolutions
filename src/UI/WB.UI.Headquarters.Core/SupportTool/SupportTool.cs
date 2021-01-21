using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WB.UI.Headquarters.SupportTool
{
    public class SupportTool
    {
        private readonly IHost host;

        public SupportTool(IHost host)
        {
            this.host = host;
        }

        public async Task<int> Run(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new MigrateCommand(this.host),
                new UsersCommand(this.host),
                new VersionCommand(this.host),
                new WorkspacesCommand(this.host)
            };

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }
    }
}
