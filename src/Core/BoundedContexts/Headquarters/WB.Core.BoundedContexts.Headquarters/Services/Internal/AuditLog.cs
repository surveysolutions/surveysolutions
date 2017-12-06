using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    public class AuditLog : IAuditLog
    {
        private readonly ILogger logger;

        public AuditLog(ILogger logger)
        {
            this.logger = logger;
        }

        public void ExportStared(string processName, DataExportFormat format)
        {
            this.Append(processName, "exported", format.ToString());
        }

        public void QuestionnaireDeleted(string title, QuestionnaireIdentity questionnaire)
        {
            this.Append($"(ver. {questionnaire.Version}) {title}", "deleted");
        }

        public void QuestionnaireImported(string title, QuestionnaireIdentity questionnaire)
        {
            this.Append($"(ver. {questionnaire.Version}) {title}", "imported");
        }

        public void UserCreated(UserRoles role, string userName)
        {
            this.Append($"{role} user '{userName}'", "created");
        }

        public void AssignmentSizeChanged(int id, int? quantity)
        {
            this.Append($"Assignment {id}", "size changed", $"{quantity ?? -1}");
        }

        public void ExportEncriptionChanged(bool enabled)
        {
            this.Append("Export encription","changed", $"{(enabled ? "enabled" : "disabled")}'");
        }

        private void Append(string target, string action, string args = null)
        {
            logger.Info($"{target}: {action}; {args ?? string.Empty}");
        }
    }
}