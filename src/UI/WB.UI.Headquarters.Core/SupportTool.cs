using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using Option = System.CommandLine.Option;

namespace WB.UI.Headquarters
{
    public class SupportTool
    {
        private readonly IHost host;

        public SupportTool(IHost host)
        {
            this.host = host;
        }

        private IConfiguration Configuration => host.Services.GetRequiredService<IConfiguration>();

        public async Task<int> Run(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                MigrateCommand(),
                UsersCommand()
            };


            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }

        private Command UsersCommand()
        {
            return new Command("users")
            {
                UsersCreateCommand(),
                ResetPasswordCommand(),
            };
        }

        private Command UsersCreateCommand()
        {
            var cmd = new Command("create")
            {
                new Option("--role")
                {
                    Required = true,
                    Argument = new Argument<UserRoles>()
                },
                new Option("--password")
                {
                    Required = true,
                    Argument = new Argument<string>()
                },
                new Option("--login")
                {
                    Required = true,
                    Argument = new Argument<string>()
                }
            };
            cmd.Handler = CommandHandler.Create<UserRoles, string, string>(async (role, password, login) =>
            {
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                    var userManager = locator.GetInstance<UserManager<HqUser>>();
                    var user = new HqUser
                    {
                        UserName = login
                    };
                    var creationResult = await userManager.CreateAsync(user, password);
                    if (creationResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role.ToString());
                        logger.LogInformation("Created user {user} as {role}", login, role);
                    }
                    else
                    {
                        logger.LogError("Failed to create user {user}", login);
                        foreach (var error in creationResult.Errors)
                        {
                            logger.LogError(error.Description);
                        }
                        
                        unitOfWork.DiscardChanges(); 
                    }
                });

            });
            return cmd;
        }

        private Command ResetPasswordCommand()
        {
            var cmd = new Command("reset-password")
            {
                new Option("--username")
                {
                    Required = true,
                    Argument = new Argument<string>()
                },
                new Option("--password")
                {
                    Required = true,
                    Argument = new Argument<string>()
                },
            };
            cmd.Handler = CommandHandler.Create<string, string>(async (username, password) =>
            {
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                    var userManager = locator.GetInstance<UserManager<HqUser>>();
                    var user = await userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        logger.LogError($"User {username} not found");
                        unitOfWork.DiscardChanges();
                        return;
                    }

                    var result = await userManager.ResetPasswordAsync(user, user.PasswordHashSha1, password);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Reset password for user {username} succeeded");
                    }
                    else
                    {
                        logger.LogError($"Failed to reset password for user {username}");
                        foreach (var error in result.Errors)
                        {
                            logger.LogError(error.Description);
                        }
                        
                        unitOfWork.DiscardChanges(); 
                    }
                });

            });
            return cmd;
        }

        private Command MigrateCommand()
        {
            return new Command("migrate")
            {
                Handler = CommandHandler.Create(async () =>
                {
                    var connectionString = Configuration.GetConnectionString("DefaultConnection");
                    UnitOfWorkConnectionSettings unitOfWorkConnectionSettings =
                        Startup.BuildUnitOfWorkSettings(connectionString);
                    await new OrmModule(unitOfWorkConnectionSettings).Init(
                        host.Services.GetRequiredService<IServiceLocator>(),
                        new UnderConstructionInfo());
                })
            };
        }
    }
}
