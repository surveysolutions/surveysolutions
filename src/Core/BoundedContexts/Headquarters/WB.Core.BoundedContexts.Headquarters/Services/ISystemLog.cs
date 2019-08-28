﻿using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISystemLog
    {
        void ExportStared(string processName, DataExportFormat format);
        void QuestionnaireDeleted(string title, QuestionnaireIdentity questionnaire);
        void QuestionnaireImported(string title, QuestionnaireIdentity questionnaire);
        void UserCreated(UserRoles role, string userName);
        void AssignmentSizeChanged(int id, int? quantity);
        void ExportEncryptionChanged(bool enabled);
        void UserMovedToAnotherTeam(string interviewerName, string newSupervisorName, string previousSupervisorName);
        void AssignmentsUpgradeStarted(string title, long fromVersion, long toVersion);
        void EmailProviderWasChanged(string previousProvider, string currentProvider);
        void UsersImported(int importedSupervisors, int importedInterviewers);
        void AssignmentsImported(long assignmentsCount, string questionnaireTitle, long questionnaireVersion,
            int firstAssignmentId, int lastAssignmentId);
        void InterviewerArchived(string interviewerName);
        void InterviewerUnArchived(string interviewerName);
        void SupervisorArchived(string supervisorName);
        void SupervisorUnArchived(string supervisorName);
    }
}
