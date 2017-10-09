using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    public class AuditLog : IAuditLog
    {
        private readonly ILogger logger;

        public AuditLog(ILogger logger)
        {
            this.logger = logger;
        }

        public void Append(string message)
        {
            logger.Info(message);
        }
    }
}