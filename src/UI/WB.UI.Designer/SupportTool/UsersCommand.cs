using System.CommandLine;
using System.CommandLine.Invocation;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Infrastructure.Native.Storage.Postgre;
using Option = System.CommandLine.Option;

namespace WB.UI.Designer.SupportTool
{
    public class UsersCommand : Command
    {
        private readonly IHost host;

        public UsersCommand(IHost host) : base("users", "Manage users of Headquarters")
        {
            this.host = host;

            this.Add(UsersCreateCommand());
            this.Add(ResetPasswordCommand());
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
                new Option(new [] { "--username", "--login" })
                {
                    Required = true,
                    Argument = new Argument<string>()
                },
                new Option("--email")
                {
                    Required = false,
                    Argument = new Argument<string>()
                }
            };

            cmd.Handler = CommandHandler.Create<UserRoles, string, string, string>(async (role, password, login, email) =>
            {
                using var scope = this.host.Services.CreateScope();
                var locator = scope.ServiceProvider;
                var db = locator.GetRequiredService<DesignerDbContext>();
                await using var tr = await db.Database.BeginTransactionAsync();
                var loggerProvider = locator.GetRequiredService<ILoggerProvider>();
                var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                var userManager = locator.GetRequiredService<UserManager<DesignerIdentityUser>>();
                var user = new DesignerIdentityUser
                {
                    UserName = login,
                    Email = email,
                    EmailConfirmed =true
                };
                var creationResult = await userManager.CreateAsync(user, password);
                if (creationResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.ToString());
                    logger.LogInformation("Created user {user} as {role}", login, role);
                    await tr.CommitAsync();
                }
                else
                {
                    logger.LogError("Failed to create user {user}", login);
                    foreach (var error in creationResult.Errors)
                    {
                        logger.LogError(error.Description);
                    }

                    await tr.RollbackAsync();
                }
            });
            return cmd;
        }

        private Command ResetPasswordCommand()
        {
            var cmd = new Command("reset-password")
            {
                new Option(new [] { "--username", "--login" })
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
                using var scope = this.host.Services.CreateScope();
                var locator = scope.ServiceProvider;
                var db = locator.GetRequiredService<DesignerDbContext>();
                await using var tr = await db.Database.BeginTransactionAsync();
                using var unitOfWork = locator.GetRequiredService<IUnitOfWork>();
                var loggerProvider = locator.GetRequiredService<ILoggerProvider>();
                var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                var userManager = locator.GetRequiredService<UserManager<DesignerIdentityUser>>();
                var user = await userManager.FindByNameAsync(username);
                if (user == null)
                {
                    logger.LogError($"User {username} not found");
                    return;
                }

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, resetToken, password);

                if (result.Succeeded)
                {
                    logger.LogInformation($"Reset password for user {username} succeeded");
                    await tr.CommitAsync();
                }
                else
                {
                    logger.LogError($"Failed to reset password for user {username}");
                    foreach (var error in result.Errors)
                    {
                        logger.LogError(error.Description);
                    }

                    await tr.RollbackAsync();
                }

            });
            return cmd;
        }
    }
}
