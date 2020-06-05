using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace WB.UI.Designer.SupportTool
{
    public class SupportTool
    {
        private readonly IWebHost host;

        public SupportTool(IWebHost host)
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
                new ImportQuestionnaireCommand(this.host)
            };

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }
    }
}
