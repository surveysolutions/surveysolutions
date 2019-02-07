using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    public class AuditLog : IAuditLog
    {
        private readonly ILogger logger;

        public AuditLog(ILoggerProvider logger)
        {
            this.logger = logger.GetFor<AuditLog>();
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

        public void AssignmentsUpgradeStarted(string title, long fromVersion, long toVersion)
        {
            this.Append("Assignments", "Upgrade", $"From (ver. {fromVersion}) to (ver. {toVersion}) {title}");
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

        public void UserMovedToAnotherTeam(string interviewerName, string newSupervisorName, string previousSupervisorName)
        {
            this.Append($"User {interviewerName}", "moved", $"From team {previousSupervisorName}' to {newSupervisorName}");
        }

        private void Append(string target, string action, string args = null)
        {
            logger.Info($"{target}: {action}; {args ?? string.Empty}");
        }
    }
}
