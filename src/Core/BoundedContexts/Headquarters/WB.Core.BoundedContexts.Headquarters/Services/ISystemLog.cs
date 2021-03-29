using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISystemLog
    {
        void ExportStared(string processName, DataExportFormat format);
        void QuestionnaireDeleted(string title, QuestionnaireIdentity questionnaire);
        void QuestionnaireImported(string title, QuestionnaireIdentity questionnaire, Guid userId, string userName);
        void UserCreated(UserRoles role, string userName);
        void UserPasswordChanged(string currentUser, string userName);
        void UserPasswordChangeFailed(string currentUser, string userName);
        void AssignmentSizeChanged(int id, int? quantity);
        void ExportEncryptionChanged(bool enabled);
        void UserMovedToAnotherTeam(string interviewerName, string newSupervisorName, string previousSupervisorName);
        void AssignmentsUpgradeStarted(string title, long fromVersion, long toVersion, Guid userId, string userName);
        void EmailProviderWasChanged(string previousProvider, string currentProvider);
        void UsersImported(int importedSupervisors, int importedInterviewers, string responsible);
        void AssignmentsImported(long assignmentsCount, string questionnaireTitle, long questionnaireVersion,
            int firstAssignmentId, int lastAssignmentId, string responsibleName);
        void InterviewerArchived(string interviewerName);
        void InterviewerUnArchived(string interviewerName);
        void SupervisorArchived(string supervisorName);
        void SupervisorUnArchived(string supervisorName);
        void WorkspaceCreated(string workspaceName, string displayName);
        void WorkspaceUpdated(string workspaceName, string oldName, string newName);
        void WorkspaceDeleted(string workspaceName);
        void WorkspaceEnabled(string workspaceName);
        void WorkspaceDisabled(string workspaceName);
        void WorkspaceUserAssigned(string userName, ICollection<string> workspaces);
        void WorkspaceUserUnAssigned(string userName, ICollection<string> workspaces);
    }
}
