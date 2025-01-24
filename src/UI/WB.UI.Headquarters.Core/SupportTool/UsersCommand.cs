using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;
using Option = System.CommandLine.Option;

namespace WB.UI.Headquarters.SupportTool
{
    public class UsersCommand : Command
    {
        private readonly IHost host;

        public UsersCommand(IHost host) : base("users", "Manage users of Headquarters")
        {
            this.host = host;

            this.Add(UsersCreateCommand());
            this.Add(ResetPasswordCommand());
            this.Add(Disable2faCommand());
            this.Add(ReleaseAutoLockCommand());
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
                new Option(new [] { "--workspace", "-w" })
                {
                    Required = false,
                    Argument = new Argument<string>()
                },
                new Option("--email")
                {
                    Required = false,
                    Argument = new Argument<string>()
                },
                new Option("--supervisor")
                {
                    Required = false,
                    Argument = new Argument<string>()
                }
            };

            cmd.Handler = CommandHandler.Create<UserRoles, string, string, string, string, string>(async (role, password, login, workspace, email, supervisor) =>
            {
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                    var workspaceService = locator.GetInstance<IWorkspaceContextSetter>();
                    var workspaceName = workspace ?? WorkspaceConstants.DefaultWorkspaceName;
                    workspaceService.Set(workspaceName);

                    var userManager = locator.GetInstance<HqUserManager>();
                    var user = new HqUser
                    {
                        UserName = login,
                        Email = email,
                        PasswordChangeRequired = !(role is UserRoles.ApiUser or UserRoles.Administrator)
                    };

                    HqUser supervisorUser = null;

                    if (!string.IsNullOrWhiteSpace(supervisor) && role == UserRoles.Interviewer)
                        supervisorUser = await userManager.FindByNameAsync(supervisor);

                    if (role == UserRoles.Interviewer && supervisorUser == null)
                    {
                        logger.LogError("Supervisor name is required for interviewer creation");
                        return;
                    }

                    if (supervisorUser != null && supervisorUser.Workspaces.All(w => w.Workspace.Name != workspaceName))
                    {
                        logger.LogError("Supervisor must exists in workspace of interviewer");
                        return;
                    }
                    
                    var workspaces = locator.GetInstance<IWorkspacesStorage>();
                    var workspaceObj = await workspaces.GetByIdAsync(workspaceName);
                    user.Workspaces.Add(new WorkspacesUsers(workspaceObj, user, supervisorUser));

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
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(UsersCreateCommand));
                    var userManager = locator.GetInstance<HqUserManager>();
                    var user = await userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        logger.LogError($"User {username} not found");
                        unitOfWork.DiscardChanges();
                        return;
                    }

                    bool disableForcePassword = user.IsInRole(UserRoles.Administrator) || user.IsInRole(UserRoles.ApiUser);
                    if (!disableForcePassword)
                        user.PasswordChangeRequired = true;

                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, resetToken, password);
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

        private Command Disable2faCommand()
        {
            var cmd = new Command("disable2fa")
            {
                new Option(new [] { "--username", "--login" })
                {
                    Required = true,
                    Argument = new Argument<string>()
                }
            };

            cmd.Handler = CommandHandler.Create<string>(async (username) =>
            {
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(Disable2faCommand));
                    var userManager = locator.GetInstance<HqUserManager>();
                    var user = await userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        logger.LogError($"User {username} not found");
                        unitOfWork.DiscardChanges();
                        return;
                    }
                    
                    var result = await userManager.SetTwoFactorEnabledAsync(user, false);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Disable 2fa for user {username} succeeded");
                    }
                    else
                    {
                        logger.LogError($"Failed to disable 2fa for user {username}");
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

        private Command ReleaseAutoLockCommand()
        {
            var cmd = new Command("releaselock")
            {
                new Option(new [] { "--username", "--login" })
                {
                    Required = true,
                    Argument = new Argument<string>()
                }
            };

            cmd.Handler = CommandHandler.Create<string>(async (username) =>
            {
                var inScopeExecutor = this.host.Services.GetRequiredService<IInScopeExecutor>();
                await inScopeExecutor.ExecuteAsync(async (locator, unitOfWork) =>
                {
                    var loggerProvider = locator.GetInstance<ILoggerProvider>();
                    var logger = loggerProvider.CreateLogger(nameof(ReleaseAutoLockCommand));
                    var userManager = locator.GetInstance<HqUserManager>();
                    var user = await userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        logger.LogError($"User {username} not found");
                        unitOfWork.DiscardChanges();
                        return;
                    }
                    
                    var result = await userManager.SetLockoutEndDateAsync(user, null);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Release lock for user {username} succeeded");
                    }
                    else
                    {
                        logger.LogError($"Failed to release lock for user {username}");
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
    }
}
