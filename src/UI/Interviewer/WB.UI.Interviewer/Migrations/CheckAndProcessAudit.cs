using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Interviewer.Migrations
{
    [Migration(201904081211)]
    public class CheckAndProcessAudit : IMigration
    {
        private readonly IAudioAuditService auditService;

        public CheckAndProcessAudit(IAudioAuditService auditService)
        {
            this.auditService = auditService;
        }

        public void Up()
        {
            auditService.CheckAndProcessAllAuditFiles();
        }
    }
}
