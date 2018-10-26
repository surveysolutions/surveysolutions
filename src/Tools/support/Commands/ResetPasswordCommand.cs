using System.ComponentModel;
using System.Threading.Tasks;
using NConsole;
using NLog;

namespace support
{
    [Description("Reset password for a user.")]
    public class ResetPasswordCommand : ConfigurationDependentCommand, IConsoleCommand
    {
        private readonly IDatabaseService databaseService;
        private readonly ILogger logger;

        public ResetPasswordCommand(
            IConfigurationManagerSettings configurationManagerSettings, 
            ILogger logger, 
            IDatabaseService databaseService) : base(configurationManagerSettings)
        {
            this.logger = logger;
            this.databaseService = databaseService;
        }

        [Description("Login (user name) to reset password.")]
        [Argument(Name = "login")]
        public string Login { get; set; }

        [Description("New user's password.")]
        [Argument(Name = "new-password")]
        public string NewPassword { get; set; }

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            if (!ReadConfigurationFile(host))
                return null;

            var passwordHash = HashPassword(NewPassword);

            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                host.WriteMessage($"Reset password for user {Login}: ");
                await host.TryExecuteActionWithAnimationAsync(logger, databaseService.UpdatePasswordAsync(ConnectionString, Login, passwordHash));
            }
            else
                host.WriteMessage("Connection string not found");

            return null;
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new Microsoft.AspNet.Identity.PasswordHasher();
            return passwordHasher.HashPassword(password);
        }
    }
}
