using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    public class SystemLog : ISystemLog
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInScopeExecutor inScopeExecutor;

        public SystemLog(IAuthorizedUser authorizedUser, IInScopeExecutor inScopeExecutor)
        {
            this.authorizedUser = authorizedUser;
            this.inScopeExecutor = inScopeExecutor;
        }

        public void ExportStared(string processName, DataExportFormat format)
        {
            this.Append(LogEntryType.ExportStarted, processName, "exported", format.ToString());
        }

        public void QuestionnaireDeleted(string title, QuestionnaireIdentity questionnaire)
        {
            this.Append(LogEntryType.QuestionnaireDeleted, $"(ver. {questionnaire.Version}) {title}", "deleted");
        }

        public void QuestionnaireImported(string title, QuestionnaireIdentity questionnaire, Guid userId, string importUserName)
        {
            this.Append(LogEntryType.QuestionnaireImported, $"(ver. {questionnaire.Version}) {title}", "imported",
                responsibleName: importUserName,
                responsibleUserId: userId);
        }

        public void AssignmentsUpgradeStarted(string title, long fromVersion, long toVersion, Guid userId, string userName)
        {
            this.Append(LogEntryType.AssignmentsUpgradeStarted, "Assignments", "Upgrade",
                $"From (ver. {fromVersion}) to (ver. {toVersion}) {title}",
                responsibleName: userName,
                responsibleUserId: userId);
        }

        public void EmailProviderWasChanged(string previousProvider, string currentProvider)
        {
            this.Append(LogEntryType.EmailProviderChanged, "Update", $"Previous provider was {previousProvider}, current provider is {currentProvider}");
        }

        public void UsersImported(int importedSupervisors, int importedInterviewers, string responsibleName)
            => this.Append(LogEntryType.UsersImported, "Users", "Import",
                $"User {responsibleName} created {importedSupervisors + importedInterviewers} users in batch mode, " +
                $"of which {importedInterviewers} are interviewers and {importedSupervisors} supervisors",
                responsibleName: responsibleName);

        public void AssignmentsImported(long assignmentsCount, string questionnaireTitle, long questionnaireVersion,
            int firstAssignmentId, int lastAssignmentId, string responsibleName)
            => this.Append(LogEntryType.AssignmentsImported, "Assignments", "Import",
                $"User {responsibleName} created {assignmentsCount} assignment(s) {firstAssignmentId}-{lastAssignmentId} " +
                $"for {questionnaireTitle} (v{questionnaireVersion})",
                responsibleName: responsibleName);

        public void InterviewerArchived(string interviewerName)
            => this.Append(LogEntryType.InterviewerArchived, "Interviewer", "Archive",
                $"User {this.authorizedUser.UserName} has archived interviewer account {interviewerName}");

        public void InterviewerUnArchived(string interviewerName)
            => this.Append(LogEntryType.InterviewerUnArchived, "Interviewer", "Unarhive",
                $"User {this.authorizedUser.UserName} has unarchived interviewer account {interviewerName}");

        public void SupervisorArchived(string supervisorName)
            => this.Append(LogEntryType.SupervisorArchived, "Supervisor", "Archive",
                $"User {this.authorizedUser.UserName} has archived interviewer account {supervisorName}");

        public void SupervisorUnArchived(string supervisorName)
            => this.Append(LogEntryType.SupervisorUnarchived, "Supervisor", "Unarhive",
                $"User {this.authorizedUser.UserName} has unarchived interviewer account {supervisorName}");

        public void UserCreated(UserRoles role, string userName)
        {
            this.Append(LogEntryType.UserCreated, $"{role} user '{userName}'", "created");
        }

        public void AssignmentSizeChanged(int id, int? quantity)
        {
            this.Append(LogEntryType.AssignmentSizeChanged, $"Assignment {id}", "size changed", $"{quantity ?? -1}");
        }

        public void ExportEncryptionChanged(bool enabled)
        {
            this.Append(LogEntryType.ExportEncryptionChanged, "Export encryption", "changed", $"{(enabled ? "enabled" : "disabled")}'");
        }

        public void UserMovedToAnotherTeam(string interviewerName, string newSupervisorName, string previousSupervisorName)
        {
            this.Append(LogEntryType.UserMovedToAnotherTeam, $"User {interviewerName}", "moved", $"From team {previousSupervisorName}' to {newSupervisorName}");
        }

        private void Append(LogEntryType type, string target, string action, string args = null, string responsibleName = null, Guid? responsibleUserId = null)
        {
            AppendLogEntry(responsibleUserId ?? this.authorizedUser.Id,
                responsibleName ?? this.authorizedUser.UserName,
                type,
                $"{target}: {action}; {args ?? string.Empty}");
        }

        private void AppendLogEntry(Guid? userid, string userName, LogEntryType type, string log)
        {
            inScopeExecutor.Execute(serviceLocator =>
            {
                var logEntry = new SystemLogEntry
                {
                    Type = type,
                    UserName = userName,
                    UserId = userid,
                    LogDate = DateTime.UtcNow,
                    Log = log
                };

                var systemLogStorage = serviceLocator.GetInstance<IPlainStorageAccessor<SystemLogEntry>>();
                try
                {
                    systemLogStorage.Store(logEntry, logEntry.Id);
                }
                catch (Exception e)
                {
                    serviceLocator.GetInstance<ILogger>().Error("Error on system log writing", e);
                }
            });
        }
    }
}
