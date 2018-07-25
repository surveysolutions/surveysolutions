using System;
using WB.Core.GenericSubdomains.Portable.Services;

namespace Utils.Commands
{
    public class DbMigrator
    {
        private readonly ILogger logger;

        public DbMigrator(ILogger logger)
        {
            this.logger = logger;
        }

        public int Run(string serverName)
        {
            logger.Info($"Migration of db {serverName}");

          
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Finished at {DateTime.Now}");
            return 0;
        }
    }
}
