using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Persistence.Headquarters.Migrations.Workspace;

namespace WB.UI.Headquarters.SupportTool
{
    public class WorkspacesCommand : Command
    {
        private readonly IHost host;
        
        public WorkspacesCommand(IHost host) : base("workspaces", "Migrate database to latest version")
        {
            this.host = host;

            var addCommand = new Command("add")
            {
                new Option(new[] { "--name", "-n" }, "Name of the workspace")
                {
                    Required = true,
                    Argument = new Argument<string>()
                },

                new Option(new[] { "--title", "-t" }, "Title of the workspace")
                {
                    Required = true,
                    Argument = new Argument<string>()
                }
            };

            addCommand.Handler = CommandHandler.Create<string, string>(Workspace);

            this.Add(addCommand);
        }

        private async Task Workspace(string name, string title)
        {
            var logger = host.Services.GetRequiredService<ILogger<WorkspacesCommand>>();
            IPlainStorageAccessor<Workspace> workspaces =
                            host.Services.GetRequiredService<IPlainStorageAccessor<Workspace>>();

            var service = host.Services.GetRequiredService<IWorkspacesService>();
            var workspace = new Workspace(name, title);

            workspaces.Store(workspace, null);
            await service.Generate(workspace.Name,
                DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());

            logger.LogInformation("Added workspace {name}");
        }
    }
}
