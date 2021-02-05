namespace WB.Core.BoundedContexts.Headquarters.Views.SystemLog
{
    public enum LogEntryType
    {
        Unknown = 0,
        QuestionnaireImported = 1,
        QuestionnaireDeleted = 2,
        ExportStarted = 3,
        AssignmentsUpgradeStarted = 4,
        UserCreated = 5,
        AssignmentSizeChanged = 6,
        ExportEncryptionChanged = 7,
        UserMovedToAnotherTeam = 8,
        EmailProviderChanged = 9,
        UsersImported = 10,
        AssignmentsImported = 11,
        InterviewerArchived = 12,
        InterviewerUnArchived = 13,
        SupervisorArchived = 14,
        SupervisorUnarchived = 15,
        WorkspaceCreated = 16,
        WorkspaceDeleted = 17,
        WorkspaceDisabled = 18,
        WorkspaceEnabled = 19,
        WorkspaceUserAssigned = 20,
        WorkspaceUserUnAssigned = 21,
        WorkspaceUpdated = 22,
        UserPasswordChanged = 23,
        UserPasswordChangFailed = 24,
    }
}
