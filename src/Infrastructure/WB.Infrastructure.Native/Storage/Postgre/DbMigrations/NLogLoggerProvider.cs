using Microsoft.Extensions.Logging;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class NLogLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger(categoryName);

            return new NLogLogger(logger);
        }

        public void Dispose()
        {
        }
    }
}