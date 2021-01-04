using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Designer.Services.Restore;

namespace WB.UI.Designer.SupportTool
{
    public class ImportQuestionnaireCommand : Command
    {
        private readonly IHost host;

        public ImportQuestionnaireCommand(IHost host) : base("import", "Import questionnaire")
        {
            this.host = host;

            this.AddOption(new Option(new[] { "--path" }, "Path to questionnaire backup to import")
            {
                Required = true,
                Argument = new Argument<string>()
            });

            this.AddOption(new Option(new[] { "--username", "--login" }, "Username of user with Administrative rights")
            {
                Required = true,
                Argument = new Argument<string>()
            });

            this.AddOption(new Option(new[] { "--createnew" }, "Create a new questionnaire.")
            {
                Required = false,
                Argument = new Argument<bool>(() => false)
            });

            this.Handler = CommandHandler.Create<string, string, bool>(ImportQuestionnaire);
        }

        private async Task ImportQuestionnaire(string path, string username, bool createnew)
        {
            using var scope = this.host.Services.CreateScope();
            var locator = scope.ServiceProvider;
            await new DesignerBoundedContextModule().InitAsync(locator.GetRequiredService<IServiceLocator>(), new UnderConstructionInfo());
            var loggerProvider = locator.GetRequiredService<ILoggerProvider>();
            var logger = loggerProvider.CreateLogger(nameof(ImportQuestionnaireCommand));
            var userManager = locator.GetRequiredService<UserManager<DesignerIdentityUser>>();
            var user = await userManager.FindByNameAsync(username);

            if (user == null || !await userManager.IsInRoleAsync(user, "Administrator"))
            {
                throw new Exception($"Cannot find user {username} or user is not Administrator");
            }

            var restore = locator.GetRequiredService<IQuestionnaireRestoreService>();

            if (!File.Exists(path))
            {
                throw new Exception($"Cannot find file at path {path}");
            }

            var fs = File.OpenRead(path);
            var state = new RestoreState();
            try
            {
                var id = restore.RestoreQuestionnaire(fs, user.Id, state, createnew);

                if (state.Error != null)
                {
                    await Console.Error.WriteLineAsync(state.Error);
                }
                else
                {
                    Console.WriteLine($@"Restore finished. Restored {state.RestoredEntitiesCount} entities. Questionnaire Id:{id}");
                }

            }
            catch (Exception e)
            {
                if (state.Error != null)
                {
                    logger.LogError(e, state.Error);
                }
            }
        }
    }
}
