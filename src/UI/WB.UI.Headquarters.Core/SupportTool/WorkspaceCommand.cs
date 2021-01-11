using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Persistence.Headquarters.Migrations.Workspace;

namespace WB.UI.Headquarters.SupportTool
{
    public class WorkspacesCommand : Command
    {
        private readonly IHost host;
        
        public WorkspacesCommand(IHost host) : base("workspaces", "Migrate database to latest version")
        {
            this.host = host;

            var addCommand = new Command("add", "Add new workspace to Headquarters")
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

            addCommand.Handler = CommandHandler.Create<string, string>(AddWorkspace);

            this.Add(addCommand);

            var listCommand = new Command("list", "List workspaces");
            listCommand.Handler = CommandHandler.Create(ListWorkspaces);
            this.Add(listCommand);
        }

        private Task ListWorkspaces()
        {
            var ws = host.Services.GetRequiredService<IWorkspacesCache>();

            foreach (var workspaceContext in ws.AllEnabledWorkspaces())
            {
                Console.WriteLine($"{workspaceContext.Name}\t\t{workspaceContext.DisplayName}");
            }

            return Task.CompletedTask;
        }

        private async Task AddWorkspace(string name, string title)
        {
            using var scope = host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<WorkspacesCommand>>();
            IPlainStorageAccessor<Workspace> workspaces =
                scope.ServiceProvider.GetRequiredService<IPlainStorageAccessor<Workspace>>();

            var service = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
            var workspace = new Workspace(name.ToLower(), title);

            workspaces.Store(workspace, null);
            await service.Generate(workspace.Name,
                DbUpgradeSettings.FromFirstMigration<M202011201421_InitSingleWorkspace>());

            uow.AcceptChanges();
            logger.LogInformation("Added workspace {name}", workspace.Name);
        }
    }
}
